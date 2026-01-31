-- GhostGrannyNecklaceLUA.lua
-- This is the main controller script for the quest.

Quest = nil

---------------------------------------------------------------------
-- INIT: Called once when the quest is first loaded.
---------------------------------------------------------------------
function Init(questObject)
    Quest = questObject
    Quest:Log("GhostGrannyNecklaceLUA: Init phase started.")

    -- Initialize all the quest state variables to false.
    Quest:SetStateValue("WifeNeededForCutscene", false)
    Quest:SetStateValue("HeroKnowsAboutNecklace", false)
    Quest:SetStateValue("NecklaceReturned", false)
    Quest:SetStateValue("BanditKilled", false)
    Quest:SetStateValue("WarnedGettingCold", false)
    Quest:SetStateValue("GuidedToBanditLocation", false)
    Quest:SetStateValue("GhostToRest", false)
    Quest:SetStateValue("QuestCardGiven", false)
end

---------------------------------------------------------------------
-- MAIN: Called once after Init(). Sets up the quest structure.
---------------------------------------------------------------------
function Main(questObject)
    Quest = questObject
    Quest:Log("GhostGrannyNecklaceLUA: Main() called. Setting up entities and threads...")

    --Quest:AddEntityBinding("GhostGrannyNecklace", "GhostGrannyNecklace")
    Quest:AddEntityBinding("GhostGrannySon", "GhostGrannySon")
    --Quest:AddEntityBinding("GhostGrannyDaughterInLaw", "GhostGrannyDaughterInLaw")
    Quest:FinalizeEntityBindings()

    Quest:CreateThread("CreateBandit", { region = "GreatwoodLake" })
    Quest:CreateThread("GhostGrannyHints", { region = "OrchardFarm" })
    Quest:CreateThread("MonitorQuest", { region = "OrchardFarm" })

    Quest:Log("GhostGrannyNecklaceLUA: Main() setup is complete. Quest is now running.")
end

---------------------------------------------------------------------
-- ONPERSIST: Called by the game when saving or loading.
---------------------------------------------------------------------
function OnPersist(questObject, context)
    Quest = questObject
    Quest:Log("GhostGrannyNecklaceLUA: OnPersist called.")

    local heroKnows = Quest:GetStateValue("HeroKnowsAboutNecklace")
    Quest:SetStateValue("HeroKnowsAboutNecklace", Quest:PersistTransferBool(context, "HeroKnowsAboutNecklace", heroKnows))

    local returned = Quest:GetStateValue("NecklaceReturned")
    Quest:SetStateValue("NecklaceReturned", Quest:PersistTransferBool(context, "NecklaceReturned", returned))

    local banditKilled = Quest:GetStateValue("BanditKilled")
    Quest:SetStateValue("BanditKilled", Quest:PersistTransferBool(context, "BanditKilled", banditKilled))
end

---------------------------------------------------------------------
-- THREAD: MonitorQuest
---------------------------------------------------------------------
function MonitorQuest(questObject)
    Quest = questObject
    Quest:Log("[Thread: MonitorQuest] Quest monitoring has started.")
    while true do
        if not Quest:Wait(0) then break end
        if Quest:GetStateValue("NecklaceReturned") == true then
            Quest:Log("[Thread: MonitorQuest] Necklace has been returned. Completing quest...")
            Quest:CompleteQuest(true)
            while Quest:IsRegionLoaded("OrchardFarm") do
                if not Quest:Wait(0) then return end
            end
            Quest:Log("[Thread: MonitorQuest] Player has left Orchard Farm. Deactivating quest.")
            Quest:DeactivateQuest(Quest:GetActiveQuestName(), 0)
            break
        end
    end
    Quest:Log("[Thread: MonitorQuest] Quest monitoring has finished.")
end

---------------------------------------------------------------------
-- THREAD: CreateBandit
---------------------------------------------------------------------
function CreateBandit(questObject)
    Quest = questObject
    Quest:Log("[Thread: CreateBandit] Thread started.")

    while not Quest:GetStateValue("HeroKnowsAboutNecklace") do
        if not Quest:Wait(0) then return end
    end
    Quest:Log("[Thread: CreateBandit] Hero knows about the necklace. Proceeding.")
    Quest:SetCreatureGeneratorsEnabled("GreatwoodLake", false)

    while true do
        if not Quest:Wait(0) then break end
        if Quest:IsObjectInHeroPossession("OBJECT_GHOST_GRANNY_NECKLACE") then
            break
        end
        if Quest:IsLevelLoaded("Greatwood_3") and not Quest:GetStateValue("BanditKilled") then
            local bandit = Quest:GetThing("GhostGrannyBandit")
            if bandit:IsNull() then
                Quest:Log("[Thread: CreateBandit] Spawning bandit.")
                local spawnPoint = Quest:GetThing("GhostGrannyBanditSpawn")
                bandit = Quest:CreateCreature("CREATURE_BANDIT_LIEUTENANT", spawnPoint:GetPos(), "GhostGrannyBandit")
                Quest:CreateThread("CheckBanditDead", { region = "BarrowFields", args = { bandit } })
                while Quest:IsLevelLoaded("Greatwood_3") do
                    if not Quest:Wait(0) then return end
                end
            end
        end
    end
    Quest:Log("[Thread: CreateBandit] Thread finished. Re-enabling creature generators.")
    Quest:SetCreatureGeneratorsEnabled("GreatwoodLake", true)
end

---------------------------------------------------------------------
-- THREAD: CheckBanditDead
---------------------------------------------------------------------
function CheckBanditDead(questObject, banditObject)
    Quest = questObject
    if banditObject == nil or banditObject:IsNull() then
        Quest:Log("[Monitor] ERROR: Received an invalid bandit object.")
        return
    end
    Quest:Log("[Monitor] Started monitoring bandit '" .. banditObject:GetDataString() .. "' for death.")
    while true do
        if not Quest:Wait(0) then break end
        if banditObject:IsKilledByHero() then
            Quest:Log("[Monitor] Bandit killed by Hero. Updating quest state.")
            Quest:SetStateValue("BanditKilled", true)
            Quest:SetQuestCardObjective(Quest:GetActiveQuestName(), "TEXT_QUEST_GHOST_GRANNY_NECKLACE_OBJECTIVE_03", "OrchardFarm", "")
            break 
        end
    end
    Quest:Log("[Monitor] Finished monitoring bandit.")
end

---------------------------------------------------------------------
-- THREAD: GhostGrannyHints
---------------------------------------------------------------------
function GhostGrannyHints(questObject)
    Quest = questObject
    Quest:Log("[Thread: GhostGrannyHints] Thread started.")

    -- Wait for the hero to know about the necklace before doing anything.
    while not Quest:GetStateValue("HeroKnowsAboutNecklace") do
        if not Quest:Wait(0) then return end
    end

    -- Wait until the correct level is loaded.
    while not Quest:IsLevelLoaded("Greatwood_3") do
        if not Quest:Wait(0) then return end
    end
    Quest:Log("[Thread: GhostGrannyHints] Greatwood_3 loaded. Initializing hints.")

    -- Get all the marker entities we'll need for this thread.
    local gettingColdTrigger = Quest:GetThing("M_GhostGrannyGettingColder")
    local murderPointTrigger = Quest:GetThing("M_GhostGrannyMurderPoint")
    local hero = Quest:GetHero()

    -- Create the ghost but keep her inactive for now.
    local ghostGranny = Quest:CreateCreature("CREATURE_GHOST_VILLAGER_FEMALE", gettingColdTrigger:GetPos(), "GhostGranny")
    Quest:EntitySetAllStategroupsEnabled(ghostGranny, false)
    Quest:EntitySetTargetable(ghostGranny, false)

    local triggerDistance = 5.0 -- A reasonable trigger distance.

    -- This is the main hint loop. It runs until the bandit is killed.
    while not Quest:GetStateValue("BanditKilled") do
        if not Quest:Wait(0) then break end

        -- Proximity Trigger 1: "Getting Colder" hint
        if Quest:IsDistanceBetweenThingsUnder(hero, gettingColdTrigger, triggerDistance) and not Quest:GetStateValue("WarnedGettingCold") then
            Quest:Log("[Thread: GhostGrannyHints] Hero near 'Getting Colder' trigger.")
            Quest:SetStateValue("WarnedGettingCold", true)
            
            Quest:CreateEffectAtPos("Ghost_Appear_01", gettingColdTrigger:GetPos())
            Quest:EntityTeleportToThing(ghostGranny, gettingColdTrigger)
            
            if Quest:GetHealth(ghostGranny) > 0 then
                ghostGranny:SpeakAndWait("TEXT_QST_026_GHOST_GETTING_COLD", 4)
            end
        end

        -- Proximity Trigger 2: Murder cutscene
        if Quest:IsDistanceBetweenThingsUnder(hero, murderPointTrigger, triggerDistance) and not Quest:GetStateValue("GuidedToBanditLocation") then
            Quest:Log("[Thread: GhostGrannyHints] Hero near 'Murder Point' trigger. Playing cutscene.")
            Quest:SetStateValue("GuidedToBanditLocation", true)
            
            local bandit = Quest:GetThing("GhostGrannyBandit")
            Quest:PlayCutscene("CS_GHOSTGRANNY_MURDERED", {
                BANDIT = bandit,
                HERO = hero,
                GRAN = ghostGranny
            })
        end
    end

    Quest:Log("[Thread: GhostGrannyHints] Bandit has been killed. Waiting for hero to talk to ghost.")
    
    -- Loop after bandit is dead, wait for player to talk to the ghost.
    while true do
        if not Quest:Wait(0) then break end
        
        if ghostGranny:IsTalkedToByHero() then
            Quest:Log("[Thread: GhostGrannyHints] Hero talked to ghost. Giving necklace.")
            
            -- Play the final cutscene where the ghost gives the hero the necklace.
            Quest:PlayCutscene("CS_GHOSTGRANNY_NECKLACE", {
                HERO = hero,
                GRAN = ghostGranny
            })

            -- Give the necklace object to the hero.
            Quest:GiveHeroObject("OBJECT_GHOST_GRANNY_NECKLACE", 1)
            
            -- The ghost's job is done.
            break
        end
    end

    -- Clean up the ghost entity.
    Quest:RemoveThing(ghostGranny)
    Quest:Log("[Thread: GhostGrannyHints] Thread finished.")
end