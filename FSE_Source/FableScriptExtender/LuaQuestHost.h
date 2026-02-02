/**
 * @file LuaQuestHost.h
 * @brief Host class that wraps Lua quest scripts to interface with Fable's quest system.
 *
 * LuaQuestHost is the bridge between Fable's native quest system and Lua scripts.
 * When the game activates a custom quest, it instantiates a LuaQuestHost through
 * the QuestAllocator<N> template. The host then loads the corresponding Lua script
 * and translates game callbacks (Init, Main, OnPersist) into Lua function calls.
 *
 * KEY RESPONSIBILITIES:
 *
 * 1. SCRIPT LIFECYCLE MANAGEMENT
 *    - Constructor loads and parses the Lua script file
 *    - Init() calls the script's Init(quest) function
 *    - Main() runs the main coroutine (blocking until complete or yielded)
 *    - OnPersist() handles save/load state serialization
 *
 * 2. LUA ENVIRONMENT ISOLATION
 *    - Each quest gets its own sol::state and sol::environment
 *    - Scripts cannot pollute the global Lua namespace
 *    - LuaQuestState API is bound to the "Quest" variable
 *
 * 3. THREAD MANAGEMENT
 *    - CreateThread() spawns parallel execution contexts
 *    - ThreadRunner<N>() executes thread functions in game's coroutine system
 *    - Used for region-specific logic running concurrently
 *
 * 4. ENTITY SCRIPT SPAWNING
 *    - Tracks entity scripts that belong to this quest
 *    - Entity allocators receive reference to parent LuaQuestHost
 *
 * MEMORY LAYOUT:
 * The 'base' member MUST be first and match CScriptBase_Retail layout.
 * The g_LuaQuestHostVTable array defines the virtual function table that
 * Fable expects (Destructor, RegisterMain, Main, Init, OnPersist).
 *
 * @see LuaEntityHost - Entity script wrapper (child of quest)
 * @see LuaQuestState - Lua API exposed to quest scripts
 * @see QuestAllocator - Template instantiating LuaQuestHost
 */
#pragma once
#include "FableAPI.h"
#include "sol/sol.hpp"
#include <string>
#include <memory>
#include <map>
#include <vector>
#include "MasterQuest.h"

class LuaQuestState;
struct CScriptDataBase;

/** @brief Virtual function table matching Fable's expected quest script interface */
extern void* g_LuaQuestHostVTable[];

/**
 * @brief Host class wrapping Lua quest scripts for Fable integration.
 *
 * Instantiated by QuestAllocator<N> when the game activates a custom quest.
 * Manages the Lua environment, script execution, threading, and state persistence.
 */
class LuaQuestHost {
public:
    CScriptBase_Retail base;
    CGameScriptInterfaceBase* pInterface;

    CQ_SunnyvaleMasterData* m_pMasterData;

    void Destructor(bool bDelete);
    void RegisterMain();
    void Main();
    void Init();
    void OnPersist(void* pCtx);

    LuaQuestHost(CScriptDataBase* data, CGameScriptInterfaceBase* interface, const std::string& scriptName);
    ~LuaQuestHost();

    const std::string& GetScriptName() const;
    LuaQuestState* GetQuestState() { return m_pQuestState.get(); }
    sol::environment& GetEnvironment() { return m_env; }
    sol::state* GetLuaState() { return m_pLuaState.get(); }

    CQ_SunnyvaleMasterData* GetMasterData() { return m_pMasterData; }

    void SetMasterData(CScriptDataBase* pdata);

    void CreateThread(const std::string& luaFunctionName, const std::string& regionName, const std::vector<sol::object>& args);

    template<int N> void ThreadRunner();

private:
    std::string m_scriptName;
    std::unique_ptr<LuaQuestState> m_pQuestState;
    std::unique_ptr<sol::state> m_pLuaState;
    sol::environment m_env;

    int m_threadCount;
    std::map<int, std::string> m_threadFunctions;
    std::map<int, std::vector<sol::object>> m_threadArgs;
};