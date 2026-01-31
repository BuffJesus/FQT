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

extern void* g_LuaQuestHostVTable[];

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