**Fable Script Extender (FSE) - Technical Overview**

Fable Script Extender (FSE) is a DLL-based modding tool for Fable: The Lost Chapters (compatible only with the Steam version of the executable) that injects a full Lua scripting engine into the game. It allows modders to write complex quests, custom entity behaviors, and game logic in Lua, bypassing the limitations of the native C++ script compilation process.

**1. Injection & Initialization**
The tool operates as a dynamic link library (dllmain.cpp) loaded by the game process. 
It installs a JMP hook at memory address 0xCDB355 to intercept the game's native script registration phase.
Upon triggering the hook, FSE executes InjectCustomScripts, which initializes the internal Fable API pointers and registers custom script allocators (g_questAllocatorPool and g_entityAllocatorPool) with the game engine. This tricks the game into loading FSE's Lua-backed "Hosts" instead of the native compiled scripts.

**2. The Lua Bridge (Sol2)**
FSE leverages the Sol2 C++ library to bind the game's internal engine functions to Lua.
A central singleton (LuaManager.cpp) responsible for the lifecycle of the Lua Virtual Machine. It enforces State Isolation, creating separate Lua states for different entities to prevent variable pollution and ensure stability.
The system parses a quests.lua configuration file to map internal Game Script IDs and Entity Definitions to specific Lua file paths, populating the g_questDefinitions vector.

**3. Custom Host Architecture**
The tool replaces the game's standard script hosts with hybrid C++/Lua classes that act as proxies between the engine and the Lua VM.
LuaQuestHost: Acts as the wrapper for the game's quest logic. It initializes a LuaQuestState object, which serves as the interface between the script and the game world, and executes the script's Main loop.
LuaEntityHost: Wraps individual entity (Scripted NPC/ Object) logic. It manages the lifecycle of a script attached to a specific CScriptThing. It handles the acquisition of entity pointers and creates an isolated environment where the *this variable represents the specific entity.

**4. The API**
FSE exposes a massive portion of the internal Fable engine to Lua through two primary interfaces, converting internal engine pointers into safe Lua usertypes.
LuaQuestState API 
Defined in LuaQuestState.cpp, this API provides global functions for world manipulation. It wraps access to the game's internal CWorld, CThingManager, and CHero structures and more.
World Manipulation: SetTimeOfDay, TransitionToTheme, CreateCreature, CreateObject.
UI & Cutscenes: ShowMessage, PlayCutscene, StartMovieSequence, FadeScreenOut.
Hero Control: GiveHeroGold, GiveHeroExperience, SetHeroAge, GiveHeroRenownPoints.
Logic & State: SetGlobalBool, GetGlobalInt, IsQuestActive, FailAllActiveQuests and more.
LuaEntityAPI 
Defined in LuaEntityAPI.cpp, this API provides context-aware functions for controlling specific entities via the *this context.
AI & Movement: MoveToPosition, FollowThing ect.
Animation & Combat: PlayAnimation, PlayCombatAnimation, UnsheatheWeapons, FireProjectileWeaponAtTarget ect.
Interaction: Speak, Converse, OpenChest, MsgIsHitByHero ect.

**5. Threading & Blocking Support**
Fable's engine relies on a frame-based execution model. FSE implements sophisticated wrappers (e.g., SpeakAndWait, MoveToPosition with wait loops) to bridge Lua's linear execution with the game's asynchronous tasks.
Mechanism: When a blocking command is issued, FSE creates a CScriptGameResourceObjectScriptedThingBase handle to acquire control of the entity.
Yielding: It enters a C++ loop that checks IsPerformingScriptTask while calling NewScriptFrame_API to yield execution back to the game engine. This allows Lua scripts to pause execution until an in-game action (like walking to a point) completes, without freezing the game thread.


**6. Memory Management**
FSE integrates directly with the game's memory manager to ensure stability and native compatibility.
Custom Allocators: The tool implements QuestAllocator and EntityAllocator templates. These direct the game engine to allocate memory for FSE's custom host classes within the game's own memory pool using Game_malloc.
Smart Pointer Wrapping: Interactions with game objects (CScriptThing) are wrapped in std::shared_ptr with custom deleters. These deleters interact with the game's internal RefCount system, ensuring that FSE correctly increments and decrements reference counts to prevent memory leaks or crashes when scripts are unloaded.
VTable Hooks: The tool reconstructs and utilizes the game's Virtual Method Tables (g_pCScriptThingVTable, etc.) to call internal functions dynamically.


