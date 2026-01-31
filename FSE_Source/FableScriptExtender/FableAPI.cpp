#include "FableAPI.h"
#include <Shlwapi.h>
#include <sstream>
#include <memory>
#include "EntityScriptingAPI.h"
#pragma comment(lib, "shlwapi.lib")

std::string g_fseBasePath = "";

void InitializeFSEPaths(HMODULE hMod) {
    char dllPath[MAX_PATH] = { 0 };
    GetModuleFileNameA(hMod, dllPath, MAX_PATH);
    PathRemoveFileSpecA(dllPath);

    g_fseBasePath = std::string(dllPath) + "/FSE";
}

void LogCScriptThingDetails(const std::string& context, CScriptThing* pThing) {
    std::stringstream ss;
    ss << context << ": CScriptThing* = 0x" << std::hex << reinterpret_cast<uintptr_t>(pThing);
    if (!pThing) {
        ss << " (NULL)";
        LogToFile(ss.str());
        return;
    }

    ss << ", pVTable = 0x" << std::hex << reinterpret_cast<uintptr_t>(pThing->pVTable);
    if (pThing->pVTable == g_pCScriptThingVTable) {
        ss << " (Matches Known VTable)";
    }
    else {
        ss << " (!!! MISMATCH !!! Expected: 0x" << std::hex << reinterpret_cast<uintptr_t>(g_pCScriptThingVTable) << ")";
    }

    ss << ", pImp.Data = 0x" << std::hex << reinterpret_cast<uintptr_t>(pThing->pImp.Data);
    ss << ", pImp.Info = 0x" << std::hex << reinterpret_cast<uintptr_t>(pThing->pImp.Info);

    if (pThing->pImp.Info) {
        ss << ", Info->RefCount = " << std::dec << pThing->pImp.Info->RefCount;
        ss << ", Info->DeleteFunc = 0x" << std::hex << reinterpret_cast<uintptr_t>(pThing->pImp.Info->DeleteFunc);
        ss << ", Info->Data = 0x" << std::hex << reinterpret_cast<uintptr_t>(pThing->pImp.Info->Data);
        if (pThing->pImp.Info->Data != pThing->pImp.Data) {
            ss << " (!!! Info->Data != pImp.Data !!!)";
        }
    }
    else {
        ss << ", Info = NULL";
    }
    LogToFile(ss.str());
}

std::string GetLogFilePath() {
    if (g_fseBasePath.empty()) return "C:/Temp/FableQuestHook_fallback.log";
    return g_fseBasePath + "/FableScriptExtender.log";
}

std::string GetQuestsConfigPath() {
    return g_fseBasePath + "/quests.lua";
}

std::string GetScriptPath(const std::string& scriptFile) {
    return g_fseBasePath + "/" + scriptFile + ".lua";
}

std::shared_ptr<CScriptThing> WrapScriptThingOutput(CScriptThing* pThingBuffer) {
    if (!pThingBuffer) return nullptr;

    // We must validate the result. IsNull check is safest.
    if (pThingBuffer->pVTable) {
        // --- THIS IS THE CORRECTED LINE ---
        auto* pVTable = reinterpret_cast<CScriptThingVTable*>(pThingBuffer->pVTable);
        // --- END CORRECTION ---

        if (pVTable->IsNull && pVTable->IsNull(pThingBuffer)) {
            Game_free(pThingBuffer); // Free the buffer, it's a null object
            return nullptr;
        }
    }
    else {
        // No VTable, invalid.
        Game_free(pThingBuffer);
        return nullptr;
    }

    // The thing is valid. Increment its ref count.
    if (pThingBuffer->pImp.Info) {
        pThingBuffer->pImp.Info->RefCount++;
    }
    else {
        LogToFile("!!! WARNING: WrapScriptThingOutput - Thing has no pImp.Info! Cannot increment ref count.");
    }

    // Return a shared_ptr that manages the heap-allocated buffer
    // and decrements the game's ref count on deletion.
    return std::shared_ptr<CScriptThing>(pThingBuffer, [](CScriptThing* p) {
        if (p && p->pImp.Info && p->pImp.Info->RefCount > 0) {
            p->pImp.Info->RefCount--;
        }
        if (p && Game_free) {
            Game_free(p);
        }
        });
}

DWORD g_fableBase = 0;

void* pEmptyDataAlloc = nullptr;
void** g_pEntityScriptBindingVTable = nullptr;
void** g_pCScriptThingVTable = nullptr;
void** g_pCScriptGameResourceObjectScriptedThingBaseVTable = nullptr;
void** g_pMovieObjectVTable = nullptr;
t_malloc                                Game_malloc = nullptr;
t_free                                  Game_free = nullptr;
tCCharString_Constructor_Literal        CCharString_Construct_Literal = nullptr;
tCCharString_Constructor_Copy           CCharString_Construct_Copy = nullptr;
tCCharString_Destructor                 CCharString_Destroy = nullptr;
tCheckSection                           CheckSection = nullptr;
tAddScript                              AddScript = nullptr;
tSetScriptActiveStatus                  SetScriptActiveStatus_Func = nullptr;
tCScriptBase_Constructor                CScriptBase_Construct = nullptr;
tCSpawnedFunc_Constructor               CSpawnedFunc_Construct = nullptr;
tAddSpawnedFunction                     AddSpawnedFunction_func = nullptr;
tScriptAllocFunc                        pSunnyvaleDataAlloc_func = nullptr;
CGameScriptInterfaceBase**              g_pDSTGame = nullptr;
tRunCutsceneMacro                       RunCutsceneMacro_Func = nullptr;
tAddEntityScriptBinding                 AddEntityScriptBinding_API = nullptr;
tPostAddScriptedEntities_CScriptBase    PostAddScriptedEntities_CScriptBase_API = nullptr;
tIsActiveThreadTerminating_Entity       IsActiveThreadTerminating_Entity_API = nullptr;
tIsActiveThreadTerminating_Quest        IsActiveThreadTerminating_Quest_API = nullptr;
tStdMap_Constructor                     StdMap_Construct_API = nullptr;
tStdMap_OperatorBracket                 StdMap_OperatorBracket_API = nullptr;
tStdMap_Destructor                      StdMap_Destroy_API = nullptr;
tCBaseObject_Constructor                CBaseObject_Construct_API = nullptr;
tCBaseObject_AssignmentOperator         CBaseObject_Assign_API = nullptr;
tCBaseObject_Destructor                 CBaseObject_Destroy_API = nullptr;
tCleanupPImp                            CleanupMoviePImp_API = nullptr;
tInitScriptObjectHelper1                InitScriptObjectHelper1_API = nullptr;
tInitScriptObjectHelper2                InitScriptObjectHelper2_API = nullptr;
tCCharString_AssignmentOperator         CCharString_Assign_API = nullptr;
tCCharString_ToConstChar                CCharString_ToConstChar_API = nullptr;
tCCharString_OperatorPlus               CCharString_OperatorPlus_API = nullptr;
tGFCharStringToInt                      GFCharStringToInt_API = nullptr;
tGFIntToCharString                      GFIntToCharString_API = nullptr;
tIsDistanceBetweenThingsUnder           IsDistanceBetweenThingsUnder_API = nullptr;
tCPersistContext_Transfer_bool          CPersistContext_Transfer_bool_API = nullptr;
tCPersistContext_Transfer_int           CPersistContext_Transfer_int_API = nullptr;
tCPersistContext_Transfer_string        CPersistContext_Transfer_string_API = nullptr;
tCPersistContext_Transfer_float         CPersistContext_Transfer_float_API = nullptr;
tCPersistContext_Transfer_uint          CPersistContext_Transfer_uint_API = nullptr;
tCSGROSTB_Destructor                    CSGROSTB_Destroy_API = nullptr;

void InitializeFableAPI() {
    LogToFile("--- Initializing Fable API Pointers ---");
    g_fableBase = (DWORD)GetModuleHandleA(NULL);
    if (!g_fableBase) {
        LogToFile("!!! CRITICAL: GetModuleHandleA failed! Can't initialize API. !!!");
        return;
    }

    std::stringstream ss;
    ss << "Module Base Address (g_fableBase): 0x" << std::hex << g_fableBase;
    LogToFile(ss.str());

    Game_malloc = ASLR<t_malloc>(0xBFEA0E);
    Game_free = ASLR<t_free>(0xBFEA14);
    CCharString_Construct_Literal = ASLR<tCCharString_Constructor_Literal>(0x99EBF0);
    CCharString_Construct_Copy = ASLR<tCCharString_Constructor_Copy>(0x99EC30);
    CCharString_Destroy = ASLR<tCCharString_Destructor>(0x99EAE0);
    CheckSection = ASLR<tCheckSection>(0xCB5AC0);
    AddScript = ASLR<tAddScript>(0xCB5C90);
    SetScriptActiveStatus_Func = ASLR<tSetScriptActiveStatus>(0xCBFAB8);
    CScriptBase_Construct = ASLR<tCScriptBase_Constructor>(0xCB8110);
    CSpawnedFunc_Construct = ASLR<tCSpawnedFunc_Constructor>(0xCDD450);
    AddSpawnedFunction_func = ASLR<tAddSpawnedFunction>(0xCB7E50);
    pEmptyDataAlloc = ASLR<void*>(0xCD4AC0);
    pSunnyvaleDataAlloc_func = ASLR<tScriptAllocFunc>(0xCDBD20);
    g_pDSTGame = ASLR<CGameScriptInterfaceBase**>(0x143E8F8);
    AddEntityScriptBinding_API = ASLR<tAddEntityScriptBinding>(0xCB8230);
    PostAddScriptedEntities_CScriptBase_API = ASLR<tPostAddScriptedEntities_CScriptBase>(0xCB8930);
    g_pEntityScriptBindingVTable = ASLR<void**>(0x12EA57C);
    IsActiveThreadTerminating_Entity_API = ASLR<tIsActiveThreadTerminating_Entity>(0xF35B30);
    IsActiveThreadTerminating_Quest_API = ASLR<tIsActiveThreadTerminating_Quest>(0xCB7940);
    RunCutsceneMacro_Func = ASLR<tRunCutsceneMacro>(0xCBFB7D);
    StdMap_Construct_API = ASLR<tStdMap_Constructor>(0xCDBF70);
    StdMap_Destroy_API = ASLR<tStdMap_Destructor>(0xCDBFB0);
    StdMap_OperatorBracket_API = ASLR<tStdMap_OperatorBracket>(0xCD3D2E);
    CBaseObject_Construct_API = ASLR<tCBaseObject_Constructor>(0x99A380);
    CBaseObject_Assign_API = ASLR<tCBaseObject_AssignmentOperator>(0x99A3B0);
    CBaseObject_Destroy_API = ASLR<tCBaseObject_Destructor>(0x99A430);
    g_pMovieObjectVTable = ASLR<void**>(0x1260EF4);
    CleanupMoviePImp_API = ASLR<tCleanupPImp>(0x6E7AB0);
    InitScriptObjectHelper1_API = ASLR<tInitScriptObjectHelper1>(0xCD23B9);
    InitScriptObjectHelper2_API = ASLR<tInitScriptObjectHelper2>(0xCD2770);
    g_pCScriptGameResourceObjectScriptedThingBaseVTable = ASLR<void**>(0x127094C);
    g_pCScriptThingVTable = ASLR<void**>(0x1238C8C);
    CCharString_Assign_API = ASLR<tCCharString_AssignmentOperator>(0x99EFB0);
    CCharString_ToConstChar_API = ASLR<tCCharString_ToConstChar>(0x99E4C0);
    GFCharStringToInt_API = ASLR<tGFCharStringToInt>(0x99E7F0);
    GFIntToCharString_API = ASLR<tGFIntToCharString>(0x99F830);
    CCharString_OperatorPlus_API = ASLR<tCCharString_OperatorPlus>(0x99F690);
    IsDistanceBetweenThingsUnder_API = ASLR<tIsDistanceBetweenThingsUnder>(0xCBE2FF);
    CPersistContext_Transfer_bool_API = ASLR<tCPersistContext_Transfer_bool>(0x4045C0);   
    CPersistContext_Transfer_int_API = ASLR<tCPersistContext_Transfer_int>(0x410BE0);   
    CPersistContext_Transfer_string_API = ASLR<tCPersistContext_Transfer_string>(0x4109A0);
    CPersistContext_Transfer_float_API = ASLR<tCPersistContext_Transfer_float>(0x410620);   
    CPersistContext_Transfer_uint_API = ASLR<tCPersistContext_Transfer_uint>(0x4106F0); 
    CSGROSTB_Destroy_API = ASLR<tCSGROSTB_Destructor>(0x7E74D0);

    LogToFile("--- Fable API Pointers Initialized ---");
}