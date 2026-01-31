-- GrannySon.lua
-- This script controls the behavior of the ghost's son.

Quest = nil
Me = nil
isSetup = false
hasSpoken = false
talkCounter = 0

---------------------------------------------------------------------
-- INIT: Called once when the entity is loaded.
---------------------------------------------------------------------
function Init(questObject, meObject)
    Quest = questObject
    Me = meObject
    Quest:Log("GhostGrannySon '" .. Me:GetDataString() .. "' initialized.")
end

---------------------------------------------------------------------
-- MAIN: The main lifecycle loop for this entity.
---------------------------------------------------------------------
function Main(questObject, meObject)
    Quest = questObject
    Me = meObject

    -- This block runs only once to set up the NPC's initial state.
    if not isSetup then
        Quest:Log("GrannySon: Performing one-time setup.")
        
        -- Make the NPC non-killable and take control of his AI.
        Me:MakeBehavioral()
		--Quest:Log("Made Behavioral.")
        --Me:TakeExclusiveControl()
		--Quest:Log("Took exclusive control")
        --Quest:EntitySetAsKillable(Me, false)
		--Quest:Log("Set as killable")
        --Quest:EntitySetOpinionReactionMask(Me, "OPINION_REACTION_MASK_EX_SCRIPT_ACQUAINTANCE")
		--Quest:Log("Set opinion reaction mask")
        
        -- Turn to face the hero
        --local hero = Quest:GetHero()
        --Quest:EntitySetFacingAngleTowardsThing(Me, hero)
		--Quest:Log("Set facing angle")

        isSetup = true
		Quest:Log("Setup complete")
    end

    -- This is the main loop that runs every frame.
    while true do
        if not Quest:Wait(0) then break end

        -- Check if the player is trying to talk to the Son or give him the necklace.
        local wasItemPresented = Me:MsgIsPresentedWithItem()
        local presentedItemName = g_PresentedItemName -- Get the item name from the global variable
        g_PresentedItemName = nil -- Clear it immediately

        local shouldTalk = false
        if Me:IsTalkedToByHero() then
            shouldTalk = true
        elseif wasItemPresented and presentedItemName == "OBJECT_GHOST_GRANNY_NECKLACE" then
            shouldTalk = true
        end

        if shouldTalk then
            HandleConversation()
        end

        -- This part handles the NPC running away if attacked.
        if Me:MsgIsHitByHero() or Me:MsgIsHitByAnySpecialAbilityFromHero() then
            if not Me:MsgIsHitByHealLifeFromHero() then -- Don't run away if being healed
                HandleFleeing()
                break -- Exit the main loop as the NPC is now gone.
            end
        end
    end
end

---------------------------------------------------------------------
-- Handles the main conversation flow.
---------------------------------------------------------------------
function HandleConversation()
    Quest:Log("GrannySon: Starting conversation with Hero.")

    if Quest:GetStateValue("NecklaceReturned") == true then
        -- Logic for after the necklace has been returned to the family.
        if Quest:GetStateValue("GhostToRest") == false then
            Me:SpeakAndWait("TEXT_QST_026_HUSBAND_TELL_MOTHER", 4)
        else
            Me:SpeakAndWait("TEXT_QST_026_HUSBAND_THANKS", 4)
        end
    elseif Quest:IsObjectInHeroPossession("OBJECT_GHOST_GRANNY_NECKLACE") then
        -- Logic for when the hero has the necklace and is trying to return it.
        HandleReturnNecklaceConversation()
    else
        -- Logic for the initial conversation to start the quest.
        HandleQuestStartConversation()
    end
    talkCounter = talkCounter + 1
end

---------------------------------------------------------------------
-- Logic for when the player has the necklace.
---------------------------------------------------------------------
function HandleReturnNecklaceConversation()
    Quest:GiveHeroYesNoQuestion(
        "TEXT_QST_026_RETURN_NECKLACE_QUESTION", 
        "TEXT_OBJECT_HERO_ANSWER_YES", 
        "TEXT_OBJECT_HERO_ANSWER_NO"
    )
    
    -- Wait for the player to answer
    local answer = -1
    while answer < 0 do
        if not Quest:Wait(0) then return end
        answer = Quest:MsgIsQuestionAnsweredYesOrNo()
    end

    if answer == 1 then -- Player chose "Yes"
        Quest:Log("GrannySon: Hero agreed to return the necklace.")
        Quest:SetStateValue("WifeNeededForCutscene", true)
        
        local wife = Quest:GetThing("GhostGrannyDaughterInLaw")
        local hero = Quest:GetHero()

        -- Play the outro cutscene
        Quest:PlayCutscene("CS_GHOSTGRANNY_OUTRO", {
            HERO = hero,
            WIFE = wife,
            FARMER = Me
        })

        -- Take the necklace and give rewards
        Quest:TakeObjectFromHero("OBJECT_GHOST_GRANNY_NECKLACE")
        Quest:GiveHeroGold(500) -- Example value
        Quest:GiveHeroMorality(20) -- Example value
        Quest:GiveHeroExperience(100) -- Example value
        Quest:SetStateValue("NecklaceReturned", true)
        Quest:ClearThingHasInformation(Me)
    else
        -- Player chose "No"
        Me:SpeakAndWait("TEXT_QST_026_HUSBAND_FIND_GIFT", 4)
    end
end

---------------------------------------------------------------------
-- Logic for the first conversation to begin the quest.
---------------------------------------------------------------------
function HandleQuestStartConversation()
    if Quest:GetStateValue("HeroKnowsAboutNecklace") then
        -- If the player has already started the quest but talks again
        Me:SpeakAndWait("TEXT_QST_026_HUSBAND_FIND_GIFT", 4)
    elseif talkCounter > 0 then
        -- If the player has talked before but didn't accept
        Me:SpeakAndWait("TEXT_QST_026_HUSBAND_OH_MOTHER", 4)
    else
        -- This is the very first conversation
        local wife = Quest:GetThing("GhostGrannyDaughterInLaw")
        local hero = Quest:GetHero()

        -- Play the intro cutscene
        Quest:PlayCutscene("CS_GHOSTGRANNY_INTROB", { -- Assuming INTROB is the default
            HERO = hero,
            WIFE = wife,
            FARMER = Me
        })

        -- Ask the player if they want to help
        Quest:GiveHeroYesNoQuestion(
            "TEXT_QST_026_HUSBAND_GAME_QUESTION", 
            "TEXT_OBJECT_HERO_ANSWER_YES", 
            "TEXT_OBJECT_HERO_ANSWER_NO"
        )
        local answer = -1
        while answer < 0 do
            if not Quest:Wait(0) then return end
            answer = Quest:MsgIsQuestionAnsweredYesOrNo()
        end

        if answer == 1 then -- Player chose "Yes"
            Quest:Log("GrannySon: Hero agreed to help. Starting quest.")
            Quest:GiveHeroQuestCardDirectly("OBJECT_QUEST_CARD_GHOST_GRANNY_NECKLACE", Quest:GetActiveQuestName(), false)
            Quest:SetStateValue("QuestCardGiven", true)
            Quest:SetQuestCardObjective(Quest:GetActiveQuestName(), "TEXT_QUEST_GHOST_GRANNY_NECKLACE_OBJECTIVE_01", "OrchardFarm", "")
            Me:SpeakAndWait("TEXT_QST_026_HUSBAND_THANKS_FOR_CHOOSING_TO_HELP", 4)
            -- THIS IS THE CRITICAL STATE CHANGE
            Quest:SetStateValue("HeroKnowsAboutNecklace", true)
        end
    end
end

---------------------------------------------------------------------
-- Handles the NPC fleeing if attacked.
---------------------------------------------------------------------
function HandleFleeing()
    Quest:Log("GrannySon: I've been attacked! Fleeing.")
    
    if Me:GetHealth() <= 1.0 then
        Me:SpeakAndWait("TEXT_QST_026_HUSBAND_RUNS_AWAY_01", 4)
        
        -- Fade out, remove the character, and fade back in
        Quest:FadeScreenOut(1.0, 1.0)
        Quest:Pause(2.0)
        Quest:RemoveThing(Me)
        Quest:FadeScreenIn()
    end
end