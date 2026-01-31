
Quest = nil

function Init(questObject)
    Quest = questObject
end

function Main(questObject)
    Quest = questObject
    Quest:AddEntityBinding("DemonDoorRich", "DemonDoorLUA/Entities/DemonDoorRich")
	Quest:Log("Entities bound")
    Quest:FinalizeEntityBindings()
	Quest:Log("Entities finalized")
 
end

function OnPersist(Quest, Context)
end

function OpenDoor(ParentQuest, faceEntity, doorScriptName)

  -- 1. Fade out the face entity
  ParentQuest:Log(" Fading out face entity...")
  --ParentQuest:EntityFadeOut(faceEntity, 3.0) -- Use ParentQuest API
  ParentQuest:Wait(3.0) -- Wait for the fade

  local actualDoor = ParentQuest:GetThingWithScriptName(doorScriptName)
    ParentQuest:OpenDoor(actualDoor)
    ParentQuest:SetThingPersistent(actualDoor, true)
    ParentQuest:ReleaseThing(actualDoor)
    actualDoor = nil
  
  --if faceEntity and not faceEntity:IsNull() then
  --    ParentQuest:Log(" Removing face entity.")
  --    ParentQuest:RemoveThing(faceEntity)
  --else
  --    ParentQuest:Log(" Face entity was already invalid or removed before final RemoveThing call.")
  --end

  ParentQuest:Log("Parent Quest: OpenDemonDoorGeometry finished.")
end