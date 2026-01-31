
local heroME = nil
local targetMarker = nil

function Init(questObject, meObject)
    meObject:MakeBehavioral()
	meObject:AcquireControl()
end

function Main(questObject, meObject)

    questObject:Log("Entity Main: Running setup logic...")
    targetMarker = questObject:GetThingWithScriptName("MK_MOVETOHERE")
	heroME = questObject:GetHero()
    itsME = questObject:GetThingWithScriptName("NPCtoTalk")

    if targetMarker then
        questObject:Log("Entity Main: Found target. Starting initial movement.")
        meObject:MoveToAndPickUpGenericBox(targetMarker, 2) 
        currentState = "MOVING"
    else
        questObject:Log("!!! ERROR: Entity Main: Could not find 'MK_MOVETOHERE'.")
        currentState = "DONE" 
    end

    while true do 

        local isTalkedTo = meObject:IsTalkedToByHero()
        local isTaskRunning = meObject:IsPerformingScriptTask()

		if g_CurrentConvoID ~= nil then

		if not questObject:IsConversationActive(g_CurrentConvoID) then
        questObject:Log("Ambient conversation finished.")
        g_CurrentConvoID = nil
		end
	end

        if currentState == "MOVING" then
            if isTalkedTo then
				questObject:StartMovieSequence()
				meObject:Speak(heroME, "You seem dumb.")
				questObject:SetStateBool("talker_spoken_to", true)
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
				elseif not isTaskRunning then
                questObject:Log("State: MOVING -> DONE (Arrived at destination)")
				meObject:FollowThing(heroME, 2, true)
                --meObject:ReleaseControl() 
			
			if meObject:IsFollowActionRunning() == true then
			questObject:Log("Following")
                currentState = "DONE"
			end
        end

        elseif currentState == "PLAYING_ANIM" then
            if not isTaskRunning then
                questObject:Log("State: PLAYING_ANIM -> MOVING (Resuming movement)")
                currentState = "MOVING"
                if targetMarker then
                    meObject:MoveToThing(targetMarker, 1.0, 1)
                else
                    questObject:Log("!!! ERROR: Cannot resume movement, targetMarker is nil.")
                    currentState = "DONE"
                end
            end

        elseif currentState == "DONE" then
        end

        if not questObject:NewScriptFrame(meObject) then
            questObject:Log("...Main() loop terminating. Releasing control...")
            meObject:ReleaseControl()
            break 
        end
    end
end
