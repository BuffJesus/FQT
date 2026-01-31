#include "LuaQuestHost.h"
#include "LuaQuestState.h"
#include "LuaManager.h"

constexpr int MAX_QUEST_THREADS = 20;
typedef void(LuaQuestHost::* tThreadRunner)();
tThreadRunner g_threadRunnerPool[MAX_QUEST_THREADS] = {
    &LuaQuestHost::ThreadRunner<0>,  &LuaQuestHost::ThreadRunner<1>,  &LuaQuestHost::ThreadRunner<2>,  &LuaQuestHost::ThreadRunner<3>,  &LuaQuestHost::ThreadRunner<4>,
    &LuaQuestHost::ThreadRunner<5>,  &LuaQuestHost::ThreadRunner<6>,  &LuaQuestHost::ThreadRunner<7>,  &LuaQuestHost::ThreadRunner<8>,  &LuaQuestHost::ThreadRunner<9>,
    &LuaQuestHost::ThreadRunner<10>, &LuaQuestHost::ThreadRunner<11>, &LuaQuestHost::ThreadRunner<12>, &LuaQuestHost::ThreadRunner<13>, &LuaQuestHost::ThreadRunner<14>,
    &LuaQuestHost::ThreadRunner<15>, &LuaQuestHost::ThreadRunner<16>, &LuaQuestHost::ThreadRunner<17>, &LuaQuestHost::ThreadRunner<18>, &LuaQuestHost::ThreadRunner<19>
};

LuaQuestHost::LuaQuestHost(CScriptDataBase* data, CGameScriptInterfaceBase* interface, const std::string& scriptName)
    : m_scriptName(scriptName),
    m_pQuestState(nullptr),
    m_pLuaState(std::make_unique<sol::state>()), // Create the isolated state
    m_env(*m_pLuaState, sol::create, m_pLuaState->globals()) // Use the new state to init the env
{
    // Now that the state and env are created, open libs and register bindings
    m_pLuaState->open_libraries(sol::lib::base, sol::lib::string, sol::lib::math, sol::lib::table);
    LuaManager::GetInstance().RegisterBindingsInState(*m_pLuaState);

    // Continue with original constructor logic
    CScriptBase_Construct(&this->base);

    this->base.pVTable = g_LuaQuestHostVTable;
    this->pInterface = interface;
    this->m_pMasterData = static_cast<CQ_SunnyvaleMasterData*>(data);
    this->m_threadCount = 0;
    m_pQuestState = std::make_unique<LuaQuestState>(this, interface);
    LogToFile("    (Quest) LuaQuestHost for '" + scriptName + "' created, owning its state AND isolated Lua VM.");
}

LuaQuestHost::~LuaQuestHost() {}

void LuaQuestHost::SetMasterData(CScriptDataBase* pdata)
{
    m_pMasterData = static_cast<CQ_SunnyvaleMasterData*>(pdata);
}

const std::string& LuaQuestHost::GetScriptName() const {
    return m_scriptName;
}

void LuaQuestHost::Init() {
    LogToFile("--- Quest '" + m_scriptName + "': C++ Init() phase ---");
    try {
        std::string scriptPath = GetScriptPath(m_scriptName);
        LogToFile("    Loading quest script from: " + scriptPath);
        m_pLuaState->script_file(scriptPath, m_env); // <-- USE m_pLuaState

        sol::protected_function luaInit = m_env["Init"];
        if (luaInit.valid()) {
            sol::protected_function_result result = luaInit(this->m_pQuestState.get());
            if (!result.valid()) {
                sol::error err = result;
                LogToFile("!!! LUA RUNTIME ERROR in Init() of '" + m_scriptName + "': " + std::string(err.what()));
            }
        }
    }
    catch (const std::exception& e) {
        std::string errorMsg = "!!!!!!!!!! C++ EXCEPTION in LuaQuestHost::Init for '";
        errorMsg += m_scriptName + "': " + e.what();
        LogToFile(errorMsg);
        MessageBoxA(NULL, errorMsg.c_str(), "FSE LUA ERROR", MB_OK | MB_ICONERROR);
    }
}

void LuaQuestHost::Main() {
    LogToFile("--- Quest '" + m_scriptName + "': C++ Main() phase ---");
    try {
        sol::protected_function luaMain = m_env["Main"];
        if (luaMain.valid()) {
            sol::protected_function_result result = luaMain(this->m_pQuestState.get());
            if (!result.valid()) {
                sol::error err = result;
                LogToFile("!!! LUA RUNTIME ERROR in Main() of '" + m_scriptName + "': " + std::string(err.what()));
            }
        }
    }
    catch (const std::exception& e) {
        LogToFile("!!!!!!!!!! C++ EXCEPTION in LuaQuestHost::Main for '" + m_scriptName + "': " + std::string(e.what()));
    }
}

void LuaQuestHost::CreateThread(const std::string& luaFunctionName, const std::string& regionName, const std::vector<sol::object>& args) {
    if (m_threadCount >= MAX_QUEST_THREADS) {
        LogToFile("!!! ERROR: Max threads reached for '" + m_scriptName + "'");
        return;
    }
    int threadIndex = m_threadCount++;
    m_threadFunctions[threadIndex] = luaFunctionName;

    // *** Directly assign the vector ***
    m_threadArgs[threadIndex] = args; // Assign the vector directly

    std::string logMessage = "    Thread '" + luaFunctionName + "' registered in slot #" + std::to_string(threadIndex) + " for region '" + regionName + "'";
    if (!args.empty()) { // Check if the vector is empty
        logMessage += " with " + std::to_string(args.size()) + " argument(s).";
    }

    CSpawnedFunc* pFunc = (CSpawnedFunc*)Game_malloc(sizeof(CSpawnedFunc));
    if (pFunc) {
        FableString name(luaFunctionName.c_str());
        CSpawnedFunc_Construct(pFunc, name, 0);
        pFunc->pVTable = ASLR<void**>(0x12F5C24);
        pFunc->pThunkToMain = GetMemberFunctionAddress(g_threadRunnerPool[threadIndex]);
        pFunc->pOwnerScript = &this->base;
        FableString section(regionName.c_str());
        AddSpawnedFunction_func(&this->base, pFunc, section);
        LogToFile(logMessage);
    }
}

template<int N> void LuaQuestHost::ThreadRunner() {
    if (IsActiveThreadTerminating_Quest_API && IsActiveThreadTerminating_Quest_API(&this->base)) {
        return;
    }

    try {
        if (m_threadFunctions.count(N)) {
            const std::string& funcName = m_threadFunctions.at(N);
            sol::protected_function luaFunc = m_env[funcName];
            if (luaFunc.valid()) {
                sol::protected_function_result result;

                if (m_threadArgs.count(N)) {
                    result = luaFunc(this->m_pQuestState.get(), sol::as_args(m_threadArgs.at(N)));
                }
                else {
                    result = luaFunc(this->m_pQuestState.get());
                }

                if (!result.valid()) {
                    sol::error err = result;
                    LogToFile("!!! LUA RUNTIME ERROR in thread '" + funcName + "': " + std::string(err.what()));
                }
            }
        }
    }
    catch (const std::exception&) {
    }
}

template void LuaQuestHost::ThreadRunner<0>(); template void LuaQuestHost::ThreadRunner<1>(); template void LuaQuestHost::ThreadRunner<2>();
template void LuaQuestHost::ThreadRunner<3>(); template void LuaQuestHost::ThreadRunner<4>(); template void LuaQuestHost::ThreadRunner<5>();
template void LuaQuestHost::ThreadRunner<6>(); template void LuaQuestHost::ThreadRunner<7>(); template void LuaQuestHost::ThreadRunner<8>();
template void LuaQuestHost::ThreadRunner<9>(); template void LuaQuestHost::ThreadRunner<10>(); template void LuaQuestHost::ThreadRunner<11>();
template void LuaQuestHost::ThreadRunner<12>(); template void LuaQuestHost::ThreadRunner<13>(); template void LuaQuestHost::ThreadRunner<14>();
template void LuaQuestHost::ThreadRunner<15>(); template void LuaQuestHost::ThreadRunner<16>(); template void LuaQuestHost::ThreadRunner<17>();
template void LuaQuestHost::ThreadRunner<18>(); template void LuaQuestHost::ThreadRunner<19>();

void LuaQuestHost::Destructor(bool bDelete) {}

void LuaQuestHost::OnPersist(void* pCtx) {
    LogToFile("--- Quest '" + m_scriptName + "': C++ OnPersist() triggered. ---");

    try {
        sol::protected_function luaOnPersist = m_env["OnPersist"];
        if (luaOnPersist.valid()) {
            sol::protected_function_result result = luaOnPersist(this->m_pQuestState.get(), pCtx);
            if (!result.valid()) {
                sol::error err = result;
                LogToFile("!!! LUA RUNTIME ERROR in OnPersist() of '" + m_scriptName + "': " + std::string(err.what()));
            }
        }
        else {
            LogToFile("--- Quest '" + m_scriptName + "': No Lua OnPersist() function found. ---");
        }
    }
    catch (const std::exception& e) {
        LogToFile("!!!!!!!!!! C++ EXCEPTION in LuaQuestHost::OnPersist for '" + m_scriptName + "': " + std::string(e.what()));
    }
}

void LuaQuestHost::RegisterMain() { AutoRegisterMain(&this->base, GetMemberFunctionAddress(&LuaQuestHost::Main)); }