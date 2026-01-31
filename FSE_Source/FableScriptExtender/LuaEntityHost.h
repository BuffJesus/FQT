#pragma once
#include "FableAPI.h"
#include "sol/sol.hpp"
#include <string>

class LuaQuestHost;
class LuaQuestState;

extern void* g_LuaEntityHostVTable[];

class LuaEntityHost {
public:
    void** pVTable;        // 0x00
    CGameScriptInterfaceBase* pInterface;     // 0x04
    CScriptThing     m_Me;           // 0x08
    LuaQuestHost* m_pParentHost;  // 0x14
    CScriptDataBase* pData;          // 0x18
    DWORD            unknown_1C;     // 0x1C

    void Destructor(bool bDelete);
    void Main();
    void Init();

    CScriptBase_Retail* GetParentScript_Stub();

    void OnPersist(void* pCtx);

    void OnPredicateFail_Stub();
    void OnInterrupted_Stub();

    LuaEntityHost(CScriptDataBase* pMasterData, LuaQuestHost* pParentQuest, const CScriptThing* thing, const std::string& scriptName);
    ~LuaEntityHost();
};