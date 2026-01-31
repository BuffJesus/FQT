-- WaspBossLUA.lua

-- Define entity script names locally
local entityScriptsToBind = {
    { gameName = "GratefulVillagerSpawn", fileName = "GratefulVillagerSpawn" },
    { gameName = "WaspChaser",            fileName = "WaspChaser"            },
    { gameName = "WaspChaseWoman",        fileName = "WaspChaseWoman"        },
    { gameName = "WaspAttacker",          fileName = "WaspAttacker"          },
    { gameName = "WaspVictim",            fileName = "WaspVictim"            },
    { gameName = "FleeingWoman",          fileName = "FleeingWoman"          },
    { gameName = "WaspHelper",            fileName = "WaspHelper"            },
    { gameName = "QueenHornet",           fileName = "QueenHornet"           },
    { gameName = "HornetDrone",           fileName = "HornetDrone"           }
}

function Init(Quest)
     Quest:Log("--- WaspBossLUA: Init (Final Review) ---")

     local questName = "WaspBossLUA"
     local regionName = "PicnicArea"

     -- Add Quest Region
     Quest:AddQuestRegion(questName, regionName)
     Quest:Log("Added quest region: " .. regionName)

     -- Set Map Offset
     Quest:SetQuestWorldMapOffset(questName, 10, 10)
     Quest:Log("Set world map offset to (10, 10)")

     -- Initialize Persisted State Variables
     Quest:SetStateBool("QuestStartScreened", false)
     Quest:SetStateInt("SavedVillagerCount", 0)
     Quest:Log("Persistent quest state variables initialized.")

     -- Initialize Non-Persisted State Variables (matching C++ Init)
     Quest:SetStateBool("MissionSucceeded", false)
     Quest:SetStateBool("MissionFailed", false)
     Quest:SetStateBool("SaidLineOnce", false) -- From C++ *(_WORD *)&this->SaidLineOnce = 0;
     Quest:SetStateBool("CutsceneFinished", false)
     Quest:SetStateBool("StartChase", false)
     -- Add any others seen in C++ member list if needed (e.g., DronesCreated, QueenHornetAttacks)
     Quest:SetStateInt("DronesCreated", 0)
     Quest:SetStateInt("QueenHornetAttacks", 0)
     Quest:Log("Non-persistent state variables initialized.")


     -- Register and Reset Timer
     local timerId = Quest:RegisterTimer()
     if timerId ~= -1 then
         Quest:SetStateInt("ReachedWaspHelperTimerId", timerId)
         Quest:SetTimer(timerId, 0) -- Reset timer to 0 here in Init
         Quest:Log("Registered and reset ReachedWaspHelper timer (ID: " .. timerId .. ")")
     else
         Quest:Log("!!! WARNING: Failed to register ReachedWaspHelper timer!")
         Quest:SetStateInt("ReachedWaspHelperTimerId", -1)
     end
 end

function WaspIntro(Quest, unknownParam)
    Quest:Log("--- WaspBossLUA: WaspIntro called ---")
    Quest:SetStateBool("StartChase", true) -- Matches C++
    local victim = Quest:GetThing("WaspVictim")
    local hero = Quest:GetHero()
    if not victim or victim:IsNull() then Quest:Log("WaspIntro: Cannot find WaspVictim!"); return end
    if not hero or hero:IsNull() then Quest:Log("WaspIntro: Cannot find Hero!"); return end
    Quest:Log("WaspIntro: Ensuring Hero and Victim are behavioral...")
    hero:MakeBehavioral() -- Replaces C++ StartScriptingEntity
    victim:MakeBehavioral() -- Replaces C++ StartScriptingEntity
    local actors = { HERO = hero, VICTIM = victim } -- Matches C++ actor map
    Quest:Log("WaspIntro: Playing cutscene CS_WASPBOSS_INTRO...")
    Quest:PlayCutscene("CS_WASPBOSS_INTRO", actors) -- Wrapper handles movie/pause/macro/unpause
    Quest:Log("WaspIntro: Cutscene finished.")
    if not Quest:GetStateBool("QuestStartScreened") then -- Matches C++ check
        local questName = Quest:GetActiveQuestName()
        if questName == "WaspBossLUA" then
            Quest:Log("WaspIntro: Kicking off Quest Start Screen...")
            Quest:KickOffQuestStartScreen(questName, true, false) -- Matches C++ params (1, 0)
            Quest:SetStateBool("QuestStartScreened", true) -- Matches C++
        else
             Quest:Log("!!! WaspIntro: Active quest name mismatch? Expected WaspBossLUA, got " .. questName)
        end
    end
    Quest:Log("--- WaspBossLUA: WaspIntro finished ---")
end

function Main(Quest)
     Quest:Log("--- WaspBossLUA: Main (Final Review) ---")
     local questName = "WaspBossLUA"
     local first2Region = "First2" -- Explicitly use "First2"
     local lookoutPointRegion = "LookoutPoint"
     local picnicAreaLevel = "PicnicArea"
     local objective1TextKey = "TEXT_QUEST_WASP_MENACE_OBJECTIVE_01"
     local objective1Region1 = "HeroGuildComplexInside" -- From C++
     local gmMessageMapKey = "TEXT_QST_028_GUILDSEAL_WASP_MAP"
     local bossMusicId = 23 -- MUSIC_SET_BOSS

     -- 1. Set Timer
     local timerId = Quest:GetStateInt("ReachedWaspHelperTimerId")
     if timerId ~= -1 then Quest:SetTimer(timerId, 120); Quest:Log("Set timer "..timerId.." to 120.") else Quest:Log("Timer invalid!") end

     -- 2. Wait LookoutPoint
     Quest:Log("Waiting for region '" .. lookoutPointRegion .. "'...")
     while not Quest:IsRegionLoaded(lookoutPointRegion) do if not Quest:Wait(0.1) then Quest:Log("Terminated."); return end end
     Quest:Log("... Loaded.")

     -- 3. Pause
     Quest:Log("Pausing 1.0s...")
     Quest:Pause(1.0); if not Quest:Wait(0.0) then return end

     -- 4. Give Tutorial
     Quest:Log("Giving Camera Tutorial...")
     Quest:GiveHeroTutorial(6); if not Quest:Wait(0.0) then return end -- TUTORIAL_CATEGORY_CAMERA = 6

     -- 5. Bind Entities
     Quest:Log("Binding entities...")
     for _, entityInfo in ipairs(entityScriptsToBind) do Quest:AddEntityBinding(entityInfo.gameName, entityInfo.fileName) end

     -- 6. Finalize Bindings
     Quest:FinalizeEntityBindings(); Quest:Log("Bindings finalized."); if not Quest:Wait(0.0) then return end

     -- 7. Set Objective (Corrected Regions)
     Quest:Log("Setting objective 1...")
     Quest:SetQuestCardObjective(questName, objective1TextKey, objective1Region1, first2Region) -- Correct regions

     -- 8. GM Message (Corrected Graphic Name Region)
     Quest:Log("Sending GM map message...")
     Quest:HeroReceiveMessageFromGuildMaster(gmMessageMapKey, first2Region, true, true) -- Correct region/params

     -- 9. Wait PicnicArea
     Quest:Log("Waiting for level '" .. picnicAreaLevel .. "'...")
     while not Quest:IsLevelLoaded(picnicAreaLevel) do if not Quest:Wait(0.1) then Quest:Log("Terminated."); return end end
     Quest:Log("... Loaded."); if not Quest:Wait(0.0) then return end

     -- 10. Override Music
     Quest:Log("Overriding music...")
     Quest:OverrideMusic(bossMusicId, false, false) -- Correct params

     -- 11. Start Threads (Corrected Regions)
     Quest:Log("Starting threads...")
     Quest:CreateThread("WatchForTermination", { region = first2Region })
     Quest:Log("... Started WatchForTermination in " .. first2Region)
     Quest:CreateThread("DoMission", { region = picnicAreaLevel })
     Quest:Log("... Started DoMission in " .. picnicAreaLevel)
     Quest:CreateThread("WatchForCutscene", { region = first2Region })
     Quest:Log("... Started WatchForCutscene in " .. first2Region)

     Quest:Log("--- WaspBossLUA: Main finished setup. ---")
 end

function OnPersist(Quest, Context)
     Quest:Log("--- WaspBossLUA: OnPersist ---")

     -- Persist QuestStartScreened
     local currentQuestStartScreened = Quest:GetStateBool("QuestStartScreened")
     local loadedQuestStartScreened = Quest:PersistTransferBool(Context, "WaspBossLUA_QuestStartScreened", currentQuestStartScreened)
     Quest:SetStateBool("QuestStartScreened", loadedQuestStartScreened)
     -- Quest:Log("... 'QuestStartScreened' transferred. Value is now: " .. tostring(loadedQuestStartScreened))

     -- Persist SavedVillagerCount
     local currentSavedVillagerCount = Quest:GetStateInt("SavedVillagerCount")
     local loadedSavedVillagerCount = Quest:PersistTransferInt(Context, "WaspBossLUA_SavedVillagerCount", currentSavedVillagerCount)
     Quest:SetStateInt("SavedVillagerCount", loadedSavedVillagerCount)
     -- Quest:Log("... 'SavedVillagerCount' transferred. Value is now: " .. tostring(loadedSavedVillagerCount))

     Quest:Log("Persistence handled.")
 end

-- Placeholder Thread Functions
function WatchForTermination(Quest)
     -- ... (Log start) ...
     local questName = "WaspBossLUA"
     local defaultRegion = "Class" -- Matches "First2"
     local picnicAreaLevel = "PicnicArea"

     while true do -- Loop condition matches C++ implicitly
         local missionSucceeded = Quest:GetStateBool("MissionSucceeded")
         local missionFailed = Quest:GetStateBool("MissionFailed")
         if missionFailed or missionSucceeded then break end
         if not Quest:Wait(0.5) then -- Combines NewScriptFrame and IsActiveThreadTerminating check
              Quest:Log("WatchForTermination: Thread terminated externally.")
              return
         end
     end
     if not Quest:Wait(0.0) then -- Matches C++ post-loop termination check
         Quest:Log("WatchForTermination: Thread terminated just before handling result.")
         return
     end

     if Quest:GetStateBool("MissionFailed") then
         Quest:Log("WatchForTermination: Handling Mission Failed...")
         Quest:SetQuestAsFailed(questName, true, "", true) -- Matches C++ call
         Quest:StopOverrideMusic(false) -- Matches C++ call
         Quest:DeactivateQuestLater(questName, 0) -- Matches C++ call
         Quest:Log("...Quest set failed, music stopped, deactivation scheduled.")
         return
     end

     if Quest:GetStateBool("MissionSucceeded") then
         Quest:Log("WatchForTermination: Handling Mission Succeeded...")
         Quest:ResetPlayerCreatureCombatMultiplier() -- Matches C++ call
         local successExperience = 500 -- <<<--- STILL NEEDS CORRECT VALUE
         Quest:GiveHeroExperience(successExperience) -- Matches C++ call
         Quest:Log("...Gave Hero " .. successExperience .. " XP.")
         Quest:SetQuestAsCompleted(questName, true, false, false) -- Matches C++ call
         Quest:StopOverrideMusic(false) -- Matches C++ call
         Quest:FadeScreenIn() -- Matches C++ call
         Quest:Log("...Quest set completed, music stopped, screen fading in.")

         Quest:Log("...Waiting for PicnicArea level to unload...")
         while Quest:IsLevelLoaded(picnicAreaLevel) do -- Matches C++ wait loop
             if not Quest:Wait(0.5) then -- Combines NewScriptFrame and IsActiveThreadTerminating check
                 Quest:Log("... Thread terminated while waiting for PicnicArea unload.")
                 Quest:DeactivateQuestLater(questName, 0) -- Still try to deactivate
                 return
             end
         end
         Quest:Log("...PicnicArea unloaded.")
         if not Quest:Wait(0.0) then -- Matches C++ post-wait termination check
              Quest:Log("WatchForTermination: Thread terminated before final GM message.")
              Quest:DeactivateQuestLater(questName, 0)
              return
         end

         Quest:Log("...Sending final Guild Master message.")
         Quest:HeroReceiveMessageFromGuildMaster("TEXT_QST_078_GM_MSG_FIRST", defaultRegion, true, true) -- Matches C++ call
         Quest:DeactivateQuestLater(questName, 0) -- Matches C++ call
         Quest:Log("...Final GM message sent, quest deactivation scheduled.")
         return
     end
     Quest:Log("--- WaspBossLUA: WatchForTermination thread finished unexpectedly? ---")
 end

-- Helper function to check if all entities in a Lua table are dead
function AreAllThingsInVectorDead(Quest, entityTable)
    if not entityTable then return true end -- Empty table means all are dead

    local allDead = true
    for i, entity in ipairs(entityTable) do
        if entity and not entity:IsNull() then -- Check if entity pointer is valid
            local health = Quest:GetHealth(entity)
            -- Quest:Log("Checking health for entity #" .. i .. ": " .. health) -- Debug log
            if health > 0 then
                allDead = false -- Found a live one
                break -- No need to check further
            end
        end
    end
    -- Quest:Log("AreAllThingsInVectorDead returning: " .. tostring(allDead)) -- Debug log
    return allDead
end

-- Main quest logic thread
function DoMission(Quest)
    Quest:Log("--- WaspBossLUA: DoMission thread started (in PicnicArea) ---")

    local defaultRegion = "Class"

    -- 1. Setup Gossip/Rumours
    Quest:Log("Setting up gossip/rumours...")
    Quest:AddRumourCategory("Post waspboss killed")
    Quest:AddNewRumourToCategory("Post waspboss killed", "TEXT_AI_GOSSIP_WASPBOSS_KILLED")
    Quest:AddGossipFactionToCategory("Post waspboss killed", "FACTION_PICNIC_AREA")
    Quest:Log("... Gossip setup complete.")

    -- 2. Spawn Initial Drones (Loop 1 to 5)
    Quest:Log("Spawning initial 5 Hornet Drones...")
    -- *** CORRECTED DefName ***
    local droneDefName = "CREATURE_HORNET_PICNIC"
    local droneScriptName = "HornetDrone"
    for i = 1, 5 do
        local posName = "QueenDepositPos" .. tostring(i)
        local spawnPosThing = Quest:GetThing(posName)
        if spawnPosThing and not spawnPosThing:IsNull() then
             local spawnPos = spawnPosThing:GetPos() --<<<<< Make sure GetPos wrapper exists and works!
             if spawnPos then
                 Quest:Log("Spawning drone #" .. i .. " near " .. posName)
                 local newDrone = Quest:CreateCreature(droneDefName, spawnPos, droneScriptName)
                 if not newDrone or newDrone:IsNull() then
                     Quest:Log("!!! WARNING: Failed to create drone #" .. i)
                 end
             else
                 Quest:Log("!!! WARNING: Failed to get position for spawn point Thing: " .. posName)
             end
        else
            Quest:Log("!!! WARNING: Could not find spawn position Thing: " .. posName)
        end
        Quest:Wait(0.1)
        if not Quest:Wait(0.0) then Quest:Log("DoMission: Terminated during initial drone spawn."); return end
    end
    Quest:Log("... Initial drone spawning finished.")

    -- 3. Get Lists of Wasp Entities
    Quest:Log("Gathering lists of wasp entities...")
    local droneList = Quest:GetAllThingsWithScriptName("HornetDrone") or {}
    local chaserList = Quest:GetAllThingsWithScriptName("WaspChaser") or {}
    local attackerList = Quest:GetAllThingsWithScriptName("WaspAttacker") or {}

    local allWasps = {}
    for _, drone in ipairs(droneList) do table.insert(allWasps, drone) end
    for _, chaser in ipairs(chaserList) do table.insert(allWasps, chaser) end
    for _, attacker in ipairs(attackerList) do table.insert(allWasps, attacker) end
    Quest:Log("... Found " .. #allWasps .. " total initial wasps.")

    -- 4. Call WaspIntro (Needs implementation)
    Quest:Log("Calling WaspIntro (placeholder)...")
    -- WaspIntro(Quest, -1)

    -- 5. Wait for all initial wasps to be dead
    Quest:Log("Waiting for all initial wasps to be killed...")
    while not AreAllThingsInVectorDead(Quest, allWasps) do
        CheckGuildmasterHelp(Quest)
        if not Quest:Wait(1.0) then
            Quest:Log("DoMission: Thread terminated while waiting for initial wasps to die.")
            return
        end
    end
    Quest:Log("... All initial wasps are dead.")
    if not Quest:Wait(0.0) then Quest:Log("DoMission: Terminated after wasps died, before queen spawn."); return end

    -- 6. Pause before spawning Queen
    Quest:Log("Pausing for 4.0 seconds before spawning Queen...")
    Quest:Pause(4.0)
    if not Quest:Wait(0.0) then Quest:Log("DoMission: Terminated after pause, before queen spawn."); return end

    -- 7. Spawn Queen Hornet
    Quest:Log("Spawning Queen Hornet...")
    -- *** CORRECTED DefName ***
    local queenDefName = "CREATURE_HORNET_QUEEN_01"
    local queenScriptName = "QueenHornet"
    local queenSpawnMarkerName = "MK_WQ_STARTING"
    local queenSpawnPosThing = Quest:GetThing(queenSpawnMarkerName)

    if queenSpawnPosThing and not queenSpawnPosThing:IsNull() then
        local queenSpawnPos = queenSpawnPosThing:GetPos() --<<<<< Make sure GetPos wrapper exists and works!
        if queenSpawnPos then
             Quest:Log("Spawning Queen at marker " .. queenSpawnMarkerName)
             local queenHornet = Quest:CreateCreature(queenDefName, queenSpawnPos, queenScriptName)
             if queenHornet and not queenHornet:IsNull() then
                 Quest:Log("... Queen Hornet spawned successfully.")
                 Quest:SetStateBool("QueenHornetAttacks", true)
                 Quest:Log("Guildmaster Help checks will now run periodically.")
             else
                 Quest:Log("!!! ERROR: Failed to spawn Queen Hornet!")
                 -- Consider failing quest: Quest:SetStateBool("MissionFailed", true); return
             end
        else
            Quest:Log("!!! ERROR: Failed to get position for Queen spawn marker: " .. queenSpawnMarkerName)
            -- Consider failing quest: Quest:SetStateBool("MissionFailed", true); return
        end
    else
        Quest:Log("!!! ERROR: Could not find Queen Hornet spawn marker: " .. queenSpawnMarkerName)
        -- Consider failing quest: Quest:SetStateBool("MissionFailed", true); return
    end

    -- 9. Wait Loop (for Mission Success/Failure or Termination) & Periodic Checks
    Quest:Log("Entering main wait loop for mission end...")
    local helpCheckInterval = 3.0
    local timeSinceLastHelpCheck = helpCheckInterval

    while true do
        if Quest:GetStateBool("MissionSucceeded") or Quest:GetStateBool("MissionFailed") then
            Quest:Log("DoMission: Detected mission end flag. Exiting loop.")
            break
        end

        timeSinceLastHelpCheck = timeSinceLastHelpCheck + 0.5
        if timeSinceLastHelpCheck >= helpCheckInterval then
            CheckGuildmasterHelp(Quest)
            timeSinceLastHelpCheck = 0.0
        end

        local currentQueen = Quest:GetThing("QueenHornet")
        if currentQueen and not currentQueen:IsNull() then
             if Quest:GetHealth(currentQueen) <= 0 then
                 if not Quest:GetStateBool("MissionSucceeded") then
                     Quest:Log("DoMission: Queen Hornet killed! Setting MissionSucceeded.")
                     Quest:SetStateBool("MissionSucceeded", true)
                     break
                 end
             end
        elseif Quest:GetStateBool("QueenHornetAttacks") then
             if not Quest:GetStateBool("MissionSucceeded") and not Quest:GetStateBool("MissionFailed") then
                Quest:Log("!!! DoMission: Queen Hornet is unexpectedly null/gone! Setting MissionFailed.")
                Quest:SetStateBool("MissionFailed", true)
                break
             end
        end

        if not Quest:Wait(0.5) then
            Quest:Log("DoMission: Thread terminated during main wait loop.")
            return
        end
    end

    -- 10. Call EndMission (Needs implementation)
    Quest:Log("Calling EndMission (placeholder)...")
    -- EndMission(Quest)

    Quest:Log("--- WaspBossLUA: DoMission thread finished ---")
end

function DoOutro(Quest)
     Quest:Log("--- WaspBossLUA: DoOutro called ---")

     -- Check if already terminated (safety)
     if not Quest:Wait(0.0) then Quest:Log("DoOutro: Terminated before execution."); return end

     -- 1. Set PanickedVillagersScene flag (using state if needed)
     -- Quest:SetStateBool("PanickedVillagersScene", true) -- Matches C++
     Quest:Log("DoOutro: Setting PanickedVillagersScene flag (if applicable).")

     local hero = Quest:GetHero()
     if not hero or hero:IsNull() then Quest:Log("!!! DoOutro: Hero not found!"); return end

     -- 2. Make Hero behavioral
     Quest:Log("DoOutro: Ensuring Hero is behavioral...")
     hero:MakeBehavioral() -- Matches C++ StartScriptingEntity replacement

     -- Note: PlayCutscene handles StartMovieSequence and associated pausing/unpausing.
     -- Manual pauses/fades happen *outside* or *before* PlayCutscene.

     -- 3. Manually pause non-scripted entities (as per C++ vtable call at index 379)
     Quest:Log("DoOutro: Pausing non-scripted entities manually...")
     Quest:PauseAllNonScriptedEntities(true) -- Matches C++ vtable call with '1'

     -- 4. Pause script 3.0s
     Quest:Log("DoOutro: Pausing script for 3.0 seconds...")
     Quest:Pause(3.0)
     if not Quest:Wait(0.0) then Quest:Log("DoOutro: Terminated during 3.0s pause."); return end

     -- 5. Fade Screen Out
     Quest:Log("DoOutro: Fading screen out...")
     Quest:FadeScreenOut(0.5, 0.5) -- Matches C++ params (color defaults to black if wrapper allows)
     if not Quest:Wait(0.0) then return end -- Check termination

     -- 6. Pause script 0.5s
     Quest:Log("DoOutro: Pausing script for 0.5 seconds...")
     Quest:Pause(0.5)
     if not Quest:Wait(0.0) then Quest:Log("DoOutro: Terminated during 0.5s pause."); return end

     -- 7. Find Nearest Queen
     local queenDefName = "CREATURE_HORNET_QUEEN_01"
     local queenSpawnPos1 = Quest:GetThing("QueenDepositPos1")
     local queenHornet = nil -- Declare variable outside if
     if queenSpawnPos1 and not queenSpawnPos1:IsNull() then
         Quest:Log("DoOutro: Looking for nearest Queen Hornet...")
         queenHornet = Quest:GetNearestWithDefName(queenSpawnPos1, queenDefName) -- Matches C++
     else
          Quest:Log("!!! DoOutro: Could not find QueenDepositPos1 for Queen search reference.")
     end

     -- 8. Check if Queen is Alive (using GetHealth)
     if queenHornet and not queenHornet:IsNull() then
         local queenHealth = Quest:GetHealth(queenHornet)
         Quest:Log("DoOutro: Found Queen Hornet. Health: " .. tostring(queenHealth))
         -- 9. Remove Queen if Alive
         if queenHealth > 0 then
             Quest:Log("DoOutro: Removing alive Queen Hornet.")
             Quest:RemoveThing(queenHornet) -- Matches C++ call (0, 1 params)
         else
             Quest:Log("DoOutro: Queen Hornet already dead/removed.")
         end
     else
         Quest:Log("DoOutro: Queen Hornet not found.")
     end
     if not Quest:Wait(0.0) then Quest:Log("DoOutro: Terminated after Queen check/removal."); return end

     -- 10. Prepare Actors for Outro Cutscene
     local actors = { HERO = hero } -- Matches C++ actor map

     -- 11. Play Outro Cutscene
     Quest:Log("DoOutro: Playing cutscene CS_WASPBOSS_OUTRO...")
     -- PlayCutscene handles movie start/pause/macro/unpause/cleanup
     Quest:PlayCutscene("CS_WASPBOSS_OUTRO", actors)
     Quest:Log("DoOutro: Outro cutscene finished.")
     if not Quest:Wait(0.0) then Quest:Log("DoOutro: Terminated after Outro cutscene."); return end

     -- 12. Unpause Entities (Explicitly, matching C++ vtable call at index 379 with 0)
     Quest:Log("DoOutro: Unpausing non-scripted entities manually...")
     Quest:PauseAllNonScriptedEntities(false) -- Matches C++ vtable call with '0'

     -- 13. Set Villager Factions
     local picnicFaction = "FACTION_PICNIC_AREA"
     local villagers = { "VILL1", "VILL2", "VILL3" }
     Quest:Log("DoOutro: Setting villager factions...")
     for _, villagerName in ipairs(villagers) do
         local villager = Quest:GetThing(villagerName)
         if villager and not villager:IsNull() then
             Quest:Log("... Setting " .. villagerName .. " to faction " .. picnicFaction)
             Quest:EntitySetInFaction(villager, picnicFaction) -- Matches C++
         else
             Quest:Log("... Could not find villager: " .. villagerName)
         end
     end

     Quest:Log("--- WaspBossLUA: DoOutro finished ---")
 end

function EndMission(Quest)
    Quest:Log("--- WaspBossLUA: EndMission monitoring started ---")
    local questName = "WaspBossLUA"
    local defaultRegion = "Class"
    local picnicAreaLevel = "PicnicArea"
    local queenScriptName = "QueenHornet"
    local heroScriptName = "First2" -- Script name for the Hero used in MsgIsKilledBy

    local queenHornet = Quest:GetThing(queenScriptName)

    if not queenHornet or queenHornet:IsNull() then
        Quest:Log("!!! EndMission: Could not find QueenHornet at start! Cannot monitor.")
        -- Decide how to handle this - maybe set MissionFailed?
        -- Quest:SetStateBool("MissionFailed", true)
        return
    end

    -- Display Quest Info HUD
    Quest:DisplayQuestInfo(true)

    -- Wait until QueenHornetAttacks flag is set (safety check, should be set by DoMission)
    Quest:Log("EndMission: Waiting for QueenHornetAttacks flag...")
    while not Quest:GetStateBool("QueenHornetAttacks") do
        if not Quest:Wait(0.5) then Quest:Log("EndMission: Terminated while waiting for flag."); return end
    end
    Quest:Log("EndMission: QueenHornetAttacks flag confirmed.")
    if not Quest:Wait(0.0) then return end -- Check termination

    -- Set Objective 2 Text
    Quest:Log("EndMission: Setting objective 2 text.")
    Quest:SetQuestCardObjective(questName, "TEXT_QUEST_WASP_MENACE_OBJECTIVE_02", defaultRegion, defaultRegion)

    -- Add Queen Health Bar to HUD
    Quest:Log("EndMission: Adding Queen health bar.")
    local barColor = { R=0, G=0, B=0, A=255 } -- Black color from C++ code (alpha likely ignored)
    local barIcon = "HUD_ICON_WASP_HEAD"
    local healthBarId = Quest:AddQuestInfoBarHealth(queenHornet, barColor, barIcon, 1.0)
    Quest:Log("... Health bar added with ID: " .. tostring(healthBarId))

    -- Main Monitoring Loop
    Quest:Log("EndMission: Entering monitoring loop...")
    local missionComplete = false
    while true do
        if not Quest:Wait(0.2) then -- Check frequently but allow termination check
             Quest:Log("EndMission: Terminated during monitoring loop.")
             missionComplete = true -- Break loop on termination
             break
        end

        -- Check if PicnicArea is still loaded
        local picnicLoaded = Quest:IsLevelLoaded(picnicAreaLevel)

        if picnicLoaded then
            -- Check if Queen was killed by Hero
             -- Re-get Queen just in case? Or assume pointer is stable.
             -- local currentQueen = Quest:GetThing(queenScriptName)
             local currentQueen = queenHornet -- Assume stable for now

             if currentQueen and not currentQueen:IsNull() then
                -- *** Call MsgIsKilledBy on the entity object ***
                if currentQueen:MsgIsKilledBy(heroScriptName) then
                    Quest:Log("EndMission: Queen killed by Hero detected!")
                    Quest:DisplayQuestInfo(false) -- Hide HUD
                    Quest:RemoveQuestInfoElement(healthBarId) -- Remove health bar
                    Quest:Log("... HUD hidden, health bar removed.")
                    DoOutro(Quest) -- Call the outro logic
                    Quest:SetStateBool("MissionSucceeded", true) -- Set success flag
                    missionComplete = true
                    break -- Exit loop
                end
             else
                 -- Queen is null/gone but level is loaded? Might be an error or manual removal.
                 if Quest:GetStateBool("QueenHornetAttacks") and not Quest:GetStateBool("MissionSucceeded") then
                      Quest:Log("!!! EndMission: Queen is null/gone unexpectedly while level loaded! Assuming failure.")
                      Quest:SetStateBool("MissionFailed", true)
                      missionComplete = true
                      break
                 end
             end

        else -- PicnicArea is NOT loaded
             Quest:Log("EndMission: PicnicArea unloaded.")
             -- If level unloaded before Queen was killed, likely a failure or abnormal exit
             if not Quest:GetStateBool("MissionSucceeded") then
                Quest:Log("... Level unloaded before success. Setting MissionFailed.")
                Quest:SetStateBool("MissionFailed", true)
             end
             missionComplete = true
             break -- Exit loop
        end

        -- Optional: Add GuildmasterHelp check here too if needed during the fight itself
        -- CheckGuildmasterHelp(Quest)

    end -- End of while true

    -- Cleanup if loop exited but mission didn't succeed/fail normally (e.g., terminated)
    if missionComplete then
        Quest:Log("EndMission: Exiting monitoring loop.")
        -- Ensure HUD is hidden and bar removed if loop broken early
        Quest:DisplayQuestInfo(false)
        Quest:RemoveQuestInfoElement(healthBarId)
    end

    Quest:Log("--- WaspBossLUA: EndMission monitoring finished ---")
    -- Execution flow returns to DoMission, which should exit because MissionSucceeded/Failed is now set.
end

function WatchForCutscene(Quest)
     Quest:Log("--- WaspBossLUA: WatchForCutscene thread started ---")

     Quest:Log("WatchForCutscene: Waiting for QueenHornetAttacks flag...")
     while not Quest:GetStateBool("QueenHornetAttacks") do
         if not Quest:Wait(0.5) then Quest:Log("WatchForCutscene: Terminated while waiting."); return end
     end
     Quest:Log("WatchForCutscene: QueenHornetAttacks flag is true.")
     if not Quest:Wait(0.0) then Quest:Log("WatchForCutscene: Terminated immediately after flag check."); return end

     local queenHornet = Quest:GetThing("QueenHornet")
     if not queenHornet or queenHornet:IsNull() then Quest:Log("!!! WatchForCutscene: QueenHornet not found!"); return end

     Quest:Log("WatchForCutscene: Setting QueenHornet behaviour to NOT_PAUSED (2).")
     Quest:EntitySetCutsceneBehaviour(queenHornet, 2) -- Matches C++ call

     local hero = Quest:GetHero()
     if not hero or hero:IsNull() then Quest:Log("!!! WatchForCutscene: Hero not found!"); return end

     Quest:Log("WatchForCutscene: Ensuring Hero is behavioral...")
     hero:MakeBehavioral() -- Replaces C++ StartScriptingEntity

     local actors = { HERO = hero } -- Matches C++ actor map

     Quest:Log("WatchForCutscene: Playing cutscene CS_WASPBOSS_QUEEN...")
     -- PlayCutscene wrapper handles movie start/pause/macro/unpause/cleanup
     Quest:PlayCutscene("CS_WASPBOSS_QUEEN", actors)
     Quest:Log("WatchForCutscene: Cutscene finished.")

     Quest:Log("WatchForCutscene: Making Hero face QueenHornet.")
     Quest:EntitySetFacingAngleTowardsThing(hero, queenHornet) -- Matches C++ call

     Quest:Log("WatchForCutscene: Resetting camera.")
     Quest:CameraDefault() -- Matches C++ call

     Quest:Log("WatchForCutscene: Setting CutsceneFinished flag to true.")
     Quest:SetStateBool("CutsceneFinished", true) -- Matches C++ assignment

     Quest:Log("--- WaspBossLUA: WatchForCutscene thread finished ---")
 end

function CheckGuildmasterHelp(Quest)
     if not Quest:GetStateBool("CutsceneFinished") then return end -- Matches C++ start condition

     local queenHornet = Quest:GetThing("QueenHornet")
     if not queenHornet or queenHornet:IsNull() then return end

     if Quest:GetHealth(queenHornet) <= 0 then return end -- Replaces predicate/IsAlive check

     -- Send Guidance 10 (only once)
     if not Quest:GetStateBool("SaidHelpLine1") then
         Quest:Log("Guildmaster Help: Sending Guidance 10")
         Quest:HeroReceiveMessageFromGuildMaster("TEXT_QST_072_GUILDMASTER_GUIDANCE_10", "Class", true, true)
         Quest:SetStateBool("SaidHelpLine1", true)
     end

     local currentPhase = Quest:EntityGetBossPhase(queenHornet)

     -- Send Guidance 20 (Phase 3, only once)
     if currentPhase == 3 and not Quest:GetStateBool("SaidHelpLine2") then
         Quest:Log("Guildmaster Help: Phase 3 detected, sending Guidance 20")
         Quest:Pause(1.0) -- Matches C++ pause
         Quest:HeroReceiveMessageFromGuildMaster("TEXT_QST_072_GUILDMASTER_GUIDANCE_20", "Class", true, true)
         Quest:SetStateBool("SaidHelpLine2", true)
     end

     -- Send Guidance 30 (Phase 4, only once)
     if currentPhase == 4 and not Quest:GetStateBool("SaidHelpLine3") then
          Quest:Log("Guildmaster Help: Phase 4 detected, sending Guidance 30")
         Quest:Pause(1.0) -- Matches C++ pause
         Quest:HeroReceiveMessageFromGuildMaster("TEXT_QST_072_GUILDMASTER_GUIDANCE_30", "Class", true, true)
         Quest:SetStateBool("SaidHelpLine3", true)
     end

     -- Send Guidance 40 (Phase 6, only once)
     if currentPhase == 6 and not Quest:GetStateBool("SaidHelpLine4") then
          Quest:Log("Guildmaster Help: Phase 6 detected, sending Guidance 40")
         Quest:Pause(1.0) -- Matches C++ pause
         Quest:HeroReceiveMessageFromGuildMaster("TEXT_QST_072_GUILDMASTER_GUIDANCE_40", "Class", true, true)
         Quest:SetStateBool("SaidHelpLine4", true)
     end

     -- Send Guidance 50 (Near death, only once, Improved Logic)
     local queenHealth = Quest:GetHealth(queenHornet)
     -- C++ waits for EXACTLY 0 health after phase 6 check, Lua checks low health after phase 6 message sent
     if queenHealth > 0 and queenHealth < 10 and Quest:GetStateBool("SaidHelpLine4") and not Quest:GetStateBool("SaidHelpLine5") then
         Quest:Log("Guildmaster Help: Queen near death, sending Guidance 50")
         Quest:Pause(1.0) -- Matches C++ pause
         Quest:HeroReceiveMessageFromGuildMaster("TEXT_QST_072_GUILDMASTER_GUIDANCE_50", "Class", true, true)
         Quest:SetStateBool("SaidHelpLine5", true)
     end
 end