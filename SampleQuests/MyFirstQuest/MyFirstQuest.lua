
Quest = nil
local HERO_ABILITY_UNHOLY_POWER_SPELL = 19
g_AbilityCounterID = -1
g_AbilityCount = 0
if g_MyActiveLights == nil then
    g_MyActiveLights = {}
end

function Init(questObject)
    Quest = questObject
    Quest:Log("MyFirstQuest: Init phase started.")
    Quest:AddQuestRegion("MyFirstQuest", "BarrowFields")
    Quest:SetStateInt("timesSpokenTo", 0)
	
	if Quest:GetStateBool("playedProximityCutscene") == nil then 
        Quest:SetStateBool("playedProximityCutscene", false)
        Quest:Log("MyFirstQuest: Initialized 'playedProximityCutscene' flag to false.")
    end	
end

function Main(questObject)
    Quest = questObject
    Quest:Log("MyFirstQuest Main() started. Setting up entity bindings...")
    Quest:AddEntityBinding("NPCtoTalk", "MyFirstQuest/Entities/NPCtoTalk")
    Quest:AddEntityBinding("BanditArcher", "MyFirstQuest/Entities/BanditArcher")
    Quest:FinalizeEntityBindings()
    
	Quest:AddQuestCard("OBJECT_QUEST_CARD_WASP_MENACE", "MyFirstQuest", false, false)

	Quest:CreateThread("EnemyChecker", {region="BarrowFields"})
	Quest:CreateThread("MyTestFunction", {region="BarrowFields"})	
	Quest:CreateThread("WatchForSpell")
	
	local fatness = Quest:GetHeroFatness()
    Quest:Log("-> Hero Fatness: " .. tostring(fatness))
	
	Quest:Log("-> Reducing Hero Health by 200 (can kill)...")
    Quest:ChangeHeroHealthBy(-2, true, false)
    Quest:Log("   (Health change applied)")
	
	    Quest:Log("-> Shaking camera...")
    Quest:CameraShake(0.7, 1.5)
    Quest:Log("   (Camera shake initiated)")
	
	
	    local married = Quest:GetHeroHasMarried()
    Quest:Log("-> Hero Has Married: " .. tostring(married))
	
	    local murderedWife = Quest:GetHeroHasMurderedWife()
    Quest:Log("-> Hero Has Murdered Wife: " .. tostring(murderedWife))
	
	
	   local renown = Quest:GetHeroRenownLevel()
    Quest:Log("-> Hero Renown Level: " .. tostring(renown))
	
	    local titleId = Quest:GetHeroTitle()
    Quest:Log("-> Hero Title ID: " .. tostring(titleId)) 

	Quest:Log("-> Giving Hero Ability: Unholy Power (ID: " .. HERO_ABILITY_UNHOLY_POWER_SPELL .. ")")
    Quest:GiveHeroAbility(HERO_ABILITY_UNHOLY_POWER_SPELL, true)
    Quest:Log("   (Ability given)")
	
	--Quest:SetQuestGoldReward("MyFirstQuest", 5000)
	--Quest:SetQuestRenownReward("MyFirstQuest", 6000)
	
	Quest:GiveQuestCardDirectly("OBJECT_QUEST_CARD_HERO_SOULS_MOTHER", "MyFirstQuest", true)
	Quest:KickOffQuestStartScreen("MyFirstQuest", true, true)


	g_AbilityCounterID = Quest:AddQuestInfoCounter("HUD_ORB_QUEST_FEAT", 10, 1.0)
	Quest:DisplayQuestInfo(true)
	Quest:Log("Infobox added")
	Quest:CreateThread("CheckForAbility")
	Quest:CreateThread("CheckForFinish")
	
end

function OnPersist(Quest, Context)

    local currentTimesSpoken = Quest:GetStateInt("timesSpokenTo") 
    local loadedTimesSpoken = Quest:PersistTransferInt(Context, "FSE_MyFirstQuest_timesSpokenTo", currentTimesSpoken)
    Quest:SetStateInt("timesSpokenTo", loadedTimesSpoken)     

    Quest:Log("... 'timesSpokenTo' transferred. Value is now: " .. tostring(loadedTimesSpoken))
end

function MyTestFunction(Quest)

	local hero = Quest:GetHero()
    local npc = Quest:GetThingWithScriptName("NPCtoTalk")
	
    while true do
        if hero and npc then
            if Quest:IsDistanceBetweenThingsUnder(hero, npc, 3.0) then
			Quest:SetGlobalBool("g_PlayCutsceneRequest", true)
			Quest:Pause(0.1)
                return
            end
        end
		
		Quest:Pause(0.1)

        if not Quest:NewScriptFrame() then
           break
        end
    end
end

function EnemyChecker(Quest)

	local marker = Quest:GetThingWithScriptName("MK_CSCRIPTTHING")
	local markerPosTable = marker:GetPos()
	local guard = Quest:CreateCreature(
                "CREATURE_BS_GUARD_RED",
                markerPosTable,      
                "SpawnedGuard" 
            )
	local MyMarker = Quest:GetThingWithScriptName("MK_MOVETOHERE")
	local MYmarkerPosTable = MyMarker:GetPos()
	local hero1 = Quest:GetHero()
	local SpawnedGuard1 = Quest:GetThingWithScriptName("SpawnedGuard")
	SpawnedGuard1:SetToKillOnLevelUnload(true)
	local MyName = SpawnedGuard1:GetDefName()
	Quest:Log("... Def name " .. tostring(MyName))
	local MyPos = SpawnedGuard1:GetHomePos()
	Quest:Log("... Home Pos " .. tostring(MyPos))
	local MyHomeMap = SpawnedGuard1:GetHomeMapName()
	Quest:Log("... Home Map " .. tostring(MyHomeMap))
	local MyCurrentMap = SpawnedGuard1:GetCurrentMapName()
	Quest:Log("... Current Map " .. tostring(MyCurrentMap))
	

	Quest:CreateExperienceOrb(MYmarkerPosTable, 1000)
	
	local allFlamingBoxes = Quest:GetAllThingsWithScriptName("FlamingBox")
		Quest:Log("gotten boxes")
	if allFlamingBoxes ~= nil then
		for _, box in ipairs(allFlamingBoxes) do
				Quest:Log("tried to do effect on the boxes")
			Quest:CreateEffectOnThing("LARGEFIRE", box, "head")
		end
	end
	
	Quest:Log("getting the sign with def name nearest")
	local bordelloSign = Quest:GetNearestWithDefName(hero1, "OBJECT_BORDELLO_SIGN")
		if bordelloSign ~= nil then
			Quest:CreateEffectOnThing("LARGEFIRE", bordelloSign, "head")
	end

    while true do
		if SpawnedGuard1:MsgIsHitByHero() then

			Quest:EntitySetThingAsEnemyOfThing(hero1, SpawnedGuard1)
        end
		
        if not Quest:NewScriptFrame() then
           break
        end
    end
end

function WatchForSpell(Quest)
    while true do	        
        if Quest:MsgOnHeroCastSpell() == 19 then
				 g_AbilityCount = g_AbilityCount + 1
				 Quest:UpdateQuestInfoCounter(g_AbilityCounterID, g_AbilityCount, 10)
        end
		
		if g_AbilityCount >= 10 then
			Quest:DisplayQuestInfo(false)
		end
		
		Quest:Pause(0.1)

        if not Quest:NewScriptFrame() then
            break
        end
    end
end


function CheckForFinish(Quest)
    local enemies = Quest:GetAllThingsWithScriptName("ToKill")

	Quest:AddScreenTitleMessage("AlexanderTheAlright is tremendously fat.", 10, true)

    while true do
	
	if #enemies > 0 then
        local allAreDead = true
		for i, enemy in ipairs(enemies) do
            if enemy:IsAlive() then 
            allAreDead = false
                break
            end
		end
			
			
		if allAreDead then
            Quest:SetQuestAsCompleted("MyFirstQuest", true, false, false)
			Quest:DeactivateQuestLater("MyFirstQuest", 0.1)
			end
		end
	
		
        if not Quest:NewScriptFrame() then
            break
        end
    end
end
