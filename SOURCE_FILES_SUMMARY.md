# Source Files Summary

## Overview

This document summarizes the source files located in `SourceFilesToReference/` that are used as reference for FSE Quest Creator Pro development.

## Directory Structure

```
SourceFilesToReference/
├── fse-quest-creator-prompt-v2.md    # Main development prompt/specification
├── FSE/                               # FSE binary and example quests
│   ├── FableScriptExtender/           # Compiled FSE with working examples
│   ├── FSEBeta/                       # Beta version with additional examples
│   └── FableScriptExtender-master/    # FSE source code (C++)
├── fse-quest-creator-v2/              # React/Electron prototype (node-based editor)
└── OldQuestManager/                   # Previous .NET quest editor attempt
```

## Key Reference Files

### 1. FSE Quest Examples

**Location:** `FSE/FableScriptExtender/FSE/` and `FSE/FSEBeta/FSE/`

#### Working Quest Examples:

**MyFirstQuest/** - Basic quest with entity spawning and dialogue
- `MyFirstQuest.lua` - Main quest script showing:
  - Immediate Main() execution (no blocking)
  - Entity binding pattern
  - Quest card setup
  - Thread creation
  - API usage examples (health changes, abilities, etc.)
- `Entities/NPCtoTalk.lua` - NPC behavior with dialogue
- `Entities/BanditArcher.lua` - Enemy entity behavior

**MySecondQuest/** - Bulletin board quest with item monitoring
- `MySecondQuest.lua` - Shows thread-based item monitoring
- `Entities/FatherChocolateQuest.lua` - NPC with gift-giving behavior
- `Entities/ChangingBulletinBoard.lua` - Interactive bulletin board

**MyThirdQuest/** - Simple quest demonstrating basic patterns
- `MyThirdQuest.lua` - Minimal quest structure

**GhostGrannyNecklaceLUA/** - Complex multi-entity quest
- `GhostGrannyNecklaceLUA.lua` - Multiple entity coordination
- `Entities/GhostGrannySon.lua` - Entity with conversation
- `Entities/GhostGrannySon.lua` - Multiple dialogue branches
- `Entities/GhostGrannyDaughterInLaw.lua` - Additional NPC

**WaspBossLUA/** - Boss fight quest with timers
- `WaspBossLUA.lua` - Timer management, complex state tracking

**DemonDoorLUA/** - Interactive door quest
- `DemonDoorLUA.lua` - Requirement checking (gold, items)
- `Entities/DemonDoorRich.lua` - Door behavior

#### Key Patterns from Examples:

1. **Main() Structure:** Never blocks, does immediate setup, creates threads
2. **Thread Patterns:** Region-bound threads auto-wait for region load
3. **Entity Scripts:** Use `Init(questObject, meObject)` and `Main(questObject, meObject)`
4. **Loop Control:** Always call `Quest:NewScriptFrame(Me)` in entity loops
5. **Entity Control:** `MakeBehavioral()`, `AcquireControl()`, `ReleaseControl()`

### 2. FSE Registry Files

**quests.lua** - Quest registration format
```lua
Quests = {
    QuestName = {
        name = "QuestName",
        file = "QuestName/QuestName",
        id = 50001,
        entity_scripts = {
            { name = "EntityName", file = "QuestName/Entities/EntityName", id = 50002 },
        }
    },
}
```

**Master/FSE_Master.lua** - Quest activation master script
- Shows how quests are activated via `ActivateQuest()`
- Global state management with `OnPersist()`

### 3. Development Specification

**fse-quest-creator-prompt-v2.md** - Complete development specification including:
- Data model definitions (QuestProject, QuestEntity, etc.)
- Node-based editor requirements
- UI/UX specifications
- Code generation patterns
- Deployment workflows

Key sections:
1. Core Architecture (data models)
2. Visual Node Editor (node types, connections)
3. Code Generation (Lua script templates)
4. UI Components (WPF views, view models)
5. Deployment Service (file management)

### 4. Node-Based Editor Prototype

**fse-quest-creator-v2/** - React/Electron prototype with:
- `src/components/NodeEditor.tsx` - Visual node editor implementation
- `src/types/nodes.ts` - Node type definitions
- `src/utils/codeGen.ts` - Code generation logic

This prototype demonstrates:
- Drag-and-drop node placement
- Connection system (True/False branches)
- Node configuration UI
- Real-time Lua code preview

### 5. FSE Source Code

**FSE/FableScriptExtender-master/** - C++ source code for FSE
- `README.md` - FSE overview and API documentation
- Shows internal implementation of Lua API
- Useful for understanding API behavior and limitations

Key insights:
- Region-bound threads are handled by FSE engine
- Entity lifecycle managed by FSE
- Lua state isolation per quest

## Critical Discoveries from Source Analysis

### 1. Region Names
- **Childhood Oakvale** uses region name `"StartOakVale"` NOT `"Oakvale"`
- StartOakVale is made up of three sections: StartOakValeEast, StartOakValeWest, StartOakValeMemorialGarden
- Region name is NOT always the geographical location name
- **Always verify region names** by checking level files (*.lev) in `data/Levels/` and working quest examples

### 2. Thread Execution
- Threads with `{region="RegionName"}` parameter auto-wait for region load
- **Do NOT manually check `IsRegionLoaded()`** in region-bound threads
- FSE engine handles thread lifecycle

### 3. Main() Function
- Must complete quickly and return
- **Never block with while loops** waiting for regions
- Do immediate setup (bindings, quest cards) then create threads

### 4. Entity Scripts
- Use lowercase `local` for Quest and Me variables
- Parameters are `questObject, meObject` (not `quest, entity`)
- Always call `NewScriptFrame(Me)` in loops
- Always call `ReleaseControl()` at end

### 5. Quest Card Objects
- Quest card object names (like `OBJECT_QUEST_CARD_WASP_MENACE`) contain the quest text
- Not defined in Lua - they're game assets
- Changing quest card object changes displayed quest name/description

## API Patterns from Examples

### Quest Lifecycle
```lua
function Init(questObject)
    -- Called once at game load
    Quest:AddQuestRegion(name, region)
    Quest:SetStateBool(name, value)
end

function Main(questObject)
    -- Called when quest activates
    -- Do immediate setup, create threads
end

function OnPersist(questObject, context)
    -- Called on save/load
    Quest:PersistTransferBool(context, "varName")
end
```

### Common API Calls
- `Quest:CreateThread(name, {region="RegionName"})` - Create region-bound thread
- `Quest:AddEntityBinding(name, path)` - Bind entity script
- `Quest:FinalizeEntityBindings()` - Complete binding process
- `Quest:CreateCreature(defName, position, scriptName)` - Spawn creature
- `Quest:GiveQuestCardDirectly(object, name, bool)` - Give quest card
- `Quest:KickOffQuestStartScreen(name, isStory, isGold)` - Show start screen
- `Quest:SetStateBool(name, value)` / `Quest:GetStateBool(name)` - State management
- `Quest:Pause(seconds)` - Pause execution
- `Quest:NewScriptFrame()` / `Quest:NewScriptFrame(Me)` - Yield control

### Entity API
- `Me:MakeBehavioral()` - Enable AI
- `Me:AcquireControl()` - Take control
- `Me:ReleaseControl()` - Release control
- `Me:IsTalkedToByHero()` - Check if hero initiated dialogue
- `Me:IsKilledByHero()` - Check if killed by hero
- `Me:SpeakAndWait(text)` - Display dialogue
- `Quest:EntitySetThingAsEnemyOfThing(a, b)` - Make entities hostile

## Node Definitions Reference

Node types found in working quests (for node-based editor):

### Trigger Nodes
- `onHeroTalks` - When hero talks to entity
- `onKilledByHero` - When entity killed by hero
- `onHitByHero` - When entity hit by hero

### Condition Nodes
- `checkHasItem` - Check if hero has item
- `checkYesNoAnswer` - Check yes/no dialogue response
- `checkStateBool` - Check boolean state variable

### Action Nodes
- `showDialogue` - Display dialogue text
- `takeItem` - Take item from hero
- `giveItem` - Give item to hero
- `becomeHostile` - Make entity hostile
- `completeQuest` - Mark quest complete
- `yesNoQuestion` - Ask yes/no question

### Flow Control Nodes
- `sequence` - Execute children in sequence
- `parallel` - Execute children in parallel
- `wait` - Wait for duration

## File Locations

### Development
- Project root: `D:\Documents\JetBrains\FQT\FQT\`
- Source references: `D:\Documents\JetBrains\FQT\SourceFilesToReference\`

### Game Deployment
- FSE directory: `C:/Programs/Steam/steamapps/common/Fable The Lost Chapters/FSE/`
- Game data: `C:/Programs/Steam/steamapps/common/Fable The Lost Chapters/data/`
- FinalAlbion.qst: `C:/Programs/Steam/steamapps/common/Fable The Lost Chapters/data/Levels/FinalAlbion.qst`

## Common Mistakes Found in Examples

Several example quests have bugs that demonstrate common mistakes:

1. **Quest:Wait() calls** - This API doesn't exist, should be `Quest:Pause()`
2. **Missing entity script files** - Entity bindings that reference non-existent files
3. **Wrong marker names** - Trying to get markers that don't exist in game
4. **Blocking while loops in Main()** - Causes quest to never complete initialization

These mistakes are documented to AVOID in generated code.

## Testing Workflow

Based on example quests:

1. Create quest Lua files in `FSE/QuestName/` directory
2. Create entity scripts in `FSE/QuestName/Entities/` directory
3. Register quest in `FSE/quests.lua`
4. Add quest to `data/Levels/FinalAlbion.qst`
5. Activate quest in `FSE/Master/FSE_Master.lua`
6. Launch game and check `FSE/FableScriptExtender.log` for errors

## Additional Resources

- Working examples in `SampleQuests/NewQuest/` folder
- Full development guide in `QUEST_DEVELOPMENT_GUIDE.md`
- Code generation templates in `Services/CodeGenerator.cs`
- Node definitions in `Data/NodeDefinitions.cs`
