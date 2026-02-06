-- Add this entry inside the Quests table in quests.lua
    RegisterQuest = {
        name = "RegisterQuest",
        file = "RegisterQuest/RegisterQuest",
        id = 60000,

        entity_scripts = {
            { name = "NpcOne", file = "RegisterQuest/Entities/NpcOne", id = 60001 },
            { name = "NpcTwo", file = "RegisterQuest/Entities/NpcTwo", id = 60002 },
            { name = "RegisterChest", file = "RegisterQuest/Entities/RegisterChest", id = 60003 },
        }
    },