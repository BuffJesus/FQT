-- FSE_Master.lua
-- Master control script for Fable Script Extender
-- This script activates all deployed quests

Quest = nil

function Init(questObject)
    Quest = questObject
    Quest:Log("FSE_Master: Init phase started.")
end

function Main(questObject)
    Quest = questObject
    Quest:Log("FSE_Master: Main() started. Activating quests...")

    -- Quests are activated here by the deployment tool
    -- FORMAT: quest:ActivateQuest("QuestName")

    Quest:Log("FSE_Master: All quests activated.")
end

function OnPersist(questObject, context)
    Quest = questObject
    -- Master script doesn't need to persist anything
end
