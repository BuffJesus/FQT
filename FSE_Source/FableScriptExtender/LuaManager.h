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

struct EntityScriptData {
    std::string scriptName;
    LuaQuestState* pQuestState;
    sol::environment env;
    sol::protected_function luaMain;
    bool errorLogged = false;
    std::unique_ptr<sol::state> pLuaState;
};

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