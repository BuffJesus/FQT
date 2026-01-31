-- quests.lua registration example for NewQuest

Quests = {
    NewQuest = {
        name = "NewQuest",
        file = "NewQuest/NewQuest",
        id = 50001,
        entity_scripts = {
            { name = "Karen", file = "NewQuest/Entities/Karen", id = 50002 },
        }
    },

    -- FSE Master (always required)
    FSE_Master = {
        name = "FSE_Master",
        file = "Master/FSE_Master",
        id = 50000,
        entity_scripts = {}
    },
}
