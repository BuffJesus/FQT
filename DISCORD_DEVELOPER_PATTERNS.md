# FSE Discord Developer Patterns and Examples

This document contains valuable patterns and examples shared by FSE developers on the official Discord server.

## Table of Contents
1. [Getting Started - Basic Quest Setup](#getting-started)
2. [Helper Quest Pattern for Start Screens](#helper-quest-pattern)
3. [MAssassinQuest - Complete Working Example](#massassinquest-example)
4. [Advanced Patterns](#advanced-patterns)

---

## Getting Started - Basic Quest Setup {#getting-started}

**From AeoN (AlbionSecrets):**

> First thing to do is create the files.
>
> Add your script in quests.lua and create a lua file for it with the same name.
> Then add this quest name to FinalAlbion.qst
> Thats always the first step for all scripts
> Then you can start with simple logic in the Main function just to test stuff

### Required Steps for Every Quest:
1. **Add entry to `quests.lua`** with quest name, file path, unique ID, and entity scripts
2. **Create quest Lua file** with matching name
3. **Add quest to `FinalAlbion.qst`** with `AddQuest("QuestName", TRUE);`
4. **Test with simple Main() logic** before adding complex features

---

## Helper Quest Pattern for Start Screens {#helper-quest-pattern}

**From MakhnoBlazed:**

Helper quests are separate, always-active quests that handle quest card registration for the Map Table. This pattern ensures quest cards appear properly in the game's quest log system.

### MAssassinQuestHelper Example

```lua
-- MAssassinQuestHELPER.lua
function Init(questObject)
    Quest = questObject
    questObject:Log("Quest Helper Running.")
    -- Add quest card to Map Table
    questObject:AddQuestCard("OBJECT_MAssassinQuestCARD", "MAssassinQuest", true, true)
    questObject:Log("Quest Card added to Map Table?")
    questObject:Log("Quest Helper Done")
end

function Main(questObject)
    -- Empty - helper only needs Init()
end
```

### quests.lua Registration for Helper

```lua
MAssassinQuestHELPER = {
    name = "MAssassinQuestHELPER",
    file = "MAssassinQuestHELPER/MAssassinQuestHELPER",
    section = "S_ALWAYS_ACTIVE",  -- CRITICAL: Makes it run always
    id = 67,
    master = false,
    entity_scripts = {}
}
```

### Key Points:
- Helper quest uses `section = "S_ALWAYS_ACTIVE"` to run at all times
- Helper handles `AddQuestCard()` to register quest in Map Table
- Main quest can focus on gameplay logic
- Helper needs unique ID separate from main quest

---

## MAssassinQuest - Complete Working Example {#massassinquest-example}

**From MakhnoBlazed:**

A complete boss fight quest demonstrating enemy spawning, combat, music override, and quest completion.

### Main Quest File

#### quests.lua Registration

```lua
MAssassinQuest = {
    name = "MAssassinQuest",
    file = "MAssassinQuest/MAssassinQuest",
    section = "S_GUILDQUESTS",
    id = 65,
    master = false,
    entity_scripts = {
        { name = "MAssassinNPC", file = "MAssassinQuest/Entities/MAssassinNPC", id = 66 },
    }
}
```

#### MAssassinQuest.lua - Main() Function

**NOTE:** This example uses a blocking while loop in Main() - see comments for why this pattern may cause issues.

```lua
function Main(Quest)
    Quest:Log("MAssassinQuestLUA Main() started. Setting up entity bindings...")

    -- NOTE: Blocking while loop waiting for region
    -- This pattern may prevent start screen from showing immediately
    while true do
        if Quest:IsRegionLoaded("BarrowFields") then
            Quest:AddEntityBinding("MAssassinNPC", "MAssassinQuest/Entities/MAssassinNPC")
            Quest:FinalizeEntityBindings()
            Quest:CreateThread("CheckForFinish")
            Quest:KickOffQuestStartScreen("MAssassinQuest", true, true)
            break
        end

        if not Quest:NewScriptFrame() then
            break
        end
    end
end
```

**Alternative Non-Blocking Pattern:**
```lua
function Main(Quest)
    Quest:Log("MAssassinQuestLUA Main() started. Setting up entity bindings...")

    -- Do immediate setup
    Quest:AddEntityBinding("MAssassinNPC", "MAssassinQuest/Entities/MAssassinNPC")
    Quest:FinalizeEntityBindings()
    Quest:KickOffQuestStartScreen("MAssassinQuest", true, true)

    -- Create region-bound thread (auto-waits for region)
    Quest:CreateThread("CheckForFinish", {region="BarrowFields"})
end
```

#### CheckForFinish Thread - Enemy Monitoring

```lua
function CheckForFinish(Quest)
    local MainNPC = Quest:GetThingWithScriptName("MAssassinNPC")
    local enemies
    local successExperience = 10000

    -- Wait for enemies to spawn
    while true do
        Quest:Log("CheckForFinish: Looking for 'SpawnedBandit' enemies...")
        local foundEnemies = Quest:GetAllThingsWithScriptName("SpawnedBandit")

        if #foundEnemies > 0 then
            Quest:Log("CheckForFinish: Found " .. #foundEnemies .. " enemies. Starting monitoring.")
            enemies = foundEnemies
            break
        end

        if not Quest:NewScriptFrame() then
            break
        end
    end

    -- Monitor until all enemies are dead
    while true do
        local allEnemiesAreDead = true
        for i, enemy in ipairs(enemies) do
            if enemy and enemy:IsAlive() then
                allEnemiesAreDead = false
                break
            end
        end

        local npcIsDead = false
        if MainNPC and not MainNPC:IsAlive() then
            npcIsDead = true
        end

        if allEnemiesAreDead and npcIsDead then
            Quest:Log("CheckForFinish: All enemies AND MainNPC are dead. Completing quest.")
            Quest:ResetPlayerCreatureCombatMultiplier()
            Quest:GiveHeroExperience(successExperience)
            Quest:GiveHeroGold("5000", 1)
            Quest:SetQuestAsCompleted("MAssassinQuest", true, false, false)
            Quest:DeactivateQuestLater("MAssassinQuest", 0.1)
            Quest:Log("[DEBUG] All enemies defeated — fading screen in.")
            Quest:FadeScreenIn()
            Quest:StopOverrideMusic(false)
            Quest:Log("[DEBUG] Quest completed successfully (all enemies dead).")
            break
        end

        if not Quest:NewScriptFrame() then
            break
        end
    end
end
```

**Key Patterns:**
- `Quest:GetAllThingsWithScriptName()` - Get array of all entities with script name
- Loop monitoring with `entity:IsAlive()` checks
- `Quest:ResetPlayerCreatureCombatMultiplier()` - Reset combat stats
- `Quest:FadeScreenIn()` / `Quest:FadeScreenOut()` - Screen effects
- `Quest:OverrideMusic()` / `Quest:StopOverrideMusic()` - Music control

### Entity Script - MAssassinNPC.lua

#### Init Function

```lua
local heroME = nil
local itsME = nil
local BanditCampTwinbladeKilled = false
local AlreadySpawned = false
local MusicPlayed = false -- safeguard to prevent music from restarting

function Init(questObject, meObject)
    meObject:MakeBehavioral()
    meObject:AcquireControl()
    questObject:Log("MAssassinNPC (Reader) '" .. meObject:GetDataString() .. "' initialized.")
    BanditCampTwinbladeKilled = questObject:GetMasterGameState("BanditCampTwinbladeKilled")
    questObject:Log("MAssassinNPC (Reader): Initial check for twinblade alive/dead status: " .. tostring(BanditCampTwinbladeKilled))
end
```

**Key Patterns:**
- `meObject:GetDataString()` - Get entity's data string for logging
- `questObject:GetMasterGameState()` - Read global game state flags
- Local variables at file scope persist throughout entity lifecycle

#### Main Function - Dialogue and Combat Triggers

```lua
function Main(questObject, meObject)
    questObject:Log("Entity Main: Running setup logic...")
    heroME = questObject:GetHero()
    itsME = questObject:GetThingWithScriptName("MAssassinNPC")
    local marker = questObject:GetThingWithScriptName("BA04")
    local pos = marker:GetPos()

    while true do
        -- Player talks to NPC
        if meObject:IsTalkedToByHero() then
            if not AlreadySpawned then
                meObject:MakeBehavioral()
                meObject:AcquireControl()
                meObject:SpeakAndWait("Oi Guild Puppet. You Murdered the great Twinblade! Prepare to die!", 4)
                questObject:Log("NPC finished speaking Success")
                questObject:EntitySetThingAsEnemyOfThing(itsME, heroME)

                -- Play boss music after speech, only once
                if not MusicPlayed then
                    local bossMusicId = 23 -- MUSIC_SET_BOSS
                    questObject:OverrideMusic(bossMusicId, false, false)
                    MusicPlayed = true
                end

                meObject:ReleaseControl()
                questObject:Log("Main NPC set as enemy Success")

                questObject:Log("Creating 3 Bandits")
                questObject:CreateCreature("CREATURE_BANDIT_GRUNT_LEVEL2_C", pos, "SpawnedBandit")
                questObject:CreateCreature("CREATURE_BANDIT_GRUNT_LEVEL2_C", pos, "SpawnedBandit")
                questObject:CreateCreature("CREATURE_BANDIT_GRUNT_LEVEL2_C", pos, "SpawnedBandit")
                AlreadySpawned = true
                questObject:Log("Bandits marked as spawned")
            else
                questObject:Log("Bandits already spawned; skipping new spawn.")
            end
        end

        -- Player hits NPC
        if meObject:MsgIsHitByHero() then
            questObject:Log("MAssassinNPC hit by hero.")
            meObject:MakeBehavioral()
            meObject:AcquireControl()
            meObject:SpeakAndWait("Figures that the Guild Puppet who killed Twinblade would lay their vile hands upon me, Die!", 4)

            -- Play boss music after speech, only once
            if not MusicPlayed then
                local bossMusicId = 23 -- MUSIC_SET_BOSS
                questObject:OverrideMusic(bossMusicId, false, false)
                MusicPlayed = true
            end

            questObject:EntitySetThingAsEnemyOfThing(itsME, heroME)
            questObject:Log("Main NPC set as enemy Success")

            if AlreadySpawned == false then
                questObject:Log("Creating 3 Bandits")
                questObject:CreateCreature("CREATURE_BANDIT_GRUNT_LEVEL2_C", pos, "SpawnedBandit")
                questObject:CreateCreature("CREATURE_BANDIT_GRUNT_LEVEL2_C", pos, "SpawnedBandit")
                questObject:CreateCreature("CREATURE_BANDIT_GRUNT_LEVEL2_C", pos, "SpawnedBandit")
                AlreadySpawned = true
                questObject:Log("Bandits marked as spawned")
            end
            meObject:ReleaseControl()
        end

        -- Safely exit the script frame
        if not questObject:NewScriptFrame(meObject) then
            break
        end
    end
end
```

**Key Patterns:**
- `meObject:SpeakAndWait(text, duration)` - Display dialogue with timeout
- `questObject:OverrideMusic(musicId, bool, bool)` - Change background music
- `questObject:CreateCreature(defName, position, scriptName)` - Spawn at position
- Use boolean flags (`AlreadySpawned`, `MusicPlayed`) to prevent duplicate actions
- Re-acquire control before making NPC speak or change behavior
- Release control after behavior changes

---

## Advanced Patterns {#advanced-patterns}

### Yes/No Questions with Answer Handling

**From AeoN (AlbionSecrets):**

```lua
if currentState == "MOVING" then
    if isTalkedTo then
        questObject:StartMovieSequence()
        meObject:Speak(heroME, "You seem dumb.")
        questObject:SetStateBool("talker_spoken_to", true)

        -- GiveHeroYesNoQuestion returns: 0, 1, or 2
        local answer = questObject:GiveHeroYesNoQuestion("Are you dumb?","Yeah", "Hell no", "Not sure")

        if answer == 2 then
            meObject:Speak(heroME, "Ah figured")
            questObject:GiveHeroObject("OBJECT_CHOCOLATE_BOX_01")
            meObject:Speak(heroME, "Now go away dummy")
        elseif answer == 1 then
            meObject:Speak(heroME, "Sure buddy.")
        elseif answer == 0 then
            meObject:Speak(heroME, "Oh trust me, you are!")
            questObject:EntitySetThingAsEnemyOfThing(itsME, heroME)
        end

        questObject:EndMovieSequence()
    end
end
```

**Key Points:**
- `GiveHeroYesNoQuestion(question, option1, option2, option3)` returns 0, 1, or 2
- Returns correspond to button choices (left to right: 0, 1, 2)
- `StartMovieSequence()` / `EndMovieSequence()` create cutscene-like moments
- `meObject:Speak(target, text)` - Non-blocking dialogue (vs `SpeakAndWait`)

### OnPersist Function

**From Discord discussion:**

> OnPersist is called when the game saves/loads to persist quest state variables across save files.

```lua
function OnPersist(questObject, context)
    Quest = questObject
    -- Transfer boolean states
    Quest:PersistTransferBool(context, "QuestCompleted")
    Quest:PersistTransferBool(context, "EnemiesSpawned")
    Quest:PersistTransferBool(context, "BossDefeated")

    -- Transfer numeric states
    Quest:PersistTransferInt(context, "KillCount")
end
```

### State Management with Master Game States

```lua
-- Read global game state (persists across all quests)
local twinbladeKilled = questObject:GetMasterGameState("BanditCampTwinbladeKilled")

-- Set global game state
questObject:SetMasterGameState("CustomGlobalFlag", true)
```

**Use Cases:**
- Track major story events (boss kills, quest completions)
- Check prerequisites for quest activation
- Create quest chains that depend on previous quest outcomes

### ShowMessageWithButtons

**From Discord discussion:**

```lua
-- Note: Return behavior uncertain, may require ActionButton polling
-- Best to use GiveHeroYesNoQuestion for confirmed button response handling
```

**Known Issues:**
- `ShowMessageWithFont()` is currently broken due to CWideString structure issues
- Prefer `GiveHeroYesNoQuestion()` for reliable button-based interactions

---

## Music Control

```lua
-- Music IDs
local MUSIC_SET_BOSS = 23  -- Boss fight music

-- Override current music
questObject:OverrideMusic(MUSIC_SET_BOSS, false, false)

-- Stop music override and return to normal
questObject:StopOverrideMusic(false)
```

---

## Screen Effects

```lua
-- Fade screen to black
Quest:FadeScreenOut()

-- Fade back from black
Quest:FadeScreenIn()

-- Movie sequences (disable player control, enable cinematic feel)
Quest:StartMovieSequence()
-- ... dialogue and events ...
Quest:EndMovieSequence()
```

---

## Combat and Enemy Management

```lua
-- Make two things enemies
questObject:EntitySetThingAsEnemyOfThing(npc, hero)

-- Check if entity is alive
if enemy:IsAlive() then
    -- ...
end

-- Get all entities with specific script name
local enemies = Quest:GetAllThingsWithScriptName("SpawnedBandit")
for i, enemy in ipairs(enemies) do
    if enemy and enemy:IsAlive() then
        -- Enemy still alive
    end
end

-- Reset combat multipliers (useful after boss fights)
Quest:ResetPlayerCreatureCombatMultiplier()
```

---

## Best Practices from Discord Community

1. **Always log extensively** - Use `Quest:Log()` to track execution flow
2. **Use helper quests** - Separate quest card registration from gameplay logic
3. **Boolean safeguards** - Prevent duplicate spawns/music with boolean flags
4. **Re-acquire control** - Call `MakeBehavioral()` and `AcquireControl()` before behavior changes
5. **Release control** - Always release at end of entity scripts
6. **NewScriptFrame checks** - Always check return value: `if not Quest:NewScriptFrame(Me) then break end`
7. **Region-bound threads** - Use `{region="RegionName"}` parameter for automatic region waiting
8. **Unique IDs** - Ensure all quests and entity scripts have unique IDs in quests.lua

---

## Additional Resources

- **FSE GitHub:** https://github.com/AeoN-Albion/FableScriptExtender
- **Official Discord:** Join for community support and examples
- **Working Examples:** See `SampleQuests/` directory for complete quest implementations
- **Development Guide:** See `QUEST_DEVELOPMENT_GUIDE.md` for comprehensive patterns

---

*Document compiled from FSE Discord server discussions (January 2026)*
*Contributors: AeoN (AlbionSecrets), MakhnoBlazed, odarenkoas*
