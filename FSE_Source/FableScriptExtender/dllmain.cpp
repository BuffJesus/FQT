/**
 * @file dllmain.cpp
 * @brief Fable Script Extender (FSE) - Main DLL entry point and memory hooking system.
 *
 * This file is the core of the Fable Script Extender modding framework. It implements:
 *
 * 1. DLL INJECTION SYSTEM
 *    - DllMain: Entry point when FSE_Launcher.exe injects this DLL into Fable.exe
 *    - InstallHook: Patches game memory at 0xCDB355 to redirect to our custom code
 *    - MyHook: Naked assembly trampoline that preserves registers and FPU state
 *
 * 2. SCRIPT REGISTRATION SYSTEM
 *    - Reads quests.lua configuration file to discover custom quests
 *    - Creates CScriptInfo structures for each quest to register with Fable's script manager
 *    - Uses template-based allocator pools (QuestAllocator<N>, EntityAllocator<N>) to
 *      instantiate LuaQuestHost and LuaEntityHost objects when the game needs them
 *
 * 3. MEMORY MANAGEMENT
 *    - Custom allocator pools handle quest and entity script instantiation
 *    - VTable arrays (g_LuaQuestHostVTable, g_LuaEntityHostVTable) define C++ virtual
 *      function tables that match Fable's expected interface layout
 *    - Reference counting system for entity hosts with custom deleter
 *
 * ARCHITECTURE OVERVIEW:
 * When Fable loads a level, it calls the registered CScriptInfo::pAllocFunc for each
 * active quest. This triggers QuestAllocator<N> which creates a LuaQuestHost instance.
 * The LuaQuestHost then manages its Lua script lifecycle (Init, Main, OnPersist) and
 * can spawn LuaEntityHost instances for entity scripts via EntityAllocator<N>.
 *
 * @note This code uses assembly-level hooking and reverse-engineered Fable structures.
 *       Memory addresses are ASLR-adjusted using the ASLR<T> template.
 *
 * @see LuaQuestHost - Lua wrapper for quest scripts
 * @see LuaEntityHost - Lua wrapper for entity scripts
 * @see LuaManager - Singleton managing Lua VM lifecycle
 * @see GameInterface - Direct access to Fable engine functions
 */
#include "FableAPI.h"
#include "GameInterface.h"
#include "LuaManager.h"
#include "LuaQuestHost.h"
#include "LuaEntityHost.h"
#include "LuaQuestState.h"
#include <Windows.h>
#include <vector>
#include <list>
#include <algorithm>
#include <sstream>
#include <Shlwapi.h>
#include <direct.h>

/** @brief Flag indicating if the memory hook has been installed */
static bool g_bHookInitialized = false;
static bool g_bScriptsRegisteredWithGame = false;
static int g_loadCount = 0;
struct EntityScriptDefinition { std::string name, file; int id; };
struct QuestDefinition { std::string name, file; int id; std::vector<EntityScriptDefinition> entityScripts; };
std::vector<QuestDefinition> g_questDefinitions;
std::vector<std::string> g_questScriptFileNames;
std::vector<std::string> g_entityScriptFileNames;
std::vector<CScriptInfo> g_scriptInfos;
std::list<FableString> g_scriptStringRegistry;
constexpr int MAX_TOTAL_SCRIPTS = 500;
constexpr int MAX_QUEST_SCRIPTS = 100;
constexpr int MAX_ENTITY_SCRIPTS = 300;

void __fastcall CustomEntityHostDeleter(void* pHostRaw) {
    if (pHostRaw) {
        LuaEntityHost* pHost = static_cast<LuaEntityHost*>(pHostRaw);

        LogToFile("    [CustomEntityHostDeleter] Deleting LuaEntityHost at 0x" + std::to_string(reinterpret_cast<uintptr_t>(pHost)));

        delete pHost;
    }
}

template<int N>
void* __fastcall QuestAllocator(CScriptDataBase* pData, CGameScriptInterfaceBase* pInterface) {
    if (N >= g_questScriptFileNames.size()) return nullptr;
    const std::string& scriptName = g_questScriptFileNames[N];
    return static_cast<void*>(&(new LuaQuestHost(pData, pInterface, scriptName))->base);
}

template<int N>
void* __fastcall EntityAllocator(void* a1, CScriptBase_Retail* parent, CScriptDataBase* pdata, const CScriptThing* thing) {
    LogToFile("[EntityAllocator<" + std::to_string(N) + ">] === ENTRY ===");

    if (N >= g_entityScriptFileNames.size()) {
        LogToFile("[EntityAllocator<" + std::to_string(N) + ">] ERROR: Index out of range");
        return nullptr;
    }

    const std::string& scriptName = g_entityScriptFileNames[N];
    LogToFile("[EntityAllocator<" + std::to_string(N) + ">] Script: " + scriptName);

    LuaQuestHost* pParentHost = reinterpret_cast<LuaQuestHost*>(parent);

    LogToFile("[EntityAllocator<" + std::to_string(N) + ">] Creating LuaEntityHost...");

    LuaEntityHost* pNewEntityHost = new LuaEntityHost(pdata, pParentHost, thing, scriptName);

    std::stringstream ss;
    ss << "[EntityAllocator<" + std::to_string(N) + ">] LuaEntityHost created: this = 0x" << std::hex << (DWORD)pNewEntityHost;
    LogToFile(ss.str());

    int* pResult = static_cast<int*>(a1);
    *pResult = (int)pNewEntityHost;

    if (pNewEntityHost) {
        LogToFile("[EntityAllocator<" + std::to_string(N) + ">] Allocating RefCount block...");
        DWORD* pMgmtBlock = (DWORD*)Game_malloc(0x0C);

        if (pMgmtBlock) {
            pResult[1] = (int)pMgmtBlock;

            *pMgmtBlock = 2;

            pMgmtBlock[1] = (DWORD)&CustomEntityHostDeleter;

            pMgmtBlock[2] = (DWORD)pNewEntityHost;
            LogToFile("[EntityAllocator<" + std::to_string(N) + ">] RefCount block created successfully (using CustomEntityHostDeleter).");
        }
        else {
            LogToFile("[EntityAllocator<" + std::to_string(N) + ">] ERROR: RefCount malloc failed!");
            pResult[1] = 0;
        }
    }
    else {
        LogToFile("[EntityAllocator<" + std::to_string(N) + ">] ERROR: LuaEntityHost construction failed!");
        pResult[1] = 0;
    }

    LogToFile("[EntityAllocator<" + std::to_string(N) + ">] === EXIT ===");
    return a1;
}

tScriptAllocFunc g_questAllocatorPool[MAX_QUEST_SCRIPTS] = { &QuestAllocator<0>,    &QuestAllocator<1>,  &QuestAllocator<2>,
&QuestAllocator<3>,  &QuestAllocator<4>,  &QuestAllocator<5>,  &QuestAllocator<6>,  &QuestAllocator<7>,  &QuestAllocator<8>,
&QuestAllocator<9>,  &QuestAllocator<10>, &QuestAllocator<11>, &QuestAllocator<12>, &QuestAllocator<13>, &QuestAllocator<14>,
&QuestAllocator<15>, &QuestAllocator<16>, &QuestAllocator<17>, &QuestAllocator<18>, &QuestAllocator<19>, &QuestAllocator<20>,
&QuestAllocator<21>, &QuestAllocator<22>, &QuestAllocator<23>, &QuestAllocator<24>, &QuestAllocator<25>, &QuestAllocator<26>,
&QuestAllocator<27>, &QuestAllocator<28>, &QuestAllocator<29>, &QuestAllocator<30>, &QuestAllocator<31>, &QuestAllocator<32>,
&QuestAllocator<33>, &QuestAllocator<34>, &QuestAllocator<35>, &QuestAllocator<36>, &QuestAllocator<37>, &QuestAllocator<38>,
&QuestAllocator<39>, &QuestAllocator<40>, &QuestAllocator<41>, &QuestAllocator<42>, &QuestAllocator<43>, &QuestAllocator<44>,
&QuestAllocator<45>, &QuestAllocator<46>, &QuestAllocator<47>, &QuestAllocator<48>, &QuestAllocator<49>, &QuestAllocator<50>,
&QuestAllocator<51>, &QuestAllocator<52>, &QuestAllocator<53>, &QuestAllocator<54>, &QuestAllocator<55>, &QuestAllocator<56>,
&QuestAllocator<57>, &QuestAllocator<58>, &QuestAllocator<59>, &QuestAllocator<60>, &QuestAllocator<61>, &QuestAllocator<62>,
&QuestAllocator<63>, &QuestAllocator<64>, &QuestAllocator<65>, &QuestAllocator<66>, &QuestAllocator<67>, &QuestAllocator<68>,
&QuestAllocator<69>, &QuestAllocator<70>, &QuestAllocator<71>, &QuestAllocator<72>, &QuestAllocator<73>, &QuestAllocator<74>,
&QuestAllocator<75>, &QuestAllocator<76>, &QuestAllocator<77>, &QuestAllocator<78>, &QuestAllocator<79>, &QuestAllocator<80>,
&QuestAllocator<81>, &QuestAllocator<82>, &QuestAllocator<83>, &QuestAllocator<84>, &QuestAllocator<85>, &QuestAllocator<86>,
&QuestAllocator<87>, &QuestAllocator<88>, &QuestAllocator<89>, &QuestAllocator<90>, &QuestAllocator<91>, &QuestAllocator<92>,
&QuestAllocator<93>, &QuestAllocator<94>, &QuestAllocator<95>, &QuestAllocator<96>, &QuestAllocator<97>, &QuestAllocator<98>,
&QuestAllocator<99> };

tEntityScriptAllocFunc g_entityAllocatorPool[MAX_ENTITY_SCRIPTS] = {
    &EntityAllocator<0>, &EntityAllocator<1>, &EntityAllocator<2>, &EntityAllocator<3>, &EntityAllocator<4>, &EntityAllocator<5>,
    &EntityAllocator<6>, &EntityAllocator<7>, &EntityAllocator<8>, &EntityAllocator<9>, &EntityAllocator<10>, &EntityAllocator<11>,
    &EntityAllocator<12>,&EntityAllocator<13>,&EntityAllocator<14>,&EntityAllocator<15>,&EntityAllocator<16>, &EntityAllocator<17>,
    &EntityAllocator<18>,&EntityAllocator<19>,&EntityAllocator<20>,&EntityAllocator<21>,&EntityAllocator<22>, &EntityAllocator<23>,
    &EntityAllocator<24>,&EntityAllocator<25>,&EntityAllocator<26>,&EntityAllocator<27>,&EntityAllocator<28>, &EntityAllocator<29>,  
    &EntityAllocator<30>,&EntityAllocator<31>,&EntityAllocator<32>,&EntityAllocator<33>,&EntityAllocator<34>, &EntityAllocator<35>,  
    &EntityAllocator<36>,&EntityAllocator<37>,&EntityAllocator<38>,&EntityAllocator<39>,&EntityAllocator<40>, &EntityAllocator<41>,
    &EntityAllocator<42>,&EntityAllocator<43>,&EntityAllocator<44>,&EntityAllocator<45>,&EntityAllocator<46>, &EntityAllocator<47>,
    &EntityAllocator<48>,&EntityAllocator<49>, &EntityAllocator<50>, &EntityAllocator<51>, &EntityAllocator<52>, &EntityAllocator<53>,
    &EntityAllocator<54>, &EntityAllocator<55>, &EntityAllocator<56>, &EntityAllocator<57>, &EntityAllocator<58>, &EntityAllocator<59>,
    &EntityAllocator<60>, &EntityAllocator<61>, &EntityAllocator<62>, &EntityAllocator<63>, &EntityAllocator<64>, &EntityAllocator<65>,
    &EntityAllocator<66>, &EntityAllocator<67>, &EntityAllocator<68>, &EntityAllocator<69>, &EntityAllocator<70>, &EntityAllocator<71>,
    &EntityAllocator<72>, &EntityAllocator<73>, &EntityAllocator<74>, &EntityAllocator<75>, &EntityAllocator<76>, &EntityAllocator<77>,
    &EntityAllocator<78>, &EntityAllocator<79>, &EntityAllocator<80>, &EntityAllocator<81>, &EntityAllocator<82>, &EntityAllocator<83>,
    &EntityAllocator<84>, &EntityAllocator<85>, &EntityAllocator<86>, &EntityAllocator<87>, &EntityAllocator<88>, &EntityAllocator<89>,
    &EntityAllocator<90>, &EntityAllocator<91>, &EntityAllocator<92>, &EntityAllocator<93>, &EntityAllocator<94>, &EntityAllocator<95>,
    &EntityAllocator<96>, &EntityAllocator<97>, &EntityAllocator<98>, &EntityAllocator<99>, &EntityAllocator<100>, &EntityAllocator<101>,
    &EntityAllocator<102>, &EntityAllocator<103>, &EntityAllocator<104>, &EntityAllocator<105>, &EntityAllocator<106>, &EntityAllocator<107>,
    &EntityAllocator<108>, &EntityAllocator<109>, &EntityAllocator<110>, &EntityAllocator<111>, &EntityAllocator<112>, &EntityAllocator<113>,
    &EntityAllocator<114>, &EntityAllocator<115>, &EntityAllocator<116>, &EntityAllocator<117>, &EntityAllocator<118>, &EntityAllocator<119>,
    &EntityAllocator<120>, &EntityAllocator<121>, &EntityAllocator<122>, &EntityAllocator<123>, &EntityAllocator<124>, &EntityAllocator<125>,
    &EntityAllocator<126>, &EntityAllocator<127>, &EntityAllocator<128>, &EntityAllocator<129>, &EntityAllocator<130>, &EntityAllocator<131>,
    &EntityAllocator<132>, &EntityAllocator<133>, &EntityAllocator<134>, &EntityAllocator<135>, &EntityAllocator<136>, &EntityAllocator<137>,
    &EntityAllocator<138>, &EntityAllocator<139>, &EntityAllocator<140>, &EntityAllocator<141>, &EntityAllocator<142>, &EntityAllocator<143>,
    &EntityAllocator<144>, &EntityAllocator<145>, &EntityAllocator<146>, &EntityAllocator<147>, &EntityAllocator<148>, &EntityAllocator<149>,
    &EntityAllocator<150>, &EntityAllocator<151>, &EntityAllocator<152>, &EntityAllocator<153>, &EntityAllocator<154>, &EntityAllocator<155>,
    &EntityAllocator<156>, &EntityAllocator<157>, &EntityAllocator<158>, &EntityAllocator<159>, &EntityAllocator<160>, &EntityAllocator<161>,
    &EntityAllocator<162>, &EntityAllocator<163>, &EntityAllocator<164>, &EntityAllocator<165>, &EntityAllocator<166>, &EntityAllocator<167>,
    &EntityAllocator<168>, &EntityAllocator<169>, &EntityAllocator<170>, &EntityAllocator<171>, &EntityAllocator<172>, &EntityAllocator<173>,
    &EntityAllocator<174>, &EntityAllocator<175>, &EntityAllocator<176>, &EntityAllocator<177>, &EntityAllocator<178>, &EntityAllocator<179>,
    &EntityAllocator<180>, &EntityAllocator<181>, &EntityAllocator<182>, &EntityAllocator<183>, &EntityAllocator<184>, &EntityAllocator<185>,
    &EntityAllocator<186>, &EntityAllocator<187>, &EntityAllocator<188>, &EntityAllocator<189>, &EntityAllocator<190>, &EntityAllocator<191>,
    &EntityAllocator<192>, &EntityAllocator<193>, &EntityAllocator<194>, &EntityAllocator<195>, &EntityAllocator<196>, &EntityAllocator<197>,
    &EntityAllocator<198>, &EntityAllocator<199>, &EntityAllocator<200>, &EntityAllocator<201>, &EntityAllocator<202>, &EntityAllocator<203>,
    &EntityAllocator<204>, &EntityAllocator<205>, &EntityAllocator<206>, &EntityAllocator<207>, &EntityAllocator<208>, &EntityAllocator<209>,
    &EntityAllocator<210>, &EntityAllocator<211>, &EntityAllocator<212>, &EntityAllocator<213>, &EntityAllocator<214>, &EntityAllocator<215>,
    &EntityAllocator<216>, &EntityAllocator<217>, &EntityAllocator<218>, &EntityAllocator<219>, &EntityAllocator<220>, &EntityAllocator<221>,
    &EntityAllocator<222>, &EntityAllocator<223>, &EntityAllocator<224>, &EntityAllocator<225>, &EntityAllocator<226>, &EntityAllocator<227>,
    &EntityAllocator<228>, &EntityAllocator<229>, &EntityAllocator<230>, &EntityAllocator<231>, &EntityAllocator<232>, &EntityAllocator<233>,
    &EntityAllocator<234>, &EntityAllocator<235>, &EntityAllocator<236>, &EntityAllocator<237>, &EntityAllocator<238>, &EntityAllocator<239>,
    &EntityAllocator<240>, &EntityAllocator<241>, &EntityAllocator<242>, &EntityAllocator<243>, &EntityAllocator<244>, &EntityAllocator<245>,
    &EntityAllocator<246>, &EntityAllocator<247>, &EntityAllocator<248>, &EntityAllocator<249>, &EntityAllocator<250>, &EntityAllocator<251>,
    &EntityAllocator<252>, &EntityAllocator<253>, &EntityAllocator<254>, &EntityAllocator<255>, &EntityAllocator<256>, &EntityAllocator<257>,
    &EntityAllocator<258>, &EntityAllocator<259>, &EntityAllocator<260>, &EntityAllocator<261>, &EntityAllocator<262>, &EntityAllocator<263>,
    &EntityAllocator<264>, &EntityAllocator<265>, &EntityAllocator<266>, &EntityAllocator<267>, &EntityAllocator<268>, &EntityAllocator<269>,
    &EntityAllocator<270>, &EntityAllocator<271>, &EntityAllocator<272>, &EntityAllocator<273>, &EntityAllocator<274>, &EntityAllocator<275>,
    &EntityAllocator<276>, &EntityAllocator<277>, &EntityAllocator<278>, &EntityAllocator<279>, &EntityAllocator<280>, &EntityAllocator<281>,
    &EntityAllocator<282>, &EntityAllocator<283>, &EntityAllocator<284>, &EntityAllocator<285>, &EntityAllocator<286>, &EntityAllocator<287>,
    &EntityAllocator<288>, &EntityAllocator<289>, &EntityAllocator<290>, &EntityAllocator<291>, &EntityAllocator<292>, &EntityAllocator<293>,
    &EntityAllocator<294>, &EntityAllocator<295>, &EntityAllocator<296>, &EntityAllocator<297>, &EntityAllocator<298>, &EntityAllocator<299>
};

tEntityScriptAllocFunc GetEntityAllocatorForScript(const std::string& scriptName) {
    auto it = std::find(g_entityScriptFileNames.begin(), g_entityScriptFileNames.end(), scriptName);
    if (it != g_entityScriptFileNames.end()) {
        size_t index = std::distance(g_entityScriptFileNames.begin(), it);
        if (index < MAX_ENTITY_SCRIPTS) {
            return g_entityAllocatorPool[index];
        }
    }
    return nullptr;
}

void* g_LuaQuestHostVTable[] = {
    GetMemberFunctionAddress(&LuaQuestHost::Destructor),
    GetMemberFunctionAddress(&LuaQuestHost::RegisterMain),
    GetMemberFunctionAddress(&LuaQuestHost::Main),
    GetMemberFunctionAddress(&LuaQuestHost::Init),
    GetMemberFunctionAddress(&LuaQuestHost::OnPersist)
};

void* g_LuaEntityHostVTable[] = {
    GetMemberFunctionAddress(&LuaEntityHost::Destructor),
    GetMemberFunctionAddress(&LuaEntityHost::Main),
    GetMemberFunctionAddress(&LuaEntityHost::Init),
    GetMemberFunctionAddress(&LuaEntityHost::GetParentScript_Stub),
    GetMemberFunctionAddress(&LuaEntityHost::OnPersist),
    GetMemberFunctionAddress(&LuaEntityHost::OnPredicateFail_Stub),
    GetMemberFunctionAddress(&LuaEntityHost::OnInterrupted_Stub)
};

void InjectCustomScripts() {
    void* pMan;
    __asm { mov pMan, esi }

    g_loadCount++;
    LogToFile("--- Load count: " + std::to_string(g_loadCount) + " ---");

    InitializeFableAPI();

    if (!g_pDSTGame || !*g_pDSTGame) {
        LogToFile("!!! CRITICAL: Could not initialize systems. g_pDSTGame is null. !!!");
        return;
    }

    InitializeGameInterface(*g_pDSTGame);

    if (g_loadCount == 1) {
        LuaManager::GetInstance().Initialize();
    }
    else {
        LuaManager::GetInstance().Reinitialize();
    }

    LogToFile("--- Lua systems initialized successfully. ---");

    try {
        LogToFile("--- Reading quests.lua definitions... ---");

        sol::state lua;
        lua.open_libraries(sol::lib::base, sol::lib::string, sol::lib::table);

        std::string questsConfigPath = GetQuestsConfigPath();
        LogToFile("    Loading config from: " + questsConfigPath);
        lua.script_file(questsConfigPath);

        sol::table qTable = lua["Quests"];
        if (!qTable) {
            LogToFile("!!! ERROR: Could not find 'Quests' table in quests.lua");
            return;
        }

        g_questDefinitions.clear();
        g_questScriptFileNames.clear();
        g_entityScriptFileNames.clear();

        for (auto kvp : qTable) {
            sol::table qd = kvp.second;
            g_questDefinitions.emplace_back(QuestDefinition{
                qd["name"], qd["file"], qd["id"]
                });
            sol::table es = qd["entity_scripts"];
            if (es) {
                for (auto es_kvp : es) {
                    sol::table ed = es_kvp.second;
                    g_questDefinitions.back().entityScripts.emplace_back(
                        EntityScriptDefinition{ ed["name"], ed["file"], ed["id"] }
                    );
                }
            }
        }

        std::sort(g_questDefinitions.begin(), g_questDefinitions.end(),
            [](const QuestDefinition& a, const QuestDefinition& b) {
                return a.id < b.id;
            });

        for (auto& q : g_questDefinitions) {
            g_questScriptFileNames.push_back(q.file);
            for (auto& es : q.entityScripts) {
                g_entityScriptFileNames.push_back(es.file);
            }
        }
        LogToFile("--- Quests.lua definitions read and sorted. ---");

    }
    catch (const std::exception& e) {
        std::string errorMsg = "C++ EXCEPTION in InjectCustomScripts (reading quests.lua): " + std::string(e.what());
        LogToFile(errorMsg);
        MessageBoxA(NULL, errorMsg.c_str(), "FSE LUA ERROR", MB_OK | MB_ICONERROR);
        return;
    }

    if (!g_bScriptsRegisteredWithGame)
    {
        LogToFile("--- First-time script registration (populating g_scriptInfos)... ---");
        try {
            g_scriptInfos.reserve(g_questScriptFileNames.size());

            size_t questIdx = 0;
            for (auto& q : g_questDefinitions) {
                if (questIdx >= MAX_QUEST_SCRIPTS) break;
                LogToFile("    Registering quest '" + q.name + "'...");

                std::string sectionName = "S_" + q.name;
                FableString& sectionStr = g_scriptStringRegistry.emplace_back(sectionName.c_str());
                FableString& scriptNameStr = g_scriptStringRegistry.emplace_back(q.name.c_str());

                CScriptInfo newScriptInfo = { 0 };
                newScriptInfo.pAllocFunc = g_questAllocatorPool[questIdx++];
                newScriptInfo.pAllocDataFunc = pSunnyvaleDataAlloc_func;
                newScriptInfo.ID = q.id;
                newScriptInfo.MasterScript = false;
                CCharString_Construct_Copy(&newScriptInfo.Name, scriptNameStr);

                g_scriptInfos.push_back(newScriptInfo);
                CScriptInfo& newScriptInfoInVector = g_scriptInfos.back();

                CheckSection(pMan, sectionStr);
                AddScript(pMan, &newScriptInfoInVector, sectionStr);
                SetScriptActiveStatus_Func(&newScriptInfoInVector.Name, ESAS_UNSTARTED);
            }
            g_bScriptsRegisteredWithGame = true;
            LogToFile("--- Custom scripts registered with the game. ---");
        }
        catch (const std::exception& e) {
            LogToFile("C++ EXCEPTION in InjectCustomScripts (First-time registration): " + std::string(e.what()));
        }
    }
    else
    {
        LogToFile("--- Re-registering scripts (re-creating all strings)... ---");
        try {
            size_t questIdx = 0;
            for (auto& scriptInfo : g_scriptInfos) {

                const std::string& scriptName = g_questDefinitions[questIdx].name;
                const std::string sectionName = "S_" + scriptName;

                CCharString_Destroy(&scriptInfo.Name);
                CCharString_Construct_Literal(&scriptInfo.Name, scriptName.c_str(), -1);

                FableString tempSectionStr(sectionName.c_str());

                AddScript(pMan, &scriptInfo, tempSectionStr);
                SetScriptActiveStatus_Func(&scriptInfo.Name, ESAS_UNSTARTED);

                questIdx++;
            }
            LogToFile("--- Scripts re-added to manager. Activation should now work. ---");
        }
        catch (const std::exception& e) {
            LogToFile("C++ EXCEPTION in InjectCustomScripts (Re-registering): " + std::string(e.what()));
        }
    }
}

void __declspec(naked) MyHook() {
    __asm {
        pushad
        sub esp, 28h
        fstenv[esp]
        call InjectCustomScripts
        fldenv[esp]
        add esp, 28h
        popad
        pop edi
        pop esi
        pop ebp
        pop ebx
        add esp, 18h
        retn
    }
}

void InstallHook() {
    InitializeFableAPI();
    DWORD hA = ASLR<DWORD>(0xCDB355);
    DWORD hFA = (DWORD)&MyHook;
    DWORD rO = hFA - hA - 5;
    BYTE p[5] = { 0xE9, 0, 0, 0, 0 };
    memcpy(p + 1, &rO, 4);
    DWORD oP;
    if (VirtualProtect((LPVOID)hA, 5, PAGE_EXECUTE_READWRITE, &oP)) {
        memcpy((void*)hA, p, 5);
        VirtualProtect((LPVOID)hA, 5, oP, &oP);
    }
}

BOOL APIENTRY DllMain(HMODULE h, DWORD r, LPVOID lp) {
    if (r == DLL_PROCESS_ATTACH) {
        InitializeFSEPaths(h);

        if (!PathIsDirectoryA(g_fseBasePath.c_str())) {
            _mkdir(g_fseBasePath.c_str());
        }

        std::ofstream(GetLogFilePath(), std::ios::trunc);

        LogToFile("--- Fable Custom Quest DLL Attached ---");

        std::stringstream ss;
        ss << "g_LuaEntityHostVTable = 0x" << std::hex << (DWORD)g_LuaEntityHostVTable;
        LogToFile(ss.str());

        for (int i = 0; i < 7; i++) {
            std::stringstream ss_entry;
            ss_entry << "    [" << i << "] = 0x" << std::hex << (DWORD)g_LuaEntityHostVTable[i];
            LogToFile(ss_entry.str());
        }

        DisableThreadLibraryCalls(h);
        InstallHook();
    }
    return TRUE;
}