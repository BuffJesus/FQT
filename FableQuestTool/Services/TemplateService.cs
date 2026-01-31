using System.Collections.Generic;
using System.Collections.ObjectModel;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

public class TemplateService
{
    public List<QuestTemplate> GetAllTemplates()
    {
        return new List<QuestTemplate>
        {
            CreateTalkTemplate(),
            CreateKillTemplate(),
            CreateFetchTemplate(),
            CreateEscortTemplate(),
            CreateDeliveryTemplate()
        };
    }

    private QuestTemplate CreateTalkTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyTalkQuest",
            Id = 50000,
            DisplayName = "Talk to Someone",
            Description = "A simple dialogue quest where you talk to an NPC",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the villager",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true,
            IsStoryQuest = false,
            IsGoldQuest = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 500,
            Renown = 50,
            Experience = 100
        };

        var npc = new QuestEntity
        {
            Id = "npc_villager",
            ScriptName = "TalkNPC",
            DefName = "VILLAGER_MALE",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        // Create node IDs for connections
        string talkNodeId = Guid.NewGuid().ToString();
        string dialogueNodeId = Guid.NewGuid().ToString();
        string completeNodeId = Guid.NewGuid().ToString();

        // Create nodes with proper Config values
        var talkBehaviorNode = new BehaviorNode
        {
            Id = talkNodeId,
            Type = "onHeroTalks",
            Category = "trigger",
            Label = "When Hero Talks",
            Icon = "💬",
            X = 100,
            Y = 100
        };

        var dialogueBehaviorNode = new BehaviorNode
        {
            Id = dialogueNodeId,
            Type = "showDialogue",
            Category = "action",
            Label = "Show Dialogue",
            Icon = "💬",
            X = 400,
            Y = 100,
            Config = new Dictionary<string, object>
            {
                { "text", "Hello, hero! Thank you for speaking with me." }
            }
        };

        var completeBehaviorNode = new BehaviorNode
        {
            Id = completeNodeId,
            Type = "completeQuest",
            Category = "action",
            Label = "Complete Quest",
            Icon = "✅",
            X = 700,
            Y = 100,
            Config = new Dictionary<string, object>
            {
                { "showScreen", "true" }
            }
        };

        npc.Nodes = new List<BehaviorNode> { talkBehaviorNode, dialogueBehaviorNode, completeBehaviorNode };

        // Add connections between nodes
        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkNodeId, FromPort = "output", ToNodeId = dialogueNodeId, ToPort = "input" },
            new NodeConnection { FromNodeId = dialogueNodeId, FromPort = "output", ToNodeId = completeNodeId, ToPort = "input" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Simple Talk Quest",
            Description = "Talk to an NPC to complete the quest. Perfect for learning the basics.",
            Category = "Dialogue",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateKillTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyKillQuest",
            Id = 50001,
            DisplayName = "Defeat Enemies",
            Description = "Kill enemies to complete the quest",
            Regions = new ObservableCollection<string> { "BarrowFields" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Defeat 5 bandits",
            ObjectiveRegion1 = "BarrowFields",
            UseQuestStartScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 1000,
            Renown = 100,
            Experience = 250
        };

        project.States.Add(new QuestState
        {
            Id = "enemiesKilled",
            Name = "Enemies Killed",
            Type = "int",
            Persist = true,
            DefaultValue = 0
        });

        return new QuestTemplate
        {
            Name = "Kill Quest",
            Description = "Track enemy kills and complete when goal is reached.",
            Category = "Combat",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateFetchTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyFetchQuest",
            Id = 50002,
            DisplayName = "Retrieve Item",
            Description = "Bring an item to an NPC",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Bring the item to the merchant",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 750,
            Renown = 75,
            Experience = 150
        };

        return new QuestTemplate
        {
            Name = "Fetch Quest",
            Description = "Retrieve an item and deliver it to an NPC.",
            Category = "Collection",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateEscortTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyEscortQuest",
            Id = 50003,
            DisplayName = "Escort Mission",
            Description = "Escort an NPC safely to a destination",
            Regions = new ObservableCollection<string> { "Oakvale", "BarrowFields" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Escort the merchant safely",
            ObjectiveRegion1 = "Oakvale",
            ObjectiveRegion2 = "BarrowFields",
            UseQuestStartScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 1500,
            Renown = 150,
            Experience = 300
        };

        return new QuestTemplate
        {
            Name = "Escort Quest",
            Description = "Protect an NPC while traveling to a destination.",
            Category = "Escort",
            Difficulty = "Intermediate",
            Template = project
        };
    }

    private QuestTemplate CreateDeliveryTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyDeliveryQuest",
            Id = 50004,
            DisplayName = "Delivery Mission",
            Description = "Deliver a package to multiple locations",
            Regions = new ObservableCollection<string> { "Oakvale", "Bowerstone", "Knothole" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Make deliveries to three towns",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 2000,
            Renown = 200,
            Experience = 400
        };

        project.States.Add(new QuestState
        {
            Id = "deliveriesMade",
            Name = "Deliveries Made",
            Type = "int",
            Persist = true,
            DefaultValue = 0
        });

        return new QuestTemplate
        {
            Name = "Delivery Quest",
            Description = "Travel to multiple locations making deliveries.",
            Category = "Travel",
            Difficulty = "Intermediate",
            Template = project
        };
    }
}
