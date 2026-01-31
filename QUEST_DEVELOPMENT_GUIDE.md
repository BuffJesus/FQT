# FSE Quest Development Guide

## Overview

This guide documents critical patterns and lessons learned from developing custom quests for Fable Script Extender (FSE).

## Critical Quest Structure Patterns

### 1. Region Management

**CRITICAL:** The region name used must match what the game loads, not the geographical location name.

- **Childhood Oakvale**: Use `"StartOakVale"` (NOT "Oakvale") - Made up of three sections: StartOakValeEast, StartOakValeWest, StartOakValeMemorialGarden
- **Always verify region names** by checking working example quests and level files (*.lev)

```lua
function Init(questObject)
    Quest = questObject
    Quest:AddQuestRegion("QuestName", "StartOakVale")  -- Correct for childhood!
    -- Quest:AddQuestRegion("QuestName", "Oakvale")    -- WRONG - doesn't exist!
end
```

### 2. Main() Function Structure

**CRITICAL:** Main() must complete quickly and NOT block waiting for regions to load.

**CORRECT Pattern:**
```lua
function Main(questObject)
    Quest = questObject
    Quest:Log("Quest Main() started.")

    -- Immediate setup (no waiting)
    Quest:AddEntityBinding("EntityName", "QuestName/Entities/EntityName")
    Quest:FinalizeEntityBindings()

    -- Quest card and start screen (immediate)
    Quest:AddQuestCard("OBJECT_QUEST_CARD_WASP_MENACE", "QuestName", false, false)
    Quest:GiveQuestCardDirectly("OBJECT_QUEST_CARD_WASP_MENACE", "QuestName", true)
    Quest:KickOffQuestStartScreen("QuestName", true, true)

    -- Create region-bound threads (they auto-wait for region)
    Quest:CreateThread("EntitySpawner", {region="StartOakVale"})
    Quest:CreateThread("MonitorCompletion", {region="StartOakVale"})

    Quest:Log("Main() completed.")
end
```

**WRONG Pattern (blocks forever):**
```lua
function Main(questObject)
    Quest = questObject

    -- DON'T DO THIS IN MAIN():
    while true do
        if Quest:IsRegionLoaded("SomeRegion") then
            -- Setup here...
            break
        end
        if not Quest:NewScriptFrame() then return end
    end
end
```

### 3. Region-Bound Threads

**CRITICAL:** Threads marked with `{region="RegionName"}` are automatically delayed by FSE until that region loads. Do NOT manually check for region load in the thread.

**CORRECT Pattern:**
```lua
Quest:CreateThread("EntitySpawner", {region="StartOakVale"})

function EntitySpawner(questObject)
    Quest = questObject
    Quest:Log("EntitySpawner executing - StartOakVale is loaded!")

    -- Start spawning immediately - no region check needed
    local marker = Quest:GetThingWithScriptName("MK_MARKER_NAME")
    -- ... spawn logic
end
```

**WRONG Pattern:**
```lua
function EntitySpawner(questObject)
    Quest = questObject

    -- DON'T DO THIS - thread already waits for region automatically:
    while not Quest:IsRegionLoaded("StartOakVale") do
        Quest:Pause(0.1)
        if not Quest:NewScriptFrame() then return end
    end
    -- This loop will block forever!
end
```

### 4. Quest Card Objects

Quest card objects contain the quest name/description text shown to players. They are game assets, NOT defined in Lua.

Common quest card objects:
- `OBJECT_QUEST_CARD_HERO_SOULS_MOTHER` - "Collecting your Mother's Soul"
- `OBJECT_QUEST_CARD_WASP_MENACE` - Wasp quest text
- `OBJECT_QUEST_CARD_GENERIC` - Generic quest card
- `OBJECT_QUEST_CARD_GHOST_GRANNY_NECKLACE` - Ghost Granny text

```lua
-- The text player sees comes from the object, not your Lua code
Quest:GiveQuestCardDirectly("OBJECT_QUEST_CARD_WASP_MENACE", "MyQuestName", true)
```

### 5. Entity Script Structure

Entity scripts must follow this exact pattern:

```lua
-- EntityName.lua
local Quest = nil
local Me = nil

function Init(questObject, meObject)
    Quest = questObject
    Me = meObject
    Me:MakeBehavioral()      -- If entity should have AI
    Me:AcquireControl()      -- Take control of entity
end

function Main(questObject, meObject)
    Quest = questObject
    Me = meObject
    local hero = Quest:GetHero()

    -- Entity properties
    Quest:EntitySetAsDamageable(Me, false)  -- If invulnerable

    while true do
        -- Behavior triggers
        if Me:IsTalkedToByHero() then
            Me:SpeakAndWait("Hello!")
            -- ... dialogue logic
        end

        if Me:IsKilledByHero() then
            Quest:SetStateBool("QuestCompleted", true)
        end

        -- Loop control (REQUIRED)
        if Me:IsNull() then break end
        if not Quest:NewScriptFrame(Me) then break end
    end

    Me:ReleaseControl()  -- Always release at end
end
```

## File Structure

```
FSE/
├── quests.lua                      # Quest registry
├── QuestName/
│   ├── QuestName.lua              # Main quest script
│   └── Entities/
│       └── EntityName.lua         # Entity behavior scripts
```

### quests.lua Registration

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

### FinalAlbion.qst Registration

Add to `data/Levels/FinalAlbion.qst`:
```
AddQuest("QuestName", TRUE);
```

## API Patterns

### Quest Lifecycle API
```lua
-- Called once at game load
function Init(questObject)
    Quest = questObject
    Quest:AddQuestRegion("QuestName", "StartOakVale")  -- Use correct region for childhood
    Quest:SetStateBool("StateVar", false)
end

-- Called when quest activates
function Main(questObject)
    -- Setup and create threads
end

-- Called on save/load
function OnPersist(questObject, context)
    Quest:PersistTransferBool(context, "StateVar")
end
```

### Common Quest API Calls
```lua
-- Quest Card
Quest:GiveQuestCardDirectly("OBJECT_QUEST_CARD", "QuestName", true)
Quest:SetQuestCardObjective("QuestName", "Objective Text", "Region1", "Region2")
Quest:SetQuestGoldReward("QuestName", 500)
Quest:SetQuestRenownReward("QuestName", 500)

-- Start Screen
Quest:KickOffQuestStartScreen("QuestName", isStoryQuest, isGoldQuest)

-- Entity Spawning
Quest:AddEntityBinding("EntityName", "QuestName/Entities/EntityName")
Quest:FinalizeEntityBindings()
Quest:CreateCreature("CREATURE_DEF_NAME", position, "scriptName")

-- Quest Markers
Quest:SetThingHasInformation(entity, true)   -- Green glow
Quest:MiniMapAddMarker(entity, "EntityName")

-- Rewards
Quest:GiveHeroGold(amount)
Quest:GiveHeroExperience(amount)
Quest:GiveHeroRenownPoints(amount)
Quest:GiveHeroMorality(amount)
Quest:GiveHeroObject("OBJECT_NAME", 1)

-- Completion
Quest:SetQuestAsCompleted("QuestName", false, false, false)
Quest:DeactivateQuestLater("QuestName", 1.0)
```

### Entity API Calls
```lua
-- Entity Control
Me:MakeBehavioral()
Me:AcquireControl()
Me:TakeExclusiveControl()
Me:ReleaseControl()

-- Entity Triggers
Me:IsTalkedToByHero()
Me:IsKilledByHero()
Me:MsgIsHitByHero()
Me:IsNull()

-- Entity Actions
Me:SpeakAndWait("Dialogue text")
Quest:EntitySetThingAsEnemyOfThing(entity1, entity2)

-- Entity Properties
Quest:EntitySetAsDamageable(Me, false)
Quest:EntitySetAsKillable(Me, false)
Quest:SetThingPersistent(Me, true)
Me:SetToKillOnLevelUnload(true)
```

### Thread Control
```lua
-- In thread functions
Quest:Pause(seconds)
if not Quest:NewScriptFrame() then return end  -- Yield control
if not Quest:NewScriptFrame(Me) then break end -- For entity scripts
```

## Common Mistakes & Solutions

### Mistake 1: Wrong Region Name
**Problem:** Quest never activates, EntitySpawner never runs
**Solution:** Use correct region names like "StartOakVale" for childhood Oakvale. Verify region names by checking level files (*.lev) in `data/Levels/` directory and working quest examples

### Mistake 2: Blocking Main()
**Problem:** Quest start screen doesn't show, game freezes during load
**Solution:** Never use while loops in Main(), use region-bound threads instead

### Mistake 3: Manual Region Checks in Threads
**Problem:** Thread waits forever, entities never spawn
**Solution:** Remove while loops checking IsRegionLoaded() - FSE handles this automatically

### Mistake 4: Missing NewScriptFrame() Calls
**Problem:** Quest or entity behavior blocks/freezes
**Solution:** Always call `Quest:NewScriptFrame()` or `Quest:NewScriptFrame(Me)` in loops

### Mistake 5: Wrong Entity Script Parameters
**Problem:** Entity script errors or doesn't control entity
**Solution:** Use `questObject, meObject` parameters (not `quest, entity`)

### Mistake 6: No Entity Control Release
**Problem:** Entity behaves strangely after script ends
**Solution:** Always call `Me:ReleaseControl()` at end of entity Main()

## Testing Checklist

- [ ] Quest registered in `quests.lua` with unique IDs
- [ ] Quest registered in `FinalAlbion.qst` with `TRUE`
- [ ] Quest activated in `FSE_Master.lua` via `ActivateQuest()`
- [ ] Region name matches actual loaded region (not geographical name)
- [ ] Main() completes quickly (no blocking while loops)
- [ ] Threads use `{region="RegionName"}` parameter
- [ ] Entity scripts use correct `Init(questObject, meObject)` signature
- [ ] Entity loops call `NewScriptFrame(Me)` and check result
- [ ] Entity Main() calls `ReleaseControl()` at end
- [ ] Check FSE log for errors and execution flow

## Example: Working Quest Structure

See `SampleQuests/NewQuest/` folder for a complete working example that follows all these patterns correctly.

## FSE Log Analysis

Key log markers:
- `Quest registered with the game` - Quest loaded successfully
- `C++ Init() phase` - Init() function called
- `C++ Main() phase` - Main() function called
- `Thread 'Name' registered in slot #N for region 'RegionName'` - Thread created
- `Binding entity 'Name' to script` - Entity script loaded
- `!!! LUA RUNTIME ERROR` - Script error (read the stack trace)

If EntitySpawner thread starts but never logs completion:
1. Check region name is correct
2. Remove manual region waiting loops
3. Verify marker names exist in game

## Additional Resources

- FSE API reference: See example quests in `SourceFilesToReference/FSE/`
- Working examples: MyFirstQuest, MySecondQuest, MyThirdQuest
- Discord examples: MAssassinQuest pattern
- Node definitions: See `NodeDefinitions.cs` for available behavior nodes
