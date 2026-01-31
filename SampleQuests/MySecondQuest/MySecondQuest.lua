-- MySecondQuest.lua
-- This quest's only job is to load and manage the ChangingBulletinBoard entity.

function Init(quest)
    quest:Log("MySecondQuest: Init phase started.")
end

function Main(quest)
    quest:AddEntityBinding("ChangingBulletinBoard", "ChangingBulletinBoard")
    quest:FinalizeEntityBindings()
    
    quest:Log("MySecondQuest: Main() finished. The bulletin board is now active.")
end
