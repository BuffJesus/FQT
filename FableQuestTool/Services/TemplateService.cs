using System.Collections.Generic;
using System.Collections.ObjectModel;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Provides built-in quest templates for common quest types.
///
/// TemplateService contains a library of pre-configured quest templates that serve
/// as starting points for quest creation. Each template includes appropriate entities,
/// behavior nodes, connections, states, and rewards for its quest type.
///
/// Available templates:
/// - Simple Talk Quest: Basic dialogue with an NPC using conversation system
/// - Cinematic Dialogue: Dramatic dialogue with camera work and effects
/// - Kill Quest: Track enemy kills with state counter
/// - Boss Fight: Combat with cinematic intro, effects, and victory sequence
/// - Fetch Quest: Item collection and delivery
/// - Escort Quest: Protect an NPC traveling to a destination
/// - Delivery Quest: Multi-location delivery mission
/// - Investigation Quest: Question witnesses with branching dialogue choices
/// </summary>
/// <remarks>
/// Templates are designed to showcase FSE features and best practices:
/// - Proper use of conversation system vs SpeakAndWait
/// - Camera work and visual effects
/// - State management for tracking progress
/// - Branching dialogue with yes/no questions
/// - Event triggers and response handling
///
/// Users can modify templates after creation to customize quest details
/// while keeping the proven structure and behaviors.
/// </remarks>
public class TemplateService
{
    /// <summary>
    /// Returns all available quest templates.
    /// </summary>
    /// <returns>List of QuestTemplate objects representing available templates</returns>
    public List<QuestTemplate> GetAllTemplates()
    {
        return new List<QuestTemplate>
        {
            CreateTalkTemplate(),
            CreateCinematicDialogueTemplate(),
            CreateKillTemplate(),
            CreateBossFightTemplate(),
            CreateFetchTemplate(),
            CreateEscortTemplate(),
            CreateDeliveryTemplate(),
            CreateInvestigationTemplate()
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
            UseQuestEndScreen = true,
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
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        // Create node IDs for connections
        string talkNodeId = Guid.NewGuid().ToString();
        string cameraNodeId = Guid.NewGuid().ToString();
        string startConvoNodeId = Guid.NewGuid().ToString();
        string line1NodeId = Guid.NewGuid().ToString();
        string line2NodeId = Guid.NewGuid().ToString();
        string endConvoNodeId = Guid.NewGuid().ToString();
        string resetCameraNodeId = Guid.NewGuid().ToString();
        string completeNodeId = Guid.NewGuid().ToString();

        // Create nodes with proper Config values
        var nodes = new List<BehaviorNode>
        {
            new BehaviorNode
            {
                Id = talkNodeId,
                Type = "onHeroTalks",
                Category = "trigger",
                Label = "When Hero Talks",
                Icon = "💬",
                X = 100,
                Y = 150
            },
            new BehaviorNode
            {
                Id = cameraNodeId,
                Type = "cameraConversation",
                Category = "action",
                Label = "Conversation Camera",
                Icon = "🎬💬",
                X = 300,
                Y = 150,
                Config = new Dictionary<string, object> { { "cameraOp", "0" } }
            },
            new BehaviorNode
            {
                Id = startConvoNodeId,
                Type = "startConversation",
                Category = "action",
                Label = "Start Conversation",
                Icon = "🎭",
                X = 500,
                Y = 150,
                Config = new Dictionary<string, object>
                {
                    { "use2DSound", "true" },
                    { "playInCutscene", "false" }
                }
            },
            new BehaviorNode
            {
                Id = line1NodeId,
                Type = "addConversationLine",
                Category = "action",
                Label = "Add Line 1",
                Icon = "💭",
                X = 700,
                Y = 100,
                Config = new Dictionary<string, object>
                {
                    { "textKey", "Hello there, hero! I've been waiting for you." },
                    { "showSubtitle", "true" }
                }
            },
            new BehaviorNode
            {
                Id = line2NodeId,
                Type = "addConversationLine",
                Category = "action",
                Label = "Add Line 2",
                Icon = "💭",
                X = 700,
                Y = 200,
                Config = new Dictionary<string, object>
                {
                    { "textKey", "Thank you for coming. Your help means a lot to me." },
                    { "showSubtitle", "true" }
                }
            },
            new BehaviorNode
            {
                Id = endConvoNodeId,
                Type = "endConversation",
                Category = "action",
                Label = "End Conversation",
                Icon = "🔚",
                X = 900,
                Y = 150,
                Config = new Dictionary<string, object> { { "immediate", "false" } }
            },
            new BehaviorNode
            {
                Id = resetCameraNodeId,
                Type = "cameraResetToHero",
                Category = "action",
                Label = "Reset Camera",
                Icon = "🔄🎥",
                X = 1100,
                Y = 150,
                Config = new Dictionary<string, object> { { "duration", "1.0" } }
            },
            new BehaviorNode
            {
                Id = completeNodeId,
                Type = "completeQuest",
                Category = "action",
                Label = "Complete Quest",
                Icon = "✅",
                X = 1300,
                Y = 150,
                Config = new Dictionary<string, object> { { "showScreen", "true" } }
            }
        };

        npc.Nodes = nodes;

        // Add connections between nodes
        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkNodeId, FromPort = "Output", ToNodeId = cameraNodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = cameraNodeId, FromPort = "Output", ToNodeId = startConvoNodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = startConvoNodeId, FromPort = "Output", ToNodeId = line1NodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = line1NodeId, FromPort = "Output", ToNodeId = line2NodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = line2NodeId, FromPort = "Output", ToNodeId = endConvoNodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = endConvoNodeId, FromPort = "Output", ToNodeId = resetCameraNodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = resetCameraNodeId, FromPort = "Output", ToNodeId = completeNodeId, ToPort = "Input" }
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
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
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
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
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
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
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
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
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

    private QuestTemplate CreateCinematicDialogueTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyCinematicQuest",
            Id = 50005,
            DisplayName = "A Mysterious Stranger",
            Description = "A cinematic dialogue quest with camera work and effects",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Speak with the mysterious stranger",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true,
            IsStoryQuest = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 1000,
            Renown = 100,
            Experience = 200
        };

        var stranger = new QuestEntity
        {
            Id = "mysterious_stranger",
            ScriptName = "MysteriousStranger",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true
        };

        // Create cinematic dialogue flow using WORKING hybrid architecture
        // Entity script sets flag when hero talks, waits for camera, then uses SpeakAndWait
        // SpeakAndWait handles letterbox bars automatically - DON'T call StartMovieSequence manually!
        string triggerId = Guid.NewGuid().ToString();
        string setFlagId = Guid.NewGuid().ToString();
        string waitCameraId = Guid.NewGuid().ToString();
        string line1Id = Guid.NewGuid().ToString();
        string wait1Id = Guid.NewGuid().ToString();
        string line2Id = Guid.NewGuid().ToString();
        string wait2Id = Guid.NewGuid().ToString();
        string line3Id = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        var nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = triggerId, Type = "onHeroTalks", Category = "trigger", Label = "When Hero Talks", Icon = "💬", X = 50, Y = 200 },
            new BehaviorNode { Id = setFlagId, Type = "setState", Category = "action", Label = "Set Dialogue Flag", Icon = "💾", X = 200, Y = 200,
                Config = new Dictionary<string, object> { { "name", "DialogueTriggered" }, { "value", "true" } } },
            new BehaviorNode { Id = waitCameraId, Type = "wait", Category = "action", Label = "Wait for Camera", Icon = "⏱️", X = 350, Y = 200,
                Config = new Dictionary<string, object> { { "seconds", "1.0" } } },
            new BehaviorNode { Id = line1Id, Type = "showDialogue", Category = "action", Label = "Line 1", Icon = "💬", X = 500, Y = 200,
                Config = new Dictionary<string, object> { { "text", "Ah... you've finally arrived. I've been expecting you." } } },
            new BehaviorNode { Id = wait1Id, Type = "wait", Category = "action", Label = "Pause", Icon = "⏱️", X = 650, Y = 200,
                Config = new Dictionary<string, object> { { "seconds", "0.5" } } },
            new BehaviorNode { Id = line2Id, Type = "showDialogue", Category = "action", Label = "Line 2", Icon = "💬", X = 800, Y = 200,
                Config = new Dictionary<string, object> { { "text", "There are forces at work here... forces you cannot yet comprehend." } } },
            new BehaviorNode { Id = wait2Id, Type = "wait", Category = "action", Label = "Pause", Icon = "⏱️", X = 950, Y = 200,
                Config = new Dictionary<string, object> { { "seconds", "0.5" } } },
            new BehaviorNode { Id = line3Id, Type = "showDialogue", Category = "action", Label = "Line 3", Icon = "💬", X = 1100, Y = 200,
                Config = new Dictionary<string, object> { { "text", "But in time, hero, you will understand. In time..." } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete", Icon = "✅", X = 1250, Y = 200,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        stranger.Nodes = nodes;
        stranger.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = triggerId, FromPort = "Output", ToNodeId = setFlagId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setFlagId, FromPort = "Output", ToNodeId = waitCameraId, ToPort = "Input" },
            new NodeConnection { FromNodeId = waitCameraId, FromPort = "Output", ToNodeId = line1Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = line1Id, FromPort = "Output", ToNodeId = wait1Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = wait1Id, FromPort = "Output", ToNodeId = line2Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = line2Id, FromPort = "Output", ToNodeId = wait2Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = wait2Id, FromPort = "Output", ToNodeId = line3Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = line3Id, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(stranger);

        return new QuestTemplate
        {
            Name = "Cinematic Dialogue",
            Description = "A dramatic dialogue with letterbox, camera work, music, and screen effects. Perfect for story moments.",
            Category = "Cinematic",
            Difficulty = "Intermediate",
            Template = project
        };
    }

    private QuestTemplate CreateBossFightTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyBossQuest",
            Id = 50006,
            DisplayName = "Defeat the Champion",
            Description = "Face a powerful boss enemy with cinematic intro",
            Regions = new ObservableCollection<string> { "Arena" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Defeat the Arena Champion",
            ObjectiveRegion1 = "Arena",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 5000,
            Renown = 500,
            Experience = 1000
        };

        project.States.Add(new QuestState
        {
            Id = "bossDefeated",
            Name = "Boss Defeated",
            Type = "bool",
            Persist = true,
            DefaultValue = false
        });

        var boss = new QuestEntity
        {
            Id = "arena_champion",
            ScriptName = "ArenaChampion",
            DefName = "CREATURE_BANDIT_CHIEF",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true,
            ShowOnMinimap = true
        };

        // Intro sequence: hero approaches -> cinematic intro -> fight -> victory
        string proximityId = Guid.NewGuid().ToString();
        string letterboxId = Guid.NewGuid().ToString();
        string dangerMusicId = Guid.NewGuid().ToString();
        string cameraLookId = Guid.NewGuid().ToString();
        string blurId = Guid.NewGuid().ToString();
        string dialogueId = Guid.NewGuid().ToString();
        string blurOffId = Guid.NewGuid().ToString();
        string letterboxOffId = Guid.NewGuid().ToString();
        string cameraResetId = Guid.NewGuid().ToString();
        string hostileId = Guid.NewGuid().ToString();

        // Death trigger branch
        string deathTriggerId = Guid.NewGuid().ToString();
        string colorFilterId = Guid.NewGuid().ToString();
        string victoryMusicId = Guid.NewGuid().ToString();
        string victoryMsgId = Guid.NewGuid().ToString();
        string filterOffId = Guid.NewGuid().ToString();
        string stopMusicId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        var nodes = new List<BehaviorNode>
        {
            // Intro sequence
            new BehaviorNode { Id = proximityId, Type = "onProximity", Category = "trigger", Label = "Hero Nearby", Icon = "📍", X = 50, Y = 100,
                Config = new Dictionary<string, object> { { "distance", "10.0" } } },
            new BehaviorNode { Id = letterboxId, Type = "letterbox", Category = "action", Label = "Start Cinematic", Icon = "🎬", X = 200, Y = 100 },
            new BehaviorNode { Id = dangerMusicId, Type = "enableDangerMusic", Category = "action", Label = "Danger Music", Icon = "⚠️🎵", X = 350, Y = 100,
                Config = new Dictionary<string, object> { { "enabled", "true" } } },
            new BehaviorNode { Id = cameraLookId, Type = "cameraLookAtEntity", Category = "action", Label = "Camera Look", Icon = "👁️🎥", X = 500, Y = 100,
                Config = new Dictionary<string, object> { { "camX", "0" }, { "camY", "2.0" }, { "camZ", "5.0" }, { "duration", "1.5" } } },
            new BehaviorNode { Id = blurId, Type = "radialBlur", Category = "action", Label = "Radial Blur", Icon = "🌀", X = 650, Y = 100,
                Config = new Dictionary<string, object> { { "intensity", "0.3" }, { "duration", "0.5" } } },
            new BehaviorNode { Id = dialogueId, Type = "showDialogue", Category = "action", Label = "Boss Taunt", Icon = "💬", X = 800, Y = 100,
                Config = new Dictionary<string, object> { { "text", "So... another challenger approaches! Prepare to meet your end!" } } },
            new BehaviorNode { Id = blurOffId, Type = "radialBlurOff", Category = "action", Label = "Blur Off", Icon = "🔲", X = 950, Y = 100,
                Config = new Dictionary<string, object> { { "duration", "0.3" } } },
            new BehaviorNode { Id = letterboxOffId, Type = "letterboxOff", Category = "action", Label = "Letterbox Off", Icon = "📺", X = 1100, Y = 100 },
            new BehaviorNode { Id = cameraResetId, Type = "cameraResetToHero", Category = "action", Label = "Reset Camera", Icon = "🔄🎥", X = 1250, Y = 100,
                Config = new Dictionary<string, object> { { "duration", "1.0" } } },
            new BehaviorNode { Id = hostileId, Type = "makeHostile", Category = "action", Label = "Attack!", Icon = "😡", X = 1400, Y = 100 },

            // Death/Victory sequence
            new BehaviorNode { Id = deathTriggerId, Type = "onKilledByHero", Category = "trigger", Label = "When Killed", Icon = "⚰️", X = 50, Y = 300 },
            new BehaviorNode { Id = colorFilterId, Type = "colorFilter", Category = "action", Label = "Gold Filter", Icon = "🎨", X = 200, Y = 300,
                Config = new Dictionary<string, object> { { "r", "1.0" }, { "g", "0.9" }, { "b", "0.5" }, { "a", "0.3" }, { "duration", "0.5" } } },
            new BehaviorNode { Id = victoryMusicId, Type = "overrideMusic", Category = "action", Label = "Victory Music", Icon = "🎵", X = 350, Y = 300,
                Config = new Dictionary<string, object> { { "musicSetType", "3" }, { "isCutscene", "false" }, { "forcePlay", "true" } } },
            new BehaviorNode { Id = victoryMsgId, Type = "showTitleMessage", Category = "action", Label = "Victory!", Icon = "📢", X = 500, Y = 300,
                Config = new Dictionary<string, object> { { "text", "VICTORY!" }, { "duration", "3.0" } } },
            new BehaviorNode { Id = filterOffId, Type = "colorFilterOff", Category = "action", Label = "Filter Off", Icon = "🔲🎨", X = 650, Y = 300,
                Config = new Dictionary<string, object> { { "duration", "1.0" } } },
            new BehaviorNode { Id = stopMusicId, Type = "stopMusicOverride", Category = "action", Label = "Stop Music", Icon = "⏹️🎵", X = 800, Y = 300 },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete", Icon = "✅", X = 950, Y = 300,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        boss.Nodes = nodes;
        boss.Connections = new List<NodeConnection>
        {
            // Intro chain
            new NodeConnection { FromNodeId = proximityId, FromPort = "Output", ToNodeId = letterboxId, ToPort = "Input" },
            new NodeConnection { FromNodeId = letterboxId, FromPort = "Output", ToNodeId = dangerMusicId, ToPort = "Input" },
            new NodeConnection { FromNodeId = dangerMusicId, FromPort = "Output", ToNodeId = cameraLookId, ToPort = "Input" },
            new NodeConnection { FromNodeId = cameraLookId, FromPort = "Output", ToNodeId = blurId, ToPort = "Input" },
            new NodeConnection { FromNodeId = blurId, FromPort = "Output", ToNodeId = dialogueId, ToPort = "Input" },
            new NodeConnection { FromNodeId = dialogueId, FromPort = "Output", ToNodeId = blurOffId, ToPort = "Input" },
            new NodeConnection { FromNodeId = blurOffId, FromPort = "Output", ToNodeId = letterboxOffId, ToPort = "Input" },
            new NodeConnection { FromNodeId = letterboxOffId, FromPort = "Output", ToNodeId = cameraResetId, ToPort = "Input" },
            new NodeConnection { FromNodeId = cameraResetId, FromPort = "Output", ToNodeId = hostileId, ToPort = "Input" },

            // Victory chain
            new NodeConnection { FromNodeId = deathTriggerId, FromPort = "Output", ToNodeId = colorFilterId, ToPort = "Input" },
            new NodeConnection { FromNodeId = colorFilterId, FromPort = "Output", ToNodeId = victoryMusicId, ToPort = "Input" },
            new NodeConnection { FromNodeId = victoryMusicId, FromPort = "Output", ToNodeId = victoryMsgId, ToPort = "Input" },
            new NodeConnection { FromNodeId = victoryMsgId, FromPort = "Output", ToNodeId = filterOffId, ToPort = "Input" },
            new NodeConnection { FromNodeId = filterOffId, FromPort = "Output", ToNodeId = stopMusicId, ToPort = "Input" },
            new NodeConnection { FromNodeId = stopMusicId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(boss);

        return new QuestTemplate
        {
            Name = "Boss Fight",
            Description = "A dramatic boss encounter with cinematic intro, danger music, visual effects, and victory celebration.",
            Category = "Combat",
            Difficulty = "Advanced",
            Template = project
        };
    }

    private QuestTemplate CreateInvestigationTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyInvestigationQuest",
            Id = 50007,
            DisplayName = "The Missing Artifact",
            Description = "Investigate clues and question witnesses",
            Regions = new ObservableCollection<string> { "Bowerstone" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Investigate the missing artifact",
            ObjectiveRegion1 = "Bowerstone",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 1500,
            Renown = 150,
            Experience = 350
        };

        project.States.Add(new QuestState { Id = "cluesFound", Name = "Clues Found", Type = "int", Persist = true, DefaultValue = 0 });
        project.States.Add(new QuestState { Id = "witness1Talked", Name = "Talked to Witness 1", Type = "bool", Persist = true, DefaultValue = false });
        project.States.Add(new QuestState { Id = "witness2Talked", Name = "Talked to Witness 2", Type = "bool", Persist = true, DefaultValue = false });

        // First witness with yes/no question
        var witness1 = new QuestEntity
        {
            Id = "witness_1",
            ScriptName = "Witness1",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true
        };

        string w1TalkId = Guid.NewGuid().ToString();
        string w1CameraId = Guid.NewGuid().ToString();
        string w1ConvoId = Guid.NewGuid().ToString();
        string w1Line1Id = Guid.NewGuid().ToString();
        string w1QuestionId = Guid.NewGuid().ToString();
        string w1CheckId = Guid.NewGuid().ToString();
        string w1YesLineId = Guid.NewGuid().ToString();
        string w1NoLineId = Guid.NewGuid().ToString();
        string w1EndConvoId = Guid.NewGuid().ToString();
        string w1SetStateId = Guid.NewGuid().ToString();
        string w1CameraResetId = Guid.NewGuid().ToString();

        witness1.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = w1TalkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "💬", X = 50, Y = 150 },
            new BehaviorNode { Id = w1CameraId, Type = "cameraConversation", Category = "action", Label = "Camera", Icon = "🎬💬", X = 200, Y = 150,
                Config = new Dictionary<string, object> { { "cameraOp", "0" } } },
            new BehaviorNode { Id = w1ConvoId, Type = "startConversation", Category = "action", Label = "Start Convo", Icon = "🎭", X = 350, Y = 150,
                Config = new Dictionary<string, object> { { "use2DSound", "true" }, { "playInCutscene", "false" } } },
            new BehaviorNode { Id = w1Line1Id, Type = "addConversationLine", Category = "action", Label = "Intro", Icon = "💭", X = 500, Y = 150,
                Config = new Dictionary<string, object> { { "textKey", "Oh, you're investigating the missing artifact? I might have seen something..." }, { "showSubtitle", "true" } } },
            new BehaviorNode { Id = w1QuestionId, Type = "yesNoQuestion", Category = "action", Label = "Ask Question", Icon = "❓", X = 650, Y = 150,
                Config = new Dictionary<string, object> { { "question", "Will you tell me what you saw?" }, { "yes", "Yes, I'll help" }, { "no", "No, leave me alone" }, { "unsure", "I'm not sure..." } } },
            new BehaviorNode { Id = w1CheckId, Type = "checkYesNoAnswer", Category = "condition", Label = "Check Answer", Icon = "?", X = 800, Y = 150 },
            new BehaviorNode { Id = w1YesLineId, Type = "addConversationLine", Category = "action", Label = "Yes Response", Icon = "💭", X = 950, Y = 50,
                Config = new Dictionary<string, object> { { "textKey", "I saw a hooded figure near the museum last night. Very suspicious!" }, { "showSubtitle", "true" } } },
            new BehaviorNode { Id = w1NoLineId, Type = "addConversationLine", Category = "action", Label = "No Response", Icon = "💭", X = 950, Y = 250,
                Config = new Dictionary<string, object> { { "textKey", "Fine, fine! I saw someone suspicious near the museum. Happy now?" }, { "showSubtitle", "true" } } },
            new BehaviorNode { Id = w1EndConvoId, Type = "endConversation", Category = "action", Label = "End Convo", Icon = "🔚", X = 1100, Y = 150,
                Config = new Dictionary<string, object> { { "immediate", "false" } } },
            new BehaviorNode { Id = w1SetStateId, Type = "setState", Category = "action", Label = "Mark Talked", Icon = "💾", X = 1250, Y = 150,
                Config = new Dictionary<string, object> { { "name", "witness1Talked" }, { "value", "true" } } },
            new BehaviorNode { Id = w1CameraResetId, Type = "cameraResetToHero", Category = "action", Label = "Reset Camera", Icon = "🔄🎥", X = 1400, Y = 150,
                Config = new Dictionary<string, object> { { "duration", "1.0" } } }
        };

        witness1.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = w1TalkId, FromPort = "Output", ToNodeId = w1CameraId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1CameraId, FromPort = "Output", ToNodeId = w1ConvoId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1ConvoId, FromPort = "Output", ToNodeId = w1Line1Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1Line1Id, FromPort = "Output", ToNodeId = w1QuestionId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1QuestionId, FromPort = "Output", ToNodeId = w1CheckId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1CheckId, FromPort = "Yes", ToNodeId = w1YesLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1CheckId, FromPort = "No", ToNodeId = w1NoLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1CheckId, FromPort = "Unsure", ToNodeId = w1NoLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1YesLineId, FromPort = "Output", ToNodeId = w1EndConvoId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1NoLineId, FromPort = "Output", ToNodeId = w1EndConvoId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1EndConvoId, FromPort = "Output", ToNodeId = w1SetStateId, ToPort = "Input" },
            new NodeConnection { FromNodeId = w1SetStateId, FromPort = "Output", ToNodeId = w1CameraResetId, ToPort = "Input" }
        };

        project.Entities.Add(witness1);

        return new QuestTemplate
        {
            Name = "Investigation Quest",
            Description = "Question witnesses with branching dialogue choices. Features yes/no questions and state tracking.",
            Category = "Mystery",
            Difficulty = "Intermediate",
            Template = project
        };
    }
}
