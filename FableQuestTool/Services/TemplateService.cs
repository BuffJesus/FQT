using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Provides built-in quest templates for common quest types.
///
/// TemplateService contains a library of pre-configured quest templates that serve
/// as starting points for quest creation. Each template includes appropriate entities,
/// behavior nodes, connections, states, and rewards for its quest type.
///
/// Built-in templates:
/// - Simple Talk Quest
/// - Cinematic Dialogue
/// - Kill Target Quest
/// - Boss Fight
/// - Fetch Quest
/// - Escort Quest
/// - Delivery Quest
/// - Investigation Quest
/// - Quest Board Starter
/// - Demon Door
///
/// External templates:
/// - Loaded from the Templates folder (next to the app or current working directory)
///   supporting .fqtproj, .fsequest, and .json files.
/// </summary>
/// <remarks>
/// Templates are designed to showcase FSE features and best practices:
/// - Use SpeakAndWait-based dialogue nodes for reliable entity scripts
/// - State management for tracking progress
/// - Branching dialogue with yes/no questions
/// - Event triggers and response handling
///
/// Users can modify templates after creation to customize quest details
/// while keeping the proven structure and behaviors.
/// </remarks>
public class TemplateService
{
    private const string TemplatesFolderName = "Templates";

    /// <summary>
    /// Returns all available quest templates.
    /// </summary>
    /// <returns>List of QuestTemplate objects representing available templates</returns>
    public List<QuestTemplate> GetAllTemplates()
    {
        var templates = new List<QuestTemplate>();
        templates.AddRange(CreateBuiltInTemplates());
        templates.AddRange(LoadExternalTemplates());

        return templates
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToList();
    }

    private List<QuestTemplate> CreateBuiltInTemplates()
    {
        var templates = new List<QuestTemplate>
        {
            CreateTalkTemplate(),
            CreateCinematicDialogueTemplate(),
            CreateKillTemplate(),
            CreateBossFightTemplate(),
            CreateFetchTemplate(),
            CreateEscortTemplate(),
            CreateDeliveryTemplate(),
            CreatePilgrimageTemplate(),
            CreateInvestigationTemplate(),
            CreateQuestBoardTemplate(),
            CreateDemonDoorTemplate(),
            CreateVariableShowcaseTemplate(),
            CreateVariableStringTemplate(),
            CreateVariableBooleanTemplate(),
            CreateVariableIntegerTemplate(),
            CreateVariableFloatTemplate(),
            CreateVariableObjectTemplate(),
            CreateVariableBranchTemplate(),
            CreateVariableExternalFlowTemplate()
        };

        foreach (QuestTemplate template in templates)
        {
            ApplyTemplateDefaults(template.Template);
        }

        return templates;
    }

    private IEnumerable<QuestTemplate> LoadExternalTemplates()
    {
        var templates = new List<QuestTemplate>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string folder in GetTemplateFolders())
        {
            if (!Directory.Exists(folder))
            {
                continue;
            }

            foreach (string file in EnumerateTemplateFiles(folder))
            {
                if (!seenPaths.Add(file))
                {
                    continue;
                }

                try
                {
                    var project = LoadQuestProject(file);
                    if (project == null)
                    {
                        continue;
                    }

                    ApplyTemplateDefaults(project);

                    string templateName = !string.IsNullOrWhiteSpace(project.DisplayName)
                        ? project.DisplayName
                        : (!string.IsNullOrWhiteSpace(project.Name) ? project.Name : Path.GetFileNameWithoutExtension(file));

                    string description = !string.IsNullOrWhiteSpace(project.Description)
                        ? project.Description
                        : "Custom template loaded from file.";

                    templates.Add(new QuestTemplate
                    {
                        Name = templateName,
                        Description = description,
                        Category = "Custom",
                        Difficulty = "Custom",
                        Template = project
                    });
                }
                catch
                {
                    // Ignore invalid templates to avoid breaking the template browser
                }
            }
        }

        return templates;
    }

    private static void ApplyTemplateDefaults(QuestProject project)
    {
        if (project == null)
        {
            return;
        }

        project.UseQuestStartScreen = true;
        project.UseQuestEndScreen = true;

        if (project.Regions == null)
        {
            project.Regions = new ObservableCollection<string>();
        }

        if (project.Regions.Count == 0)
        {
            string region = project.ObjectiveRegion1
                ?? project.ObjectiveRegion2
                ?? "Oakvale";
            project.Regions.Add(region);
        }

        if (string.IsNullOrWhiteSpace(project.ObjectiveRegion1) && project.Regions.Count > 0)
        {
            project.ObjectiveRegion1 = project.Regions[0];
        }

        foreach (QuestEntity entity in project.Entities)
        {
            if (string.IsNullOrWhiteSpace(entity.SpawnRegion))
            {
                entity.SpawnRegion = project.ObjectiveRegion1 ?? project.Regions.FirstOrDefault() ?? "Oakvale";
            }
            if (string.Equals(project.Name, "VarExternalFlowQuest", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(entity.ScriptName, "VarListener", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            entity.IsQuestTarget = true;
            entity.ShowOnMinimap = true;
        }
    }

    private static void InjectStartScreenNode(QuestProject project)
    {
        if (project == null || !project.UseQuestStartScreen)
        {
            return;
        }

        if (project.Entities.Count == 0)
        {
            return;
        }

        if (project.Entities.Any(entity =>
                entity.Nodes.Any(node => string.Equals(node.Type, "showStartScreen", StringComparison.OrdinalIgnoreCase))))
        {
            return;
        }

        QuestEntity? entityWithGraph = project.Entities.FirstOrDefault(entity =>
            entity.Nodes.Count > 0 && entity.Connections.Count > 0);

        if (entityWithGraph == null)
        {
            return;
        }

        NodeConnection? connection = entityWithGraph.Connections.FirstOrDefault(connectionCandidate =>
            string.Equals(connectionCandidate.FromPort, "Output", StringComparison.OrdinalIgnoreCase) &&
            entityWithGraph.Nodes.Any(node =>
                node.Id == connectionCandidate.FromNodeId &&
                string.Equals(node.Category, "trigger", StringComparison.OrdinalIgnoreCase)));

        connection ??= entityWithGraph.Connections.FirstOrDefault(connectionCandidate =>
            string.Equals(connectionCandidate.FromPort, "Output", StringComparison.OrdinalIgnoreCase));

        if (connection == null)
        {
            return;
        }

        BehaviorNode? fromNode = entityWithGraph.Nodes.FirstOrDefault(node => node.Id == connection.FromNodeId);
        BehaviorNode? toNode = entityWithGraph.Nodes.FirstOrDefault(node => node.Id == connection.ToNodeId);

        if (fromNode == null || toNode == null)
        {
            return;
        }

        string questCard = project.QuestCardObject ?? string.Empty;
        bool shouldGiveCard = project.GiveCardDirectly || !project.IsGuildQuest;

        string startScreenNodeId = Guid.NewGuid().ToString();
        var startScreenNode = new BehaviorNode
        {
            Id = startScreenNodeId,
            Type = "showStartScreen",
            Category = "action",
            Label = "Show Start Screen",
            Icon = "??",
            X = (fromNode.X + toNode.X) / 2.0,
            Y = (fromNode.Y + toNode.Y) / 2.0,
            Config = new Dictionary<string, object>
            {
                { "questCard", questCard },
                { "giveCard", shouldGiveCard ? "true" : "false" },
                { "showHeroGuide", "true" },
                { "isStory", project.IsStoryQuest ? "true" : "false" },
                { "isGold", project.IsGoldQuest ? "true" : "false" }
            }
        };

        entityWithGraph.Nodes.Add(startScreenNode);

        string originalToNodeId = connection.ToNodeId;
        string originalToPort = connection.ToPort;
        connection.ToNodeId = startScreenNodeId;
        connection.ToPort = "Input";

        entityWithGraph.Connections.Add(new NodeConnection
        {
            FromNodeId = startScreenNodeId,
            FromPort = "Output",
            ToNodeId = originalToNodeId,
            ToPort = string.IsNullOrWhiteSpace(originalToPort) ? "Input" : originalToPort
        });
    }

    private static IEnumerable<string> GetTemplateFolders()
    {
        var folders = new List<string>();

        string baseDir = AppContext.BaseDirectory;
        folders.Add(Path.Combine(baseDir, TemplatesFolderName));

        string currentDir = Directory.GetCurrentDirectory();
        string currentFolder = Path.Combine(currentDir, TemplatesFolderName);
        if (!string.Equals(currentFolder, folders[0], StringComparison.OrdinalIgnoreCase))
        {
            folders.Add(currentFolder);
        }

        return folders;
    }

    private static IEnumerable<string> EnumerateTemplateFiles(string folder)
    {
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".fqtproj",
            ".fsequest",
            ".json"
        };

        foreach (string file in Directory.EnumerateFiles(folder))
        {
            if (extensions.Contains(Path.GetExtension(file)))
            {
                yield return file;
            }
        }
    }

    private static QuestProject? LoadQuestProject(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<QuestProject>(json, options);
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

        string talkNodeId = Guid.NewGuid().ToString();
        string line1NodeId = Guid.NewGuid().ToString();
        string line2NodeId = Guid.NewGuid().ToString();
        string completeNodeId = Guid.NewGuid().ToString();

        var nodes = new List<BehaviorNode>
        {
            new BehaviorNode
            {
                Id = talkNodeId,
                Type = "onHeroTalks",
                Category = "trigger",
                Label = "When Hero Talks",
                Icon = "??",
                X = 100,
                Y = 150
            },
            new BehaviorNode
            {
                Id = line1NodeId,
                Type = "showDialogue",
                Category = "action",
                Label = "Line 1",
                Icon = "??",
                X = 300,
                Y = 150,
                Config = new Dictionary<string, object>
                {
                    { "text", "Hello there, hero! I've been waiting for you." }
                }
            },
            new BehaviorNode
            {
                Id = line2NodeId,
                Type = "showDialogue",
                Category = "action",
                Label = "Line 2",
                Icon = "??",
                X = 500,
                Y = 150,
                Config = new Dictionary<string, object>
                {
                    { "text", "Thank you for coming. Your help means a lot to me." }
                }
            },
            new BehaviorNode
            {
                Id = completeNodeId,
                Type = "completeQuest",
                Category = "action",
                Label = "Complete Quest",
                Icon = "?",
                X = 700,
                Y = 150,
                Config = new Dictionary<string, object> { { "showScreen", "true" } }
            }
        };

        npc.Nodes = nodes;

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkNodeId, FromPort = "Output", ToNodeId = line1NodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = line1NodeId, FromPort = "Output", ToNodeId = line2NodeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = line2NodeId, FromPort = "Output", ToNodeId = completeNodeId, ToPort = "Input" }
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
            DisplayName = "Defeat a Target",
            Description = "Defeat a specific enemy to complete the quest",
            Regions = new ObservableCollection<string> { "BarrowFields" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Defeat the bandit leader",
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
            Id = "targetKilled",
            Name = "Target Killed",
            Type = "bool",
            Persist = true,
            DefaultValue = false
        });

        var target = new QuestEntity
        {
            Id = "target_bandit",
            ScriptName = "TargetBandit",
            DefName = "CREATURE_BANDIT",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true,
            ShowOnMinimap = true
        };

        string killedId = Guid.NewGuid().ToString();
        string setStateId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        target.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = killedId, Type = "onKilledByHero", Category = "trigger", Label = "When Killed", Icon = "??", X = 100, Y = 150 },
            new BehaviorNode { Id = setStateId, Type = "setStateBool", Category = "action", Label = "Mark Target Killed", Icon = "??", X = 300, Y = 150,
                Config = new Dictionary<string, object> { { "name", "targetKilled" }, { "value", "true" } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "?", X = 500, Y = 150,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        target.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = killedId, FromPort = "Output", ToNodeId = setStateId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setStateId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(target);

        return new QuestTemplate
        {
            Name = "Kill Target Quest",
            Description = "Defeat a specific target and complete the quest.",
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

        var npc = new QuestEntity
        {
            Id = "merchant",
            ScriptName = "Merchant",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true
        };

        string talkId = Guid.NewGuid().ToString();
        string takeItemId = Guid.NewGuid().ToString();
        string thanksId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onItemPresented", Category = "trigger", Label = "When Item Given", Icon = "??", X = 100, Y = 150,
                Config = new Dictionary<string, object> { { "item", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = takeItemId, Type = "takeItem", Category = "action", Label = "Take Item", Icon = "??", X = 300, Y = 150,
                Config = new Dictionary<string, object> { { "item", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = thanksId, Type = "showDialogue", Category = "action", Label = "Thank You", Icon = "??", X = 500, Y = 150,
                Config = new Dictionary<string, object> { { "text", "Ah, just what I needed. Thank you!" } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "?", X = 700, Y = 150,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = takeItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = takeItemId, FromPort = "Output", ToNodeId = thanksId, ToPort = "Input" },
            new NodeConnection { FromNodeId = thanksId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

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

        project.States.Add(new QuestState
        {
            Id = "escortStarted",
            Name = "Escort Started",
            Type = "bool",
            Persist = true,
            DefaultValue = false
        });

        var npc = new QuestEntity
        {
            Id = "escort_npc",
            ScriptName = "EscortNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true,
            ShowOnMinimap = true
        };

        string talkId = Guid.NewGuid().ToString();
        string startId = Guid.NewGuid().ToString();
        string followId = Guid.NewGuid().ToString();
        string waitId = Guid.NewGuid().ToString();
        string stopId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 100, Y = 150 },
            new BehaviorNode { Id = startId, Type = "setStateBool", Category = "action", Label = "Start Escort", Icon = "??", X = 300, Y = 150,
                Config = new Dictionary<string, object> { { "name", "escortStarted" }, { "value", "true" } } },
            new BehaviorNode { Id = followId, Type = "followHero", Category = "action", Label = "Follow Hero", Icon = "??", X = 500, Y = 150,
                Config = new Dictionary<string, object> { { "distance", "3.0" } } },
            new BehaviorNode { Id = waitId, Type = "wait", Category = "action", Label = "Walk Together", Icon = "??", X = 700, Y = 150,
                Config = new Dictionary<string, object> { { "seconds", "5.0" } } },
            new BehaviorNode { Id = stopId, Type = "stopFollowing", Category = "action", Label = "Stop Following", Icon = "??", X = 900, Y = 150,
                Config = new Dictionary<string, object> { { "target", "Hero" } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "?", X = 1100, Y = 150,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = startId, ToPort = "Input" },
            new NodeConnection { FromNodeId = startId, FromPort = "Output", ToNodeId = followId, ToPort = "Input" },
            new NodeConnection { FromNodeId = followId, FromPort = "Output", ToNodeId = waitId, ToPort = "Input" },
            new NodeConnection { FromNodeId = waitId, FromPort = "Output", ToNodeId = stopId, ToPort = "Input" },
            new NodeConnection { FromNodeId = stopId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

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

        project.States.Add(new QuestState { Id = "deliveredOakvale", Name = "Delivered Oakvale", Type = "bool", Persist = true, DefaultValue = false });
        project.States.Add(new QuestState { Id = "deliveredBowerstone", Name = "Delivered Bowerstone", Type = "bool", Persist = true, DefaultValue = false });
        project.States.Add(new QuestState { Id = "deliveredKnothole", Name = "Delivered Knothole", Type = "bool", Persist = true, DefaultValue = false });

        var manager = new QuestEntity
        {
            Id = "delivery_manager",
            ScriptName = "DeliveryManager",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        string managerTalkId = Guid.NewGuid().ToString();
        string checkOakvaleId = Guid.NewGuid().ToString();
        string checkBowerstoneId = Guid.NewGuid().ToString();
        string checkKnotholeId = Guid.NewGuid().ToString();
        string managerFailId = Guid.NewGuid().ToString();
        string managerCompleteId = Guid.NewGuid().ToString();

        manager.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = managerTalkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 100, Y = 150 },
            new BehaviorNode { Id = checkOakvaleId, Type = "checkStateBool", Category = "condition", Label = "Oakvale Delivered?", Icon = "?", X = 300, Y = 150,
                Config = new Dictionary<string, object> { { "name", "deliveredOakvale" }, { "value", "true" } } },
            new BehaviorNode { Id = checkBowerstoneId, Type = "checkStateBool", Category = "condition", Label = "Bowerstone Delivered?", Icon = "?", X = 500, Y = 150,
                Config = new Dictionary<string, object> { { "name", "deliveredBowerstone" }, { "value", "true" } } },
            new BehaviorNode { Id = checkKnotholeId, Type = "checkStateBool", Category = "condition", Label = "Knothole Delivered?", Icon = "?", X = 700, Y = 150,
                Config = new Dictionary<string, object> { { "name", "deliveredKnothole" }, { "value", "true" } } },
            new BehaviorNode { Id = managerFailId, Type = "showDialogue", Category = "action", Label = "Not Done", Icon = "??", X = 500, Y = 300,
                Config = new Dictionary<string, object> { { "text", "You still have deliveries to make." } } },
            new BehaviorNode { Id = managerCompleteId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "?", X = 900, Y = 150,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        manager.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = managerTalkId, FromPort = "Output", ToNodeId = checkOakvaleId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkOakvaleId, FromPort = "True", ToNodeId = checkBowerstoneId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkOakvaleId, FromPort = "False", ToNodeId = managerFailId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkBowerstoneId, FromPort = "True", ToNodeId = checkKnotholeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkBowerstoneId, FromPort = "False", ToNodeId = managerFailId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkKnotholeId, FromPort = "True", ToNodeId = managerCompleteId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkKnotholeId, FromPort = "False", ToNodeId = managerFailId, ToPort = "Input" }
        };

        project.Entities.Add(manager);

        project.Entities.Add(CreateDeliveryNpc("delivery_oakvale", "DeliveryOakvale", "deliveredOakvale", 100, 350));
        project.Entities.Add(CreateDeliveryNpc("delivery_bowerstone", "DeliveryBowerstone", "deliveredBowerstone", 100, 500));
        project.Entities.Add(CreateDeliveryNpc("delivery_knothole", "DeliveryKnothole", "deliveredKnothole", 100, 650));

        return new QuestTemplate
        {
            Name = "Delivery Quest",
            Description = "Travel to multiple locations making deliveries.",
            Category = "Travel",
            Difficulty = "Intermediate",
            Template = project
        };
    }

    private QuestEntity CreateDeliveryNpc(string id, string scriptName, string stateName, double x, double y)
    {
        var npc = new QuestEntity
        {
            Id = id,
            ScriptName = scriptName,
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        string talkId = Guid.NewGuid().ToString();
        string checkItemId = Guid.NewGuid().ToString();
        string takeItemId = Guid.NewGuid().ToString();
        string setStateId = Guid.NewGuid().ToString();
        string thanksId = Guid.NewGuid().ToString();
        string noItemId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = x, Y = y },
            new BehaviorNode { Id = checkItemId, Type = "checkHasItem", Category = "condition", Label = "Has Package?", Icon = "???", X = x + 200, Y = y,
                Config = new Dictionary<string, object> { { "item", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = takeItemId, Type = "takeItem", Category = "action", Label = "Take Package", Icon = "??", X = x + 400, Y = y - 50,
                Config = new Dictionary<string, object> { { "item", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = setStateId, Type = "setStateBool", Category = "action", Label = "Mark Delivered", Icon = "??", X = x + 600, Y = y - 50,
                Config = new Dictionary<string, object> { { "name", stateName }, { "value", "true" } } },
            new BehaviorNode { Id = thanksId, Type = "showDialogue", Category = "action", Label = "Thanks", Icon = "??", X = x + 800, Y = y - 50,
                Config = new Dictionary<string, object> { { "text", "Thanks for the delivery!" } } },
            new BehaviorNode { Id = noItemId, Type = "showDialogue", Category = "action", Label = "Missing Package", Icon = "??", X = x + 400, Y = y + 100,
                Config = new Dictionary<string, object> { { "text", "I don't have my package yet." } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = checkItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkItemId, FromPort = "True", ToNodeId = takeItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkItemId, FromPort = "False", ToNodeId = noItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = takeItemId, FromPort = "Output", ToNodeId = setStateId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setStateId, FromPort = "Output", ToNodeId = thanksId, ToPort = "Input" }
        };

        return npc;
    }

    private QuestTemplate CreatePilgrimageTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyPilgrimageQuest",
            Id = 50018,
            DisplayName = "Pilgrimage to the Guild",
            Description = "Travel from Oakvale through two waypoints and report to the Heroes Guild.",
            Regions = new ObservableCollection<string> { "Oakvale", "BarrowFields", "LookoutPoint", "HeroesGuild" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Visit Barrow Fields and Lookout Point, then report to the Heroes Guild.",
            ObjectiveRegion1 = "Oakvale",
            ObjectiveRegion2 = "HeroesGuild",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 1800,
            Renown = 180,
            Experience = 320
        };

        project.States.Add(new QuestState { Id = "visitedBarrowFields", Name = "Visited Barrow Fields", Type = "bool", Persist = true, DefaultValue = false });
        project.States.Add(new QuestState { Id = "visitedLookoutPoint", Name = "Visited Lookout Point", Type = "bool", Persist = true, DefaultValue = false });

        project.Entities.Add(CreatePilgrimageWaypointNpc(
            id: "pilgrimage_barrow",
            scriptName: "PilgrimageBarrow",
            stateName: "visitedBarrowFields",
            lineText: "The first waypoint is marked. Continue to Lookout Point.",
            spawnRegion: "BarrowFields",
            x: 80,
            y: 180));

        project.Entities.Add(CreatePilgrimageWaypointNpc(
            id: "pilgrimage_lookout",
            scriptName: "PilgrimageLookout",
            stateName: "visitedLookoutPoint",
            lineText: "Second waypoint complete. Return to the Heroes Guild.",
            spawnRegion: "LookoutPoint",
            x: 80,
            y: 360));

        var guildMaster = new QuestEntity
        {
            Id = "pilgrimage_guild_master",
            ScriptName = "PilgrimageGuildMaster",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            SpawnRegion = "HeroesGuild"
        };

        string talkId = Guid.NewGuid().ToString();
        string checkBarrowId = Guid.NewGuid().ToString();
        string checkLookoutId = Guid.NewGuid().ToString();
        string readyLineId = Guid.NewGuid().ToString();
        string notReadyLineId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        guildMaster.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 100, Y = 560 },
            new BehaviorNode { Id = checkBarrowId, Type = "checkStateBool", Category = "condition", Label = "Visited Barrow?", Icon = "?", X = 300, Y = 560,
                Config = new Dictionary<string, object> { { "name", "visitedBarrowFields" }, { "value", "true" } } },
            new BehaviorNode { Id = checkLookoutId, Type = "checkStateBool", Category = "condition", Label = "Visited Lookout?", Icon = "?", X = 500, Y = 560,
                Config = new Dictionary<string, object> { { "name", "visitedLookoutPoint" }, { "value", "true" } } },
            new BehaviorNode { Id = readyLineId, Type = "showDialogue", Category = "action", Label = "Ready", Icon = "??", X = 700, Y = 500,
                Config = new Dictionary<string, object> { { "text", "Well done. Your pilgrimage is complete." } } },
            new BehaviorNode { Id = notReadyLineId, Type = "showDialogue", Category = "action", Label = "Not Ready", Icon = "??", X = 500, Y = 700,
                Config = new Dictionary<string, object> { { "text", "You still have waypoints to visit before the Guild can certify you." } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "?", X = 900, Y = 500,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        guildMaster.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = checkBarrowId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkBarrowId, FromPort = "True", ToNodeId = checkLookoutId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkBarrowId, FromPort = "False", ToNodeId = notReadyLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkLookoutId, FromPort = "True", ToNodeId = readyLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkLookoutId, FromPort = "False", ToNodeId = notReadyLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = readyLineId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(guildMaster);

        return new QuestTemplate
        {
            Name = "Pilgrimage Route",
            Description = "A multi-location route that starts in Oakvale and ends at the Heroes Guild.",
            Category = "Travel",
            Difficulty = "Intermediate",
            Template = project
        };
    }

    private QuestEntity CreatePilgrimageWaypointNpc(
        string id,
        string scriptName,
        string stateName,
        string lineText,
        string spawnRegion,
        double x,
        double y)
    {
        var npc = new QuestEntity
        {
            Id = id,
            ScriptName = scriptName,
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            SpawnRegion = spawnRegion
        };

        string talkId = Guid.NewGuid().ToString();
        string markVisitedId = Guid.NewGuid().ToString();
        string lineId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = x, Y = y },
            new BehaviorNode { Id = markVisitedId, Type = "setStateBool", Category = "action", Label = "Mark Visited", Icon = "??", X = x + 200, Y = y,
                Config = new Dictionary<string, object> { { "name", stateName }, { "value", "true" } } },
            new BehaviorNode { Id = lineId, Type = "showDialogue", Category = "action", Label = "Waypoint Line", Icon = "??", X = x + 400, Y = y,
                Config = new Dictionary<string, object> { { "text", lineText } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = markVisitedId, ToPort = "Input" },
            new NodeConnection { FromNodeId = markVisitedId, FromPort = "Output", ToNodeId = lineId, ToPort = "Input" }
        };

        return npc;
    }

    private QuestTemplate CreateCinematicDialogueTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyCinematicQuest",
            Id = 50005,
            DisplayName = "A Mysterious Stranger",
            Description = "A cinematic dialogue quest with timed beats",
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

        string triggerId = Guid.NewGuid().ToString();
        string titleId = Guid.NewGuid().ToString();
        string line1Id = Guid.NewGuid().ToString();
        string wait1Id = Guid.NewGuid().ToString();
        string line2Id = Guid.NewGuid().ToString();
        string wait2Id = Guid.NewGuid().ToString();
        string line3Id = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        stranger.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = triggerId, Type = "onHeroTalks", Category = "trigger", Label = "When Hero Talks", Icon = "??", X = 50, Y = 200 },
            new BehaviorNode { Id = titleId, Type = "showTitleMessage", Category = "action", Label = "Title", Icon = "??", X = 200, Y = 200,
                Config = new Dictionary<string, object> { { "text", "A Mysterious Stranger" }, { "duration", "3.0" } } },
            new BehaviorNode { Id = line1Id, Type = "showDialogue", Category = "action", Label = "Line 1", Icon = "??", X = 350, Y = 200,
                Config = new Dictionary<string, object> { { "text", "Ah... you've finally arrived. I've been expecting you." } } },
            new BehaviorNode { Id = wait1Id, Type = "wait", Category = "action", Label = "Pause", Icon = "??", X = 500, Y = 200,
                Config = new Dictionary<string, object> { { "seconds", "0.5" } } },
            new BehaviorNode { Id = line2Id, Type = "showDialogue", Category = "action", Label = "Line 2", Icon = "??", X = 650, Y = 200,
                Config = new Dictionary<string, object> { { "text", "There are forces at work here... forces you cannot yet comprehend." } } },
            new BehaviorNode { Id = wait2Id, Type = "wait", Category = "action", Label = "Pause", Icon = "??", X = 800, Y = 200,
                Config = new Dictionary<string, object> { { "seconds", "0.5" } } },
            new BehaviorNode { Id = line3Id, Type = "showDialogue", Category = "action", Label = "Line 3", Icon = "??", X = 950, Y = 200,
                Config = new Dictionary<string, object> { { "text", "But in time, hero, you will understand. In time..." } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete", Icon = "?", X = 1100, Y = 200,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        stranger.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = triggerId, FromPort = "Output", ToNodeId = titleId, ToPort = "Input" },
            new NodeConnection { FromNodeId = titleId, FromPort = "Output", ToNodeId = line1Id, ToPort = "Input" },
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
            Description = "A dramatic dialogue with title cards and timed beats.",
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
            Description = "Face a powerful boss enemy with a simple intro",
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

        string proximityId = Guid.NewGuid().ToString();
        string introId = Guid.NewGuid().ToString();
        string hostileId = Guid.NewGuid().ToString();
        string deathTriggerId = Guid.NewGuid().ToString();
        string setStateId = Guid.NewGuid().ToString();
        string victoryMsgId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        boss.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = proximityId, Type = "onProximity", Category = "trigger", Label = "Hero Nearby", Icon = "??", X = 50, Y = 100,
                Config = new Dictionary<string, object> { { "distance", "10.0" } } },
            new BehaviorNode { Id = introId, Type = "showTitleMessage", Category = "action", Label = "Boss Intro", Icon = "??", X = 250, Y = 100,
                Config = new Dictionary<string, object> { { "text", "The Champion Approaches" }, { "duration", "3.0" } } },
            new BehaviorNode { Id = hostileId, Type = "makeHostile", Category = "action", Label = "Attack", Icon = "??", X = 450, Y = 100 },

            new BehaviorNode { Id = deathTriggerId, Type = "onKilledByHero", Category = "trigger", Label = "When Killed", Icon = "??", X = 50, Y = 300 },
            new BehaviorNode { Id = setStateId, Type = "setStateBool", Category = "action", Label = "Mark Defeated", Icon = "??", X = 250, Y = 300,
                Config = new Dictionary<string, object> { { "name", "bossDefeated" }, { "value", "true" } } },
            new BehaviorNode { Id = victoryMsgId, Type = "showTitleMessage", Category = "action", Label = "Victory", Icon = "??", X = 450, Y = 300,
                Config = new Dictionary<string, object> { { "text", "VICTORY" }, { "duration", "3.0" } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete", Icon = "?", X = 650, Y = 300,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        boss.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = proximityId, FromPort = "Output", ToNodeId = introId, ToPort = "Input" },
            new NodeConnection { FromNodeId = introId, FromPort = "Output", ToNodeId = hostileId, ToPort = "Input" },
            new NodeConnection { FromNodeId = deathTriggerId, FromPort = "Output", ToNodeId = setStateId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setStateId, FromPort = "Output", ToNodeId = victoryMsgId, ToPort = "Input" },
            new NodeConnection { FromNodeId = victoryMsgId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(boss);

        return new QuestTemplate
        {
            Name = "Boss Fight",
            Description = "A boss encounter with a simple intro and victory sequence.",
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

        var witness1 = BuildWitness("witness_1", "Witness1", "witness1Talked", 50, 150);
        var witness2 = BuildWitness("witness_2", "Witness2", "witness2Talked", 50, 350);

        project.Entities.Add(witness1);
        project.Entities.Add(witness2);

        var investigator = new QuestEntity
        {
            Id = "investigator",
            ScriptName = "Investigator",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        string invTalkId = Guid.NewGuid().ToString();
        string checkW1Id = Guid.NewGuid().ToString();
        string checkW2Id = Guid.NewGuid().ToString();
        string invFailId = Guid.NewGuid().ToString();
        string invCompleteId = Guid.NewGuid().ToString();

        investigator.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = invTalkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 50, Y = 550 },
            new BehaviorNode { Id = checkW1Id, Type = "checkStateBool", Category = "condition", Label = "Witness 1?", Icon = "?", X = 250, Y = 550,
                Config = new Dictionary<string, object> { { "name", "witness1Talked" }, { "value", "true" } } },
            new BehaviorNode { Id = checkW2Id, Type = "checkStateBool", Category = "condition", Label = "Witness 2?", Icon = "?", X = 450, Y = 550,
                Config = new Dictionary<string, object> { { "name", "witness2Talked" }, { "value", "true" } } },
            new BehaviorNode { Id = invFailId, Type = "showDialogue", Category = "action", Label = "Need More", Icon = "??", X = 450, Y = 700,
                Config = new Dictionary<string, object> { { "text", "Keep looking. We need both witness statements." } } },
            new BehaviorNode { Id = invCompleteId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "?", X = 650, Y = 550,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        investigator.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = invTalkId, FromPort = "Output", ToNodeId = checkW1Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkW1Id, FromPort = "True", ToNodeId = checkW2Id, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkW1Id, FromPort = "False", ToNodeId = invFailId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkW2Id, FromPort = "True", ToNodeId = invCompleteId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkW2Id, FromPort = "False", ToNodeId = invFailId, ToPort = "Input" }
        };

        project.Entities.Add(investigator);

        return new QuestTemplate
        {
            Name = "Investigation Quest",
            Description = "Question witnesses and report back to complete the quest.",
            Category = "Mystery",
            Difficulty = "Intermediate",
            Template = project
        };
    }

    private QuestEntity BuildWitness(string id, string scriptName, string stateName, double x, double y)
    {
        var witness = new QuestEntity
        {
            Id = id,
            ScriptName = scriptName,
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true
        };

        string talkId = Guid.NewGuid().ToString();
        string questionId = Guid.NewGuid().ToString();
        string checkId = Guid.NewGuid().ToString();
        string yesLineId = Guid.NewGuid().ToString();
        string noLineId = Guid.NewGuid().ToString();
        string setStateId = Guid.NewGuid().ToString();

        witness.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = x, Y = y },
            new BehaviorNode { Id = questionId, Type = "yesNoQuestion", Category = "action", Label = "Ask Question", Icon = "?", X = x + 200, Y = y,
                Config = new Dictionary<string, object> { { "question", "Will you tell me what you saw?" }, { "yes", "Yes, I'll help" }, { "no", "No, leave me alone" }, { "unsure", "I'm not sure..." } } },
            new BehaviorNode { Id = checkId, Type = "checkYesNoAnswer", Category = "condition", Label = "Check Answer", Icon = "?", X = x + 400, Y = y },
            new BehaviorNode { Id = yesLineId, Type = "showDialogue", Category = "action", Label = "Yes Response", Icon = "??", X = x + 600, Y = y - 100,
                Config = new Dictionary<string, object> { { "text", "I saw a hooded figure near the museum last night. Very suspicious!" } } },
            new BehaviorNode { Id = noLineId, Type = "showDialogue", Category = "action", Label = "No Response", Icon = "??", X = x + 600, Y = y + 100,
                Config = new Dictionary<string, object> { { "text", "Fine, fine! I saw someone suspicious near the museum. Happy now?" } } },
            new BehaviorNode { Id = setStateId, Type = "setStateBool", Category = "action", Label = "Mark Talked", Icon = "??", X = x + 800, Y = y,
                Config = new Dictionary<string, object> { { "name", stateName }, { "value", "true" } } }
        };

        witness.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = questionId, ToPort = "Input" },
            new NodeConnection { FromNodeId = questionId, FromPort = "Output", ToNodeId = checkId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkId, FromPort = "Yes", ToNodeId = yesLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkId, FromPort = "No", ToNodeId = noLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkId, FromPort = "Unsure", ToNodeId = noLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = yesLineId, FromPort = "Output", ToNodeId = setStateId, ToPort = "Input" },
            new NodeConnection { FromNodeId = noLineId, FromPort = "Output", ToNodeId = setStateId, ToPort = "Input" }
        };

        return witness;
    }

    private QuestTemplate CreateQuestBoardTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyBoardQuest",
            Id = 50008,
            DisplayName = "Quest Board Starter",
            Description = "Accept a quest, defeat a target, and report back",
            Regions = new ObservableCollection<string> { "Bowerstone" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Defeat the thief",
            ObjectiveRegion1 = "Bowerstone",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 1200,
            Renown = 100,
            Experience = 200
        };

        project.States.Add(new QuestState
        {
            Id = "questAccepted",
            Name = "Quest Accepted",
            Type = "bool",
            Persist = true,
            DefaultValue = false
        });

        var questGiver = new QuestEntity
        {
            Id = "quest_giver",
            ScriptName = "QuestGiver",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        string talkId = Guid.NewGuid().ToString();
        string questionId = Guid.NewGuid().ToString();
        string checkId = Guid.NewGuid().ToString();
        string acceptId = Guid.NewGuid().ToString();
        string acceptLineId = Guid.NewGuid().ToString();
        string declineLineId = Guid.NewGuid().ToString();

        questGiver.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 50, Y = 150 },
            new BehaviorNode { Id = questionId, Type = "yesNoQuestion", Category = "action", Label = "Offer Quest", Icon = "?", X = 250, Y = 150,
                Config = new Dictionary<string, object> { { "question", "A thief is loose in town. Will you stop them?" }, { "yes", "Yes" }, { "no", "No" }, { "unsure", "Not sure" } } },
            new BehaviorNode { Id = checkId, Type = "checkYesNoAnswer", Category = "condition", Label = "Check Answer", Icon = "?", X = 450, Y = 150 },
            new BehaviorNode { Id = acceptId, Type = "setStateBool", Category = "action", Label = "Accept Quest", Icon = "??", X = 650, Y = 100,
                Config = new Dictionary<string, object> { { "name", "questAccepted" }, { "value", "true" } } },
            new BehaviorNode { Id = acceptLineId, Type = "showDialogue", Category = "action", Label = "Accepted", Icon = "??", X = 850, Y = 100,
                Config = new Dictionary<string, object> { { "text", "Great. The thief was last seen near the market." } } },
            new BehaviorNode { Id = declineLineId, Type = "showDialogue", Category = "action", Label = "Declined", Icon = "??", X = 650, Y = 250,
                Config = new Dictionary<string, object> { { "text", "Perhaps another hero will help." } } }
        };

        questGiver.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = questionId, ToPort = "Input" },
            new NodeConnection { FromNodeId = questionId, FromPort = "Output", ToNodeId = checkId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkId, FromPort = "Yes", ToNodeId = acceptId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkId, FromPort = "Unsure", ToNodeId = acceptId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkId, FromPort = "No", ToNodeId = declineLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = acceptId, FromPort = "Output", ToNodeId = acceptLineId, ToPort = "Input" }
        };

        var thief = new QuestEntity
        {
            Id = "thief",
            ScriptName = "Thief",
            DefName = "CREATURE_BANDIT",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true,
            ShowOnMinimap = true
        };

        string thiefKilledId = Guid.NewGuid().ToString();
        string checkAcceptedId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();
        string notAcceptedId = Guid.NewGuid().ToString();

        thief.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = thiefKilledId, Type = "onKilledByHero", Category = "trigger", Label = "When Killed", Icon = "??", X = 50, Y = 350 },
            new BehaviorNode { Id = checkAcceptedId, Type = "checkStateBool", Category = "condition", Label = "Quest Accepted?", Icon = "?", X = 250, Y = 350,
                Config = new Dictionary<string, object> { { "name", "questAccepted" }, { "value", "true" } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete", Icon = "?", X = 450, Y = 300,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } },
            new BehaviorNode { Id = notAcceptedId, Type = "showDialogue", Category = "action", Label = "Not Accepted", Icon = "??", X = 450, Y = 420,
                Config = new Dictionary<string, object> { { "text", "The thief is dead, but you never accepted the job." } } }
        };

        thief.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = thiefKilledId, FromPort = "Output", ToNodeId = checkAcceptedId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkAcceptedId, FromPort = "True", ToNodeId = completeId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkAcceptedId, FromPort = "False", ToNodeId = notAcceptedId, ToPort = "Input" }
        };

        project.Entities.Add(questGiver);
        project.Entities.Add(thief);

        return new QuestTemplate
        {
            Name = "Quest Board Starter",
            Description = "Accept a quest from a giver, defeat a target, and complete the quest.",
            Category = "Starter",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateDemonDoorTemplate()
    {
        var project = new QuestProject
        {
            Name = "MyDemonDoorQuest",
            Id = 50009,
            DisplayName = "Demon Door",
            Description = "Offer an item to open the door",
            Regions = new ObservableCollection<string> { "Greatwood" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Appease the Demon Door",
            ObjectiveRegion1 = "Greatwood",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true
        };

        project.Rewards = new QuestRewards
        {
            Gold = 800,
            Renown = 80,
            Experience = 150
        };

        var door = new QuestEntity
        {
            Id = "demon_door",
            ScriptName = "DemonDoor",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = true
        };

        string talkId = Guid.NewGuid().ToString();
        string checkItemId = Guid.NewGuid().ToString();
        string takeItemId = Guid.NewGuid().ToString();
        string openLineId = Guid.NewGuid().ToString();
        string lockedLineId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        door.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 50, Y = 150 },
            new BehaviorNode { Id = checkItemId, Type = "checkHasItem", Category = "condition", Label = "Has Offering?", Icon = "???", X = 250, Y = 150,
                Config = new Dictionary<string, object> { { "item", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = takeItemId, Type = "takeItem", Category = "action", Label = "Take Offering", Icon = "??", X = 450, Y = 100,
                Config = new Dictionary<string, object> { { "item", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = openLineId, Type = "showDialogue", Category = "action", Label = "Door Opens", Icon = "??", X = 650, Y = 100,
                Config = new Dictionary<string, object> { { "text", "The door is pleased... it opens." } } },
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete", Icon = "?", X = 850, Y = 100,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } },
            new BehaviorNode { Id = lockedLineId, Type = "showDialogue", Category = "action", Label = "Door Locked", Icon = "??", X = 450, Y = 250,
                Config = new Dictionary<string, object> { { "text", "I require an offering..." } } }
        };

        door.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = checkItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkItemId, FromPort = "True", ToNodeId = takeItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = checkItemId, FromPort = "False", ToNodeId = lockedLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = takeItemId, FromPort = "Output", ToNodeId = openLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = openLineId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(door);

        return new QuestTemplate
        {
            Name = "Demon Door",
            Description = "Offer an item to open the door and complete the quest.",
            Category = "Puzzle",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableShowcaseTemplate()
    {
        var project = new QuestProject
        {
            Name = "VariablesShowcase",
            Id = 50010,
            DisplayName = "Variables: Internal & External",
            Description = "Demonstrates internal variables, exposed variables, and external variable access.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the variable demo NPCs",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true,
            UseQuestEndScreen = false
        };

        var source = new QuestEntity
        {
            Id = "var_source",
            ScriptName = "VarSource",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        source.Variables.Add(new EntityVariable
        {
            Name = "Greeting",
            Type = "String",
            DefaultValue = "Hello there!",
            IsExposed = false
        });

        source.Variables.Add(new EntityVariable
        {
            Name = "IsReady",
            Type = "Boolean",
            DefaultValue = "false",
            IsExposed = true
        });

        string sourceTalkId = Guid.NewGuid().ToString();
        string setGreetingId = Guid.NewGuid().ToString();
        string setReadyId = Guid.NewGuid().ToString();
        string speakGreetingId = Guid.NewGuid().ToString();

        source.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = sourceTalkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 120 },
            new BehaviorNode { Id = setGreetingId, Type = "var_set_Greeting", Category = "variable", Label = "Set Greeting", Icon = "??", X = 260, Y = 120,
                Config = new Dictionary<string, object> { { "value", "Greetings, hero!" } } },
            new BehaviorNode { Id = setReadyId, Type = "var_set_IsReady", Category = "variable", Label = "Set IsReady", Icon = "??", X = 440, Y = 120,
                Config = new Dictionary<string, object> { { "value", "true" } } },
            new BehaviorNode { Id = speakGreetingId, Type = "showDialogue", Category = "action", Label = "Speak Greeting", Icon = "??", X = 620, Y = 120,
                Config = new Dictionary<string, object> { { "text", "$Greeting" } } }
        };

        source.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = sourceTalkId, FromPort = "Output", ToNodeId = setGreetingId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setGreetingId, FromPort = "Output", ToNodeId = setReadyId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setReadyId, FromPort = "Output", ToNodeId = speakGreetingId, ToPort = "Input" }
        };

        var listener = new QuestEntity
        {
            Id = "var_listener",
            ScriptName = "VarListener",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        string listenerTalkId = Guid.NewGuid().ToString();
        string getReadyId = Guid.NewGuid().ToString();
        string branchId = Guid.NewGuid().ToString();
        string readyLineId = Guid.NewGuid().ToString();
        string notReadyLineId = Guid.NewGuid().ToString();

        listener.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = listenerTalkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 320 },
            new BehaviorNode { Id = getReadyId, Type = "var_get_ext_VarSource.IsReady", Category = "variable", Label = "Get VarSource.IsReady", Icon = "??", X = 260, Y = 260,
                Config = new Dictionary<string, object>
                {
                    { "extEntity", "VarSource" },
                    { "extVariable", "IsReady" },
                    { "extType", "Boolean" }
                }
            },
            new BehaviorNode { Id = branchId, Type = "branch", Category = "flow", Label = "Branch", Icon = "??", X = 440, Y = 320,
                Config = new Dictionary<string, object> { { "condition", "$@VarSource.IsReady" } } },
            new BehaviorNode { Id = readyLineId, Type = "showDialogue", Category = "action", Label = "Ready", Icon = "??", X = 620, Y = 260,
                Config = new Dictionary<string, object> { { "text", "I see you're ready. Let's begin." } } },
            new BehaviorNode { Id = notReadyLineId, Type = "showDialogue", Category = "action", Label = "Not Ready", Icon = "??", X = 620, Y = 380,
                Config = new Dictionary<string, object> { { "text", "Come back after speaking with VarSource." } } }
        };

        listener.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = listenerTalkId, FromPort = "Output", ToNodeId = branchId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "True", ToNodeId = readyLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "False", ToNodeId = notReadyLineId, ToPort = "Input" },
            new NodeConnection { FromNodeId = getReadyId, FromPort = "Value", ToNodeId = branchId, ToPort = "Condition" }
        };

        project.Entities.Add(source);
        project.Entities.Add(listener);

        return new QuestTemplate
        {
            Name = "Variables: Internal & External",
            Description = "Showcases internal variables, exposed variables, and external variable reads.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableStringTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarStringQuest",
            Id = 50011,
            DisplayName = "Variable: String",
            Description = "Demonstrates an internal string variable.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the string demo NPC",
            ObjectiveRegion1 = "Oakvale"
        };

        var npc = new QuestEntity
        {
            Id = "var_string_npc",
            ScriptName = "VarStringNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        npc.Variables.Add(new EntityVariable
        {
            Name = "Greeting",
            Type = "String",
            DefaultValue = "Hello, hero!",
            IsExposed = false
        });

        string talkId = Guid.NewGuid().ToString();
        string setId = Guid.NewGuid().ToString();
        string speakId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 120 },
            new BehaviorNode { Id = setId, Type = "var_set_Greeting", Category = "variable", Label = "Set Greeting", Icon = "??", X = 260, Y = 120,
                Config = new Dictionary<string, object> { { "value", "Nice to meet you." } } },
            new BehaviorNode { Id = speakId, Type = "showDialogue", Category = "action", Label = "Speak", Icon = "??", X = 440, Y = 120,
                Config = new Dictionary<string, object> { { "text", "$Greeting" } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = setId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setId, FromPort = "Output", ToNodeId = speakId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Variable: String",
            Description = "Internal string variable usage.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableBooleanTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarBoolQuest",
            Id = 50012,
            DisplayName = "Variable: Boolean",
            Description = "Demonstrates an internal boolean variable.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the boolean demo NPC",
            ObjectiveRegion1 = "Oakvale"
        };

        var npc = new QuestEntity
        {
            Id = "var_bool_npc",
            ScriptName = "VarBoolNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        npc.Variables.Add(new EntityVariable
        {
            Name = "IsReady",
            Type = "Boolean",
            DefaultValue = "false",
            IsExposed = false
        });

        string talkId = Guid.NewGuid().ToString();
        string setId = Guid.NewGuid().ToString();
        string branchId = Guid.NewGuid().ToString();
        string readyId = Guid.NewGuid().ToString();
        string notReadyId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 260 },
            new BehaviorNode { Id = setId, Type = "var_set_IsReady", Category = "variable", Label = "Set IsReady", Icon = "??", X = 260, Y = 260,
                Config = new Dictionary<string, object> { { "value", "true" } } },
            new BehaviorNode { Id = branchId, Type = "branch", Category = "flow", Label = "Branch", Icon = "??", X = 440, Y = 260,
                Config = new Dictionary<string, object> { { "condition", "$IsReady" } } },
            new BehaviorNode { Id = readyId, Type = "showDialogue", Category = "action", Label = "Ready", Icon = "??", X = 620, Y = 220,
                Config = new Dictionary<string, object> { { "text", "Ready to go." } } },
            new BehaviorNode { Id = notReadyId, Type = "showDialogue", Category = "action", Label = "Not Ready", Icon = "??", X = 620, Y = 320,
                Config = new Dictionary<string, object> { { "text", "Not ready yet." } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = setId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setId, FromPort = "Output", ToNodeId = branchId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "True", ToNodeId = readyId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "False", ToNodeId = notReadyId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setId, FromPort = "Value", ToNodeId = branchId, ToPort = "Condition" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Variable: Boolean",
            Description = "Internal boolean variable usage.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableIntegerTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarIntQuest",
            Id = 50013,
            DisplayName = "Variable: Integer",
            Description = "Demonstrates an internal integer variable.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the integer demo NPC",
            ObjectiveRegion1 = "Oakvale"
        };

        var npc = new QuestEntity
        {
            Id = "var_int_npc",
            ScriptName = "VarIntNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        npc.Variables.Add(new EntityVariable
        {
            Name = "Count",
            Type = "Integer",
            DefaultValue = "0",
            IsExposed = false
        });

        string talkId = Guid.NewGuid().ToString();
        string setId = Guid.NewGuid().ToString();
        string speakId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 120 },
            new BehaviorNode { Id = setId, Type = "var_set_Count", Category = "variable", Label = "Set Count", Icon = "??", X = 260, Y = 120,
                Config = new Dictionary<string, object> { { "value", "3" } } },
            new BehaviorNode { Id = speakId, Type = "showDialogue", Category = "action", Label = "Speak", Icon = "??", X = 440, Y = 120,
                Config = new Dictionary<string, object> { { "text", "Count set to $Count." } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = setId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setId, FromPort = "Output", ToNodeId = speakId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Variable: Integer",
            Description = "Internal integer variable usage.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableFloatTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarFloatQuest",
            Id = 50014,
            DisplayName = "Variable: Float",
            Description = "Demonstrates an internal float variable.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the float demo NPC",
            ObjectiveRegion1 = "Oakvale"
        };

        var npc = new QuestEntity
        {
            Id = "var_float_npc",
            ScriptName = "VarFloatNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        npc.Variables.Add(new EntityVariable
        {
            Name = "Progress",
            Type = "Float",
            DefaultValue = "0.0",
            IsExposed = false
        });

        string talkId = Guid.NewGuid().ToString();
        string setId = Guid.NewGuid().ToString();
        string speakId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 120 },
            new BehaviorNode { Id = setId, Type = "var_set_Progress", Category = "variable", Label = "Set Progress", Icon = "??", X = 260, Y = 120,
                Config = new Dictionary<string, object> { { "value", "0.75" } } },
            new BehaviorNode { Id = speakId, Type = "showDialogue", Category = "action", Label = "Speak", Icon = "??", X = 440, Y = 120,
                Config = new Dictionary<string, object> { { "text", "Progress is now $Progress." } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = setId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setId, FromPort = "Output", ToNodeId = speakId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Variable: Float",
            Description = "Internal float variable usage.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableObjectTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarObjectQuest",
            Id = 50015,
            DisplayName = "Variable: Object",
            Description = "Demonstrates an internal object variable.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the object demo NPC",
            ObjectiveRegion1 = "Oakvale"
        };

        var npc = new QuestEntity
        {
            Id = "var_object_npc",
            ScriptName = "VarObjectNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        npc.Variables.Add(new EntityVariable
        {
            Name = "RewardItem",
            Type = "Object",
            DefaultValue = "OBJECT_APPLE",
            IsExposed = false
        });

        string talkId = Guid.NewGuid().ToString();
        string setId = Guid.NewGuid().ToString();
        string giveId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 120 },
            new BehaviorNode { Id = setId, Type = "var_set_RewardItem", Category = "variable", Label = "Set RewardItem", Icon = "??", X = 260, Y = 120,
                Config = new Dictionary<string, object> { { "value", "OBJECT_APPLE" } } },
            new BehaviorNode { Id = giveId, Type = "giveItem", Category = "action", Label = "Give Item", Icon = "??", X = 440, Y = 120,
                Config = new Dictionary<string, object> { { "item", "$RewardItem" }, { "amount", "1" } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = setId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setId, FromPort = "Output", ToNodeId = giveId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Variable: Object",
            Description = "Internal object variable usage.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableBranchTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarBranchQuest",
            Id = 50016,
            DisplayName = "Variables: Branch",
            Description = "Shows a branch using a variable value.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Talk to the branch demo NPC",
            ObjectiveRegion1 = "Oakvale"
        };

        var npc = new QuestEntity
        {
            Id = "var_branch_npc",
            ScriptName = "VarBranchNPC",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        npc.Variables.Add(new EntityVariable
        {
            Name = "HasKey",
            Type = "Boolean",
            DefaultValue = "false",
            IsExposed = false
        });

        string talkId = Guid.NewGuid().ToString();
        string branchId = Guid.NewGuid().ToString();
        string yesId = Guid.NewGuid().ToString();
        string noId = Guid.NewGuid().ToString();

        npc.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 260 },
            new BehaviorNode { Id = branchId, Type = "branch", Category = "flow", Label = "Branch", Icon = "??", X = 440, Y = 260,
                Config = new Dictionary<string, object> { { "condition", "$HasKey" } } },
            new BehaviorNode { Id = yesId, Type = "showDialogue", Category = "action", Label = "Has Key", Icon = "??", X = 620, Y = 220,
                Config = new Dictionary<string, object> { { "text", "You have the key." } } },
            new BehaviorNode { Id = noId, Type = "showDialogue", Category = "action", Label = "No Key", Icon = "??", X = 620, Y = 320,
                Config = new Dictionary<string, object> { { "text", "You do not have the key." } } }
        };

        npc.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = branchId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "True", ToNodeId = yesId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "False", ToNodeId = noId, ToPort = "Input" }
        };

        project.Entities.Add(npc);

        return new QuestTemplate
        {
            Name = "Variables: Branch",
            Description = "Branching flow based on a variable value.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }

    private QuestTemplate CreateVariableExternalFlowTemplate()
    {
        var project = new QuestProject
        {
            Name = "VarExternalFlowQuest",
            Id = 50017,
            DisplayName = "Variables: External Item Flow",
            Description = "Give an item to set an exposed variable, then read it from another entity.",
            Regions = new ObservableCollection<string> { "Oakvale" },
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Give the item, then talk to the listener",
            ObjectiveRegion1 = "Oakvale",
            IsGoldQuest = true
        };

        var source = new QuestEntity
        {
            Id = "var_external_source",
            ScriptName = "VarSource",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true
        };

        source.Variables.Add(new EntityVariable
        {
            Name = "HasToken",
            Type = "Boolean",
            DefaultValue = "false",
            IsExposed = true
        });

        const string itemId = "OBJECT_TEDDY_BEAR_UNGIVEABLE";
        string talkId = Guid.NewGuid().ToString();
        string presentedId = Guid.NewGuid().ToString();
        string hasItemId = Guid.NewGuid().ToString();
        string takeItemId = Guid.NewGuid().ToString();
        string setTrueId = Guid.NewGuid().ToString();
        string confirmId = Guid.NewGuid().ToString();
        string noItemId = Guid.NewGuid().ToString();
        string clearHighlightId = Guid.NewGuid().ToString();
        string hideMarkerId = Guid.NewGuid().ToString();
        string highlightListenerId = Guid.NewGuid().ToString();
        string showListenerMarkerId = Guid.NewGuid().ToString();

        source.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = talkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 220 },
            new BehaviorNode { Id = hasItemId, Type = "checkHasItem", Category = "condition", Label = "Has Item?", Icon = "??", X = 280, Y = 220,
                Config = new Dictionary<string, object> { { "item", itemId } } },
              new BehaviorNode { Id = noItemId, Type = "showDialogue", Category = "action", Label = "Needs Item", Icon = "??", X = 520, Y = 260,
                  Config = new Dictionary<string, object> { { "text", "A bully's leaning on a boy to hand over his bear. Go sort the bully out and bring the bear here. The girl can manage without it for once." } } },
            new BehaviorNode { Id = presentedId, Type = "onItemPresented", Category = "trigger", Label = "When Item Given", Icon = "??", X = 80, Y = 80,
                Config = new Dictionary<string, object> { { "item", itemId } } },
            new BehaviorNode { Id = takeItemId, Type = "takeItem", Category = "action", Label = "Take Item", Icon = "??", X = 520, Y = 80,
                Config = new Dictionary<string, object> { { "item", itemId } } },
            new BehaviorNode { Id = setTrueId, Type = "var_set_HasToken", Category = "variable", Label = "Set HasToken", Icon = "??", X = 760, Y = 80,
                Config = new Dictionary<string, object> { { "value", "true" } } },
              new BehaviorNode { Id = confirmId, Type = "showDialogue", Category = "action", Label = "Confirm", Icon = "??", X = 1000, Y = 80,
                  Config = new Dictionary<string, object> { { "text", "Lovely. The boy gets his bear, the bully gets a bruise, and you get the glory. Go tell the listener." } } },
              new BehaviorNode { Id = highlightListenerId, Type = "highlightQuestTargetByName", Category = "action", Label = "Highlight Listener", Icon = "??", X = 1200, Y = 80,
                  Config = new Dictionary<string, object> { { "targetScriptName", "VarListener" } } },
              new BehaviorNode { Id = showListenerMarkerId, Type = "showMinimapMarkerByName", Category = "action", Label = "Show Listener Marker", Icon = "??", X = 1400, Y = 80,
                  Config = new Dictionary<string, object> { { "targetScriptName", "VarListener" }, { "markerName", "Listener" } } },
              new BehaviorNode { Id = clearHighlightId, Type = "clearQuestTargetHighlight", Category = "action", Label = "Clear Quest Target", Icon = "??", X = 1600, Y = 80 },
              new BehaviorNode { Id = hideMarkerId, Type = "hideMinimapMarker", Category = "action", Label = "Hide Minimap Marker", Icon = "??", X = 1800, Y = 80 }
        };

        source.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = talkId, FromPort = "Output", ToNodeId = hasItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = hasItemId, FromPort = "True", ToNodeId = takeItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = hasItemId, FromPort = "False", ToNodeId = noItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = presentedId, FromPort = "Output", ToNodeId = takeItemId, ToPort = "Input" },
            new NodeConnection { FromNodeId = takeItemId, FromPort = "Output", ToNodeId = setTrueId, ToPort = "Input" },
            new NodeConnection { FromNodeId = setTrueId, FromPort = "Output", ToNodeId = confirmId, ToPort = "Input" },
            new NodeConnection { FromNodeId = confirmId, FromPort = "Output", ToNodeId = highlightListenerId, ToPort = "Input" },
            new NodeConnection { FromNodeId = highlightListenerId, FromPort = "Output", ToNodeId = showListenerMarkerId, ToPort = "Input" },
            new NodeConnection { FromNodeId = showListenerMarkerId, FromPort = "Output", ToNodeId = clearHighlightId, ToPort = "Input" },
            new NodeConnection { FromNodeId = clearHighlightId, FromPort = "Output", ToNodeId = hideMarkerId, ToPort = "Input" }
        };

        var listener = new QuestEntity
        {
            Id = "var_external_listener",
            ScriptName = "VarListener",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_FEMALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            MakeBehavioral = true,
            ExclusiveControl = true,
            IsQuestTarget = false,
            ShowOnMinimap = false
        };

        string listenerTalkId = Guid.NewGuid().ToString();
        string branchId = Guid.NewGuid().ToString();
        string yesId = Guid.NewGuid().ToString();
        string noId = Guid.NewGuid().ToString();
        string completeId = Guid.NewGuid().ToString();

        listener.Nodes = new List<BehaviorNode>
        {
            new BehaviorNode { Id = listenerTalkId, Type = "onHeroTalks", Category = "trigger", Label = "Hero Talks", Icon = "??", X = 80, Y = 320 },
            new BehaviorNode { Id = branchId, Type = "branch", Category = "flow", Label = "Branch", Icon = "??", X = 260, Y = 320,
                Config = new Dictionary<string, object> { { "condition", "$@VarSource.HasToken" } } },
            new BehaviorNode { Id = yesId, Type = "showDialogue", Category = "action", Label = "Has Token", Icon = "??", X = 760, Y = 280,
                Config = new Dictionary<string, object> { { "text", "Ah, the bear! The boy will be chuffed. The girl can make do with a stick and her imagination." } } },
            new BehaviorNode { Id = noId, Type = "showDialogue", Category = "action", Label = "No Token", Icon = "??", X = 440, Y = 360,
                Config = new Dictionary<string, object> { { "text", "No bear, no peace. Go give that bully a lesson and bring the bear to VarSource." } } }
            ,
            new BehaviorNode { Id = completeId, Type = "completeQuest", Category = "action", Label = "Complete Quest", Icon = "??", X = 920, Y = 280,
                Config = new Dictionary<string, object> { { "showScreen", "true" } } }
        };

        listener.Connections = new List<NodeConnection>
        {
            new NodeConnection { FromNodeId = listenerTalkId, FromPort = "Output", ToNodeId = branchId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "True", ToNodeId = yesId, ToPort = "Input" },
            new NodeConnection { FromNodeId = branchId, FromPort = "False", ToNodeId = noId, ToPort = "Input" },
            new NodeConnection { FromNodeId = yesId, FromPort = "Output", ToNodeId = completeId, ToPort = "Input" }
        };

        project.Entities.Add(source);
        project.Entities.Add(listener);

        return new QuestTemplate
        {
            Name = "Variables: External Item Flow",
            Description = "Set an exposed variable by giving an item, then read it from another entity.",
            Category = "Variables",
            Difficulty = "Beginner",
            Template = project
        };
    }
}
