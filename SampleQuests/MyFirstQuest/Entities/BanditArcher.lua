

function Init(questObject, meObject)

	meObject:MakeBehavioral()
	meObject:AcquireControl()
    meObject:TakeExclusiveControl()
end

function Main(questObject, meObject)

while true do
        -- Check if the player is talking to this entity
        if meObject:IsTalkedToByHero() then
        
            questObject:Log("CHECKER: Hero is talking to me.")

            local hasSpokenToTalker = questObject:GetStateBool("talker_spoken_to")
            
            if hasSpokenToTalker then
                questObject:Log("CHECKER: 'talker_spoken_to' is true.")
                meObject:SpeakAndWait("Ah, I see you spoke to my friend! Good job.")
            else
                questObject:Log("CHECKER: 'talker_spoken_to' is false.")
                meObject:SpeakAndWait("You should go speak to the Talker over there first.")
            end
        end
        
        -- Yield this frame
        if not questObject:NewScriptFrame(meObject) then
			    meObject:ReleaseControl()
            break
        end
    end

end
