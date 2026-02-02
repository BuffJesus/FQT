/**
 * @file LuaManager.h
 * @brief Singleton manager for Lua VM lifecycle and entity script data.
 *
 * LuaManager is the central coordinator for all Lua scripting in FSE. It manages:
 *
 * 1. LUA VM LIFECYCLE
 *    - Initialize(): Creates Lua state and registers all FSE bindings
 *    - Reinitialize(): Called on level reload to reset state
 *    - Shutdown(): Cleans up Lua resources
 *
 * 2. ENTITY SCRIPT TRACKING
 *    - Maintains a map of LuaEntityHost* to EntityScriptData
 *    - Each entity gets its own Lua environment for isolation
 *    - Tracks script state, error flags, and Main() function references
 *
 * 3. GLOBAL STATE STORAGE
 *    - Provides cross-script state sharing via Get/SetGlobalState* methods
 *    - Supports bool, int, and string types
 *    - Used for communication between quests and entities
 *
 * USAGE PATTERN:
 * @code
 * // In quest/entity initialization
 * LuaManager::GetInstance().RegisterEntityScriptData(pHost, "MyScript", pQuestState);
 *
 * // In script execution
 * EntityScriptData* data = LuaManager::GetInstance().GetEntityScriptData(pHost);
 * data->luaMain();
 *
 * // Global state access
 * LuaManager::GetInstance().SetGlobalStateBool("QuestComplete", true);
 * @endcode
 *
 * @note LuaManager uses the Sol2 library for C++/Lua interop.
 * @see sol::state - Sol2's Lua state wrapper
 * @see LuaQuestState - Quest-level Lua API bindings
 * @see LuaEntityAPI - Entity-level Lua API bindings
 */
#pragma once
#include "FableAPI.h"
#include <sol/sol.hpp>
#include <string>
#include <map>
#include "LuaEntityAPI.h"
#include <variant>
#include <mutex>

class LuaQuestState;
class LuaEntityHost;
struct CPersistContext;

/**
 * @brief Container for all data associated with an entity's Lua script.
 *
 * Each entity managed by FSE has an EntityScriptData instance that stores:
 * - The script file name (for loading/reloading)
 * - Reference to parent quest's LuaQuestState
 * - Isolated Lua environment (prevents globals from leaking between scripts)
 * - Cached reference to the Main() function for efficient repeated calls
 * - Error tracking to avoid spam logging the same error
 */
struct EntityScriptData {
    /** @brief Relative path to the script file (without .lua extension) */
    std::string scriptName;
    /** @brief Pointer to the parent quest's state wrapper */
    LuaQuestState* pQuestState;
    /** @brief Isolated Lua environment for this entity's script */
    sol::environment env;
    /** @brief Cached reference to the script's Main() function */
    sol::protected_function luaMain;
    /** @brief Flag to prevent repeated error logging */
    bool errorLogged = false;
    /** @brief Optional separate Lua state (for complete isolation) */
    std::unique_ptr<sol::state> pLuaState;
};

/**
 * @brief Singleton manager for Lua VM and entity script data.
 *
 * Provides centralized management of the Lua runtime environment and tracks
 * all entity scripts for proper cleanup and state management.
 */
class LuaManager {
public:
    static LuaManager& GetInstance();
    void Initialize();
    void Shutdown();
    void Reinitialize();
    void ClearAllEntityData();
    bool IsInitialized() const { return m_initialized; }

    void RegisterBindingsInState(sol::state& lua);

    void RegisterEntityScriptData(LuaEntityHost* pHost, const std::string& scriptName, LuaQuestState* pQuestState);
    void UnregisterEntityScriptData(LuaEntityHost* pHost);
    EntityScriptData* GetEntityScriptData(LuaEntityHost* pHost);

    void SetGlobalStateBool(const std::string& key, bool value);
    void SetGlobalStateInt(const std::string& key, int value);
    void SetGlobalStateString(const std::string& key, const std::string& value);
    int GetGlobalStateInt(const std::string& key);
    bool GetGlobalStateBool(const std::string& key);
    std::string GetGlobalStateString(const std::string& key);

private:
    LuaManager() {}
    ~LuaManager() = default;
    LuaManager(const LuaManager&) = delete;
    LuaManager& operator=(const LuaManager&) = delete;

    std::map<LuaEntityHost*, EntityScriptData> m_entityScriptDataMap;

    std::map<std::string, std::variant<bool, int, std::string>> m_globalState;
    bool m_initialized = false;
};