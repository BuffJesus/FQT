/**
 * @file LuaEntityHost.h
 * @brief Host class that wraps Lua entity scripts to interface with Fable's entity system.
 *
 * LuaEntityHost is the bridge between Fable's native entity scripting and Lua scripts.
 * When a quest spawns a scripted entity, the EntityAllocator<N> creates a LuaEntityHost
 * that loads the entity's Lua script and translates game callbacks into Lua function calls.
 *
 * KEY RESPONSIBILITIES:
 *
 * 1. ENTITY BINDING
 *    - m_Me holds the CScriptThing reference to the actual game entity
 *    - Provides access to entity position, state, animations, AI, etc.
 *    - Entity can be accessed in Lua via "Me" variable
 *
 * 2. SCRIPT EXECUTION
 *    - Init() called when entity is first bound, sets up control mode
 *    - Main() runs the behavior loop from the Lua script
 *    - Called repeatedly by the game's scripting system
 *
 * 3. QUEST INTEGRATION
 *    - m_pParentHost links back to the owning LuaQuestHost
 *    - Entity scripts access quest state via "Quest" variable
 *    - Can communicate with other entities through shared quest state
 *
 * 4. LIFECYCLE MANAGEMENT
 *    - OnPersist() handles entity state during save/load
 *    - Destructor properly cleans up Lua resources
 *    - CustomEntityHostDeleter handles reference-counted deletion
 *
 * MEMORY LAYOUT:
 * The first members MUST match Fable's expected entity script layout:
 * - pVTable at offset 0x00
 * - pInterface at offset 0x04
 * - m_Me at offset 0x08
 * - Additional members after the required layout
 *
 * VIRTUAL FUNCTIONS:
 * g_LuaEntityHostVTable defines: Destructor, Main, Init, GetParentScript,
 * OnPersist, OnPredicateFail, OnInterrupted
 *
 * @see LuaQuestHost - Parent quest host
 * @see LuaEntityAPI - Lua API exposed via "Me" object
 * @see EntityAllocator - Template instantiating LuaEntityHost
 */
#pragma once
#include "FableAPI.h"
#include "sol/sol.hpp"
#include <string>

class LuaQuestHost;
class LuaQuestState;

/** @brief Virtual function table matching Fable's expected entity script interface */
extern void* g_LuaEntityHostVTable[];

/**
 * @brief Host class wrapping Lua entity scripts for Fable integration.
 *
 * Instantiated by EntityAllocator<N> when a quest spawns a scripted entity.
 * Manages the entity binding and Lua script execution lifecycle.
 */
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