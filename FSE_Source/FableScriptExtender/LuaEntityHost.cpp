#include "LuaEntityHost.h"
#include "LuaQuestHost.h"
#include "LuaQuestState.h"
#include "LuaManager.h" 
#include "GameInterface.h"
#include <sstream> 

void LuaEntityHost::Destructor(bool bDelete) {
    if (bDelete) {

        std::stringstream ss;
        ss << "    [LuaEntityHost::Destructor] Cleaning up host 0x" << std::hex << (DWORD)this;
        LogToFile(ss.str());

        LuaManager::GetInstance().UnregisterEntityScriptData(this);
    }
}

void LuaEntityHost::Main() {
    if (IsActiveThreadTerminating_Entity_API && IsActiveThreadTerminating_Entity_API(this)) {
        return;
    }

    EntityScriptData* pData = nullptr;
    std::string scriptNameForLog = "UNKNOWN";

    try {
        pData = LuaManager::GetInstance().GetEntityScriptData(this);
        if (!pData || !pData->luaMain.valid()) {
            return;
        }

        scriptNameForLog = pData->scriptName;
        LogToFile("    [LuaEntityHost::Main] >>>>> ENTERING LUA CALL for '" + scriptNameForLog + "' <<<<<");

        sol::protected_function_result result = pData->luaMain(pData->pQuestState, &this->m_Me);

        LogToFile("    [LuaEntityHost::Main] >>>>> EXITED LUA CALL for '" + scriptNameForLog + "' <<<<<");

        if (!result.valid()) {
            if (!pData->errorLogged) {
                sol::error err = result;
                LogToFile("!!! LUA RUNTIME ERROR in Main() of '" + pData->scriptName + "': " + std::string(err.what()));
                pData->errorLogged = true;
            }
        }

        LogToFile("    [DEBUG] LuaEntityHost::Main: About to return from C++ Main function.");
    }
    catch (const std::exception& e) {
        LogToFile("!!!!!!!!!! C++ EXCEPTION in LuaEntityHost::Main for '" + scriptNameForLog + "': " + std::string(e.what()));
    }

    LogToFile("    [DEBUG] LuaEntityHost::Main: FINAL EXIT POINT.");
}

void LuaEntityHost::Init() {
    EntityScriptData* pScriptData = nullptr;
    try {
        pScriptData = LuaManager::GetInstance().GetEntityScriptData(this);
        if (!pScriptData || !pScriptData->pLuaState) { // <-- ADDED check for pLuaState
            LogToFile("!!! ERROR: LuaEntityHost::Init - No script data or pLuaState!");
            return;
        }

        std::string scriptPath = GetScriptPath(pScriptData->scriptName);
        LogToFile("    Loading entity script from: " + scriptPath);

        // <-- MODIFY THIS LINE to use the entity's isolated state -->
        pScriptData->pLuaState->script_file(scriptPath, pScriptData->env);

        sol::protected_function luaInit = pScriptData->env["Init"];
        pScriptData->luaMain = pScriptData->env["Main"];

        if (luaInit.valid()) {
            sol::protected_function_result result = luaInit(pScriptData->pQuestState, &this->m_Me);
            if (!result.valid()) {
                sol::error err = result;
                LogToFile("!!! LUA RUNTIME ERROR in Init() of '" + pScriptData->scriptName + "': " + std::string(err.what()));
            }
        }
    }
    catch (const std::exception& e) {
        std::string errorMsg = "!!!!!!!!!! C++ EXCEPTION in LuaEntityHost::Init for entity '";
        errorMsg += (pScriptData ? pScriptData->scriptName : "UNKNOWN") + "': " + e.what();
        LogToFile(errorMsg);
        MessageBoxA(NULL, errorMsg.c_str(), "FSE LUA ERROR", MB_OK | MB_ICONERROR);
    }
}

CScriptBase_Retail* LuaEntityHost::GetParentScript_Stub() {
    return &this->m_pParentHost->base;
}

void LuaEntityHost::OnPersist(void* pCtx) {
    EntityScriptData* pData = LuaManager::GetInstance().GetEntityScriptData(this);
    if (!pData) {
        LogToFile("--- Entity <Unknown>: C++ OnPersist triggered (Could not get script data). ---");
        return;
    }

    LogToFile("--- Entity '" + pData->scriptName + "': C++ OnPersist triggered. ---");

    try {
        sol::protected_function luaOnPersist = pData->env["OnPersist"];
        if (luaOnPersist.valid()) {
            sol::protected_function_result result = luaOnPersist(pData->pQuestState, &this->m_Me, pCtx);
            if (!result.valid()) {
                sol::error err = result;
                LogToFile("!!! LUA RUNTIME ERROR in OnPersist() of entity '" + pData->scriptName + "': " + std::string(err.what()));
                pData->errorLogged = true;
            }
        }
    }
    catch (const std::exception& e) {
        LogToFile("!!!!!!!!!! C++ EXCEPTION in LuaEntityHost::OnPersist for entity '" + pData->scriptName + "': " + std::string(e.what()));
        pData->errorLogged = true;
    }
}

LuaEntityHost::LuaEntityHost(CScriptDataBase* pMasterData, LuaQuestHost* pParentQuest, const CScriptThing* thing, const std::string& scriptName)
    : pInterface(pParentQuest->pInterface), m_pParentHost(pParentQuest), pData(pMasterData), unknown_1C(0)
{
    this->pVTable = g_LuaEntityHostVTable;
    memcpy(&this->m_Me, thing, sizeof(CScriptThing));

    // <-- MODIFY THIS CALL to match the new prototype
    // We just pass the quest state. The manager will create the new sol::state.
    LuaManager::GetInstance().RegisterEntityScriptData(this, scriptName, pParentQuest->GetQuestState());
}

LuaEntityHost::~LuaEntityHost() {}

void LuaEntityHost::OnPredicateFail_Stub() {
    LogToFile("    [LuaEntityHost::OnPredicateFail] CALLED!");
}

void LuaEntityHost::OnInterrupted_Stub() {
    LogToFile("    [LuaEntityHost::OnInterrupted] CALLED!");
}