# Sample Quests

This directory contains working quest examples from FSE that demonstrate various quest patterns and techniques.

## Quest Examples Overview

### NewQuest
Custom example created for this tool

A complete, working example quest that demonstrates:
- ✅ Correct region management (uses "StartOakVale" for childhood start)
- ✅ Non-blocking Main() function
- ✅ Region-bound threads without manual region checks
- ✅ Entity spawning with fallback logic
- ✅ Entity behavior with dialogue and triggers
- ✅ Quest card and start screen setup
- ✅ Quest completion and rewards
- ✅ Proper entity control lifecycle

**⚠️ IMPORTANT:** This quest uses "StartOakVale" which is ONLY available at the start of a NEW GAME (during the childhood section). If you're testing on an existing save game that's past the childhood section, this quest will NEVER activate because StartOakVale never loads. For mid-game testing, modify the quest to use a different region like "BarrowFields" or "HeroGuild".

### Files

**NewQuest.lua** - Main quest script
- Shows correct Main() pattern (no blocking)
- Demonstrates entity binding
- Quest card setup
- Thread creation
- Completion logic

**Entities/Karen.lua** - Entity behavior script
- Talk trigger with item checking
- Kill trigger
- Proper loop control with NewScriptFrame(Me)
- Control lifecycle (Acquire → behavior → Release)

**quests.lua** - Quest registration example
- Shows how to register quest and entity scripts
- Includes unique IDs

**FinalAlbion.qst.example** - Game quest registry example
- Shows how to add quest to game's quest list

## How to Use These Examples

### For Development Reference
1. Study the patterns in NewQuest.lua for quest structure
2. Review Karen.lua for entity behavior patterns
3. Check comments explaining critical sections
4. Compare with QUEST_DEVELOPMENT_GUIDE.md for detailed explanations

### For Testing in Game
1. Copy NewQuest/ folder to: `C:/Programs/Steam/steamapps/common/Fable The Lost Chapters/FSE/`
2. Add NewQuest entry to `FSE/quests.lua`
3. Add `AddQuest("NewQuest", TRUE);` to `data/Levels/FinalAlbion.qst`
4. Activate quest in `FSE/Master/FSE_Master.lua` with `quest:ActivateQuest("NewQuest")`
5. Launch game and check FSE log for execution

### For Code Generation
- CodeGenerator.cs uses similar patterns
- Node system generates code matching these patterns
- Templates in NodeDefinitions.cs follow these structures

## Key Patterns Demonstrated

### 1. Region Management
```lua
-- Init: Register region
Quest:AddQuestRegion("NewQuest", "StartOakVale")  -- For childhood Oakvale

-- Main: Create region-bound thread
Quest:CreateThread("EntitySpawner", {region="StartOakVale"})

-- Thread: No manual region check needed!
function EntitySpawner(questObject)
    -- Executes automatically when region loads
    Quest:Log("Region is loaded, spawning entities...")
end
```

### 2. Non-Blocking Main()
```lua
function Main(questObject)
    -- Do immediate setup
    Quest:AddEntityBinding(...)
    Quest:GiveQuestCardDirectly(...)
    Quest:KickOffQuestStartScreen(...)

    -- Create threads
    Quest:CreateThread("EntitySpawner", {region="StartOakVale"})

    -- Return quickly - don't block!
end
```

### 3. Entity Behavior Loop
```lua
function Main(questObject, meObject)
    Quest = questObject
    Me = meObject

    while true do
        -- Check triggers
        if Me:IsTalkedToByHero() then
            -- Handle interaction
        end

        -- REQUIRED: Loop control at end
        if Me:IsNull() then break end
        if not Quest:NewScriptFrame(Me) then break end
    end

    -- REQUIRED: Release control
    Me:ReleaseControl()
end
```

## Common Mistakes to Avoid

❌ **Wrong:** Using "Oakvale" region at game start
```lua
Quest:AddQuestRegion("Quest", "Oakvale")  -- Won't work - doesn't exist!
```

✅ **Correct:** Using "StartOakVale" for childhood Oakvale
```lua
Quest:AddQuestRegion("Quest", "StartOakVale")  -- Correct for childhood start
-- Note: Verify region names by checking level files in data/Levels/
```

---

❌ **Wrong:** Blocking while loop in Main()
```lua
function Main(questObject)
    while not Quest:IsRegionLoaded("Region") do
        Quest:Pause(0.1)
        if not Quest:NewScriptFrame() then return end
    end
    -- Setup here - but region never loads!
end
```

✅ **Correct:** Immediate setup, region-bound threads
```lua
function Main(questObject)
    Quest:AddEntityBinding(...)
    Quest:CreateThread("Spawner", {region="StartOakVale"})
end

function Spawner(questObject)
    -- Automatically waits for region
end
```

---

❌ **Wrong:** Missing NewScriptFrame() in entity loop
```lua
while true do
    if Me:IsTalkedToByHero() then
        -- ...
    end
    -- Missing NewScriptFrame() - will freeze game!
end
```

✅ **Correct:** Always call NewScriptFrame(Me)
```lua
while true do
    if Me:IsTalkedToByHero() then
        -- ...
    end
    if not Quest:NewScriptFrame(Me) then break end
end
```

---

## MyFirstQuest
Basic quest from FSE demonstrating fundamental patterns:
- Entity spawning and binding
- Basic dialogue interactions
- NPC and enemy behaviors
- Quest start screen setup

## MySecondQuest
Bulletin board quest showing:
- Item monitoring in threads
- Interactive bulletin board entity
- Gift-giving NPC behavior
- State tracking across interactions

## MyThirdQuest
Minimal quest structure demonstrating:
- Simplified quest setup
- Basic quest lifecycle
- Essential patterns only

## GhostGrannyNecklaceLUA
Complex multi-entity quest featuring:
- Multiple coordinated NPCs
- Conversation trees with multiple dialogue branches
- Inter-entity relationships
- Family quest storyline

## WaspBossLUA
Boss fight quest demonstrating:
- Timer management
- Complex state tracking
- Boss battle mechanics
- Combat event handling

## DemonDoorLUA
Interactive door quest showing:
- Requirement checking (gold, items)
- Conditional access systems
- Door entity behavior
- Reward gating

---

## See Also

- **QUEST_DEVELOPMENT_GUIDE.md** - Complete development guide with best practices
- **DISCORD_DEVELOPER_PATTERNS.md** - Community patterns including MAssassinQuest and helper quests
- **SOURCE_FILES_SUMMARY.md** - Reference files documentation
- **FSE_Source/** - Complete FSE C++ source code
