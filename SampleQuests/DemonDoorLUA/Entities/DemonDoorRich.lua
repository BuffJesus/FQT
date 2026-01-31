local isDoorOpen = false

function Init(questObject, meObject)

    Quest = questObject
	
	meObject:MakeBehavioral()
	meObject:TakeExclusiveControl()
	questObject:Log("Took exclusive control")
	
	
	
    Quest:SetThingPersistent(meObject, true) -- Make the face entity persistent
		questObject:Log("Set thing persistent")
    Quest:EntitySetAsDamageable(meObject, false) -- Make it invulnerable
		questObject:Log("Set thing persistent")
    Quest:EntitySetAsToAddToComboMultiplierWhenHit(meObject, false) -- Don't affect combat multi
		questObject:Log("Disabled combat multiplier")
    Quest:EntitySetAsKillable(meObject, false) -- Prevent it from being 'killed'
		questObject:Log("Set as unkillable")
end

function Main(questObject, meObject)

    while true do	
	
	if meObject:IsTalkedToByHero() then
	
	local heroGold = Quest:GetHeroGold()
	
	if heroGold < 1000 then
        meObject:SpeakAndWait("Go away brokie!", 4)
	elseif heroGold < 5000 then
		meObject:SpeakAndWait("I don't like the smell of poor in the morning.", 4)
	elseif heroGold < 10000 then
		meObject:SpeakAndWait("That is pocket change.", 4)
	elseif heroGold < 50000 then
		local mypos = meObject:GetPos()
		Quest:CreateEffectOnThing("LARGEFIRE", meObject, "head", true, false)
		meObject:SpeakAndWait("MOAR!!!", 4)
	elseif heroGold >= 50000 then
		meObject:SpeakAndWait("Ah now that's some serious money.", 4)
		Quest:EntitySetTargetable(meObject)
		isDoorOpen = true
		Quest:EntityFadeOut(meObject, 3.0)
		questObject:Pause(3.1)
		Quest:RemoveThing(meObject)
		Quest:CallParentFunction("OpenDoor", Me, "DD_Rich")
		end
	end
	
        if not questObject:Wait(0) then 
            return 
        end
    end
end
