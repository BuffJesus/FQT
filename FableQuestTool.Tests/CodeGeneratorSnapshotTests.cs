using System.IO;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class CodeGeneratorSnapshotTests
{
    [Fact]
    public void GenerateQuestScript_MatchesSnapshot()
    {
        QuestProject quest = BuildSnapshotQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_Minimal.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_MatchesSnapshot()
    {
        QuestProject quest = BuildSnapshotQuest();
        QuestEntity entity = new QuestEntity
        {
            ScriptName = "TestEntity",
            AcquireControl = true,
            MakeBehavioral = false
        };

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_Minimal.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_ContainerReward_MatchesSnapshot()
    {
        QuestProject quest = BuildContainerRewardQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_ContainerReward.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_Branching_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "BranchQuest" };
        QuestEntity entity = BuildBranchingEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_Branching.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_ManualContainerReward_MatchesSnapshot()
    {
        QuestProject quest = BuildManualContainerQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_ManualContainerReward.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateContainerEntityScript_ManualContainerReward_MatchesSnapshot()
    {
        QuestProject quest = BuildManualContainerQuest();
        ContainerReward container = quest.Rewards.Container!;

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateContainerEntityScript(quest, container);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_ManualContainer.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_RandomChoice_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "RandomQuest" };
        QuestEntity entity = BuildRandomChoiceEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_RandomChoice.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_Parallel_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "ParallelQuest" };
        QuestEntity entity = BuildParallelEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_Parallel.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_ObjectReward_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "ObjectRewardQuest" };
        QuestEntity entity = BuildObjectRewardEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_ObjectReward.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_Loop_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "LoopQuest" };
        QuestEntity entity = BuildLoopEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_Loop.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_WhileLoop_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "WhileQuest" };
        QuestEntity entity = BuildWhileLoopEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_WhileLoop.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateEntityScript_Event_MatchesSnapshot()
    {
        QuestProject quest = new QuestProject { Name = "EventQuest" };
        QuestEntity entity = BuildEventEntity();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "EntityScript_Event.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_ExposedVariables_MatchesSnapshot()
    {
        QuestProject quest = BuildExposedVariablesQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_ExposedVariables.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_Threads_MatchesSnapshot()
    {
        QuestProject quest = BuildThreadQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_Threads.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_QuestCardScreens_MatchesSnapshot()
    {
        QuestProject quest = BuildQuestCardQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_QuestCardScreens.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_StatePersistence_MatchesSnapshot()
    {
        QuestProject quest = BuildStatePersistenceQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_StatePersistence.lua"));
        string expected = LoadSnapshot(snapshotPath, script);

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateRegistrationSnippet_MatchesSnapshot()
    {
        QuestProject quest = BuildRegistrationQuest();

        CodeGenerator generator = new CodeGenerator();
        string snippet = generator.GenerateRegistrationSnippet(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestRegistrationSnippet.lua"));
        string expected = LoadSnapshot(snapshotPath, snippet);

        Assert.Equal(expected, NormalizeLineEndings(snippet));
    }

    private static QuestProject BuildSnapshotQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "SnapshotQuest",
            DisplayName = "Snapshot Quest",
            Description = "Short quest",
            ObjectiveText = "Do it",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true,
            IsGoldQuest = true,
            IsStoryQuest = false
        };

        quest.Regions.Add("Oakvale");
        quest.Rewards.Gold = 10;
        quest.Rewards.Renown = 5;
        quest.Rewards.Items.Add("OBJECT_APPLE");

        return quest;
    }

    private static QuestProject BuildContainerRewardQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "ContainerQuest",
            DisplayName = "Container Quest"
        };

        quest.Regions.Add("Oakvale");
        quest.Rewards.Container = new ContainerReward
        {
            ContainerDefName = "OBJECT_CHEST",
            ContainerScriptName = "RewardChest",
            SpawnLocation = ContainerSpawnLocation.NearMarker,
            SpawnReference = "MK_REWARD",
            AutoGiveOnComplete = true
        };
        quest.Rewards.Container.Items.Add("OBJECT_APPLE");
        quest.Rewards.Container.Items.Add("OBJECT_CARROT");

        return quest;
    }

    private static QuestEntity BuildBranchingEntity()
    {
        BehaviorNode trigger = new BehaviorNode
        {
            Id = "trigger",
            Type = "onHeroUsed",
            Category = "trigger"
        };
        BehaviorNode check = new BehaviorNode
        {
            Id = "check",
            Type = "checkStateBool",
            Category = "condition",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["name"] = "HasItem",
                ["value"] = "true"
            }
        };
        BehaviorNode showTrue = new BehaviorNode
        {
            Id = "showTrue",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "All good",
                ["duration"] = "2.0"
            }
        };
        BehaviorNode showFalse = new BehaviorNode
        {
            Id = "showFalse",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Not yet",
                ["duration"] = "2.0"
            }
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "BranchEntity",
            MakeBehavioral = true,
            AcquireControl = true
        };

        entity.Nodes.Add(trigger);
        entity.Nodes.Add(check);
        entity.Nodes.Add(showTrue);
        entity.Nodes.Add(showFalse);

        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = trigger.Id,
            FromPort = "Output",
            ToNodeId = check.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = check.Id,
            FromPort = "True",
            ToNodeId = showTrue.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = check.Id,
            FromPort = "False",
            ToNodeId = showFalse.Id,
            ToPort = "Input"
        });

        return entity;
    }

    private static QuestProject BuildManualContainerQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "ManualContainerQuest",
            DisplayName = "Manual Container Quest"
        };

        quest.Regions.Add("Oakvale");
        quest.Rewards.Container = new ContainerReward
        {
            ContainerDefName = "OBJECT_CHEST",
            ContainerScriptName = "ManualChest",
            SpawnLocation = ContainerSpawnLocation.NearMarker,
            SpawnReference = "MK_REWARD",
            AutoGiveOnComplete = false
        };
        quest.Rewards.Container.Items.Add("OBJECT_APPLE");
        quest.Rewards.Container.Items.Add("OBJECT_CARROT");

        return quest;
    }

    private static QuestEntity BuildRandomChoiceEntity()
    {
        BehaviorNode trigger = new BehaviorNode
        {
            Id = "trigger",
            Type = "onHeroUsed",
            Category = "trigger"
        };
        BehaviorNode random = new BehaviorNode
        {
            Id = "random",
            Type = "randomChoice",
            Category = "flow",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["maxChoice"] = "3"
            }
        };
        BehaviorNode choice1 = new BehaviorNode
        {
            Id = "choice1",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Choice 1",
                ["duration"] = "1.0"
            }
        };
        BehaviorNode choice2 = new BehaviorNode
        {
            Id = "choice2",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Choice 2",
                ["duration"] = "1.0"
            }
        };
        BehaviorNode choice3 = new BehaviorNode
        {
            Id = "choice3",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Choice 3",
                ["duration"] = "1.0"
            }
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "RandomEntity",
            MakeBehavioral = true,
            AcquireControl = true
        };

        entity.Nodes.Add(trigger);
        entity.Nodes.Add(random);
        entity.Nodes.Add(choice1);
        entity.Nodes.Add(choice2);
        entity.Nodes.Add(choice3);

        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = trigger.Id,
            FromPort = "Output",
            ToNodeId = random.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = random.Id,
            FromPort = "Output",
            ToNodeId = choice1.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = random.Id,
            FromPort = "Output",
            ToNodeId = choice2.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = random.Id,
            FromPort = "Output",
            ToNodeId = choice3.Id,
            ToPort = "Input"
        });

        return entity;
    }

    private static QuestEntity BuildParallelEntity()
    {
        BehaviorNode trigger = new BehaviorNode
        {
            Id = "trigger",
            Type = "onHeroUsed",
            Category = "trigger"
        };
        BehaviorNode parallel = new BehaviorNode
        {
            Id = "parallel",
            Type = "parallel",
            Category = "flow"
        };
        BehaviorNode left = new BehaviorNode
        {
            Id = "left",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Left path",
                ["duration"] = "1.0"
            }
        };
        BehaviorNode right = new BehaviorNode
        {
            Id = "right",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Right path",
                ["duration"] = "1.0"
            }
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "ParallelEntity",
            MakeBehavioral = true,
            AcquireControl = true
        };

        entity.Nodes.Add(trigger);
        entity.Nodes.Add(parallel);
        entity.Nodes.Add(left);
        entity.Nodes.Add(right);

        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = trigger.Id,
            FromPort = "Output",
            ToNodeId = parallel.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = parallel.Id,
            FromPort = "Output",
            ToNodeId = left.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = parallel.Id,
            FromPort = "Output",
            ToNodeId = right.Id,
            ToPort = "Input"
        });

        return entity;
    }

    private static QuestEntity BuildObjectRewardEntity()
    {
        QuestEntity entity = new QuestEntity
        {
            ScriptName = "RewardObject",
            EntityType = EntityType.Object,
            MakeBehavioral = true,
            AcquireControl = true,
            ObjectReward = new ObjectReward
            {
                Gold = 5,
                Experience = 10,
                DestroyAfterReward = false,
                OneTimeOnly = true,
                ShowMessage = true
            }
        };

        entity.ObjectReward.Items.Add("OBJECT_APPLE");
        entity.ObjectReward.Items.Add("OBJECT_CARROT");

        return entity;
    }

    private static QuestEntity BuildLoopEntity()
    {
        BehaviorNode trigger = new BehaviorNode
        {
            Id = "trigger",
            Type = "onHeroUsed",
            Category = "trigger"
        };
        BehaviorNode loop = new BehaviorNode
        {
            Id = "loop",
            Type = "loop",
            Category = "flow",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["count"] = "2"
            }
        };
        BehaviorNode action = new BehaviorNode
        {
            Id = "action",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Looping",
                ["duration"] = "1.0"
            }
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "LoopEntity",
            MakeBehavioral = true,
            AcquireControl = true
        };

        entity.Nodes.Add(trigger);
        entity.Nodes.Add(loop);
        entity.Nodes.Add(action);

        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = trigger.Id,
            FromPort = "Output",
            ToNodeId = loop.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = loop.Id,
            FromPort = "Output",
            ToNodeId = action.Id,
            ToPort = "Input"
        });

        return entity;
    }

    private static QuestEntity BuildWhileLoopEntity()
    {
        BehaviorNode trigger = new BehaviorNode
        {
            Id = "trigger",
            Type = "onHeroUsed",
            Category = "trigger"
        };
        BehaviorNode loop = new BehaviorNode
        {
            Id = "whileLoop",
            Type = "whileLoop",
            Category = "flow",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["condition"] = "Me:IsAlive()"
            }
        };
        BehaviorNode action = new BehaviorNode
        {
            Id = "action",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "While",
                ["duration"] = "1.0"
            }
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "WhileEntity",
            MakeBehavioral = true,
            AcquireControl = true
        };

        entity.Nodes.Add(trigger);
        entity.Nodes.Add(loop);
        entity.Nodes.Add(action);

        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = trigger.Id,
            FromPort = "Output",
            ToNodeId = loop.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = loop.Id,
            FromPort = "Output",
            ToNodeId = action.Id,
            ToPort = "Input"
        });

        return entity;
    }

    private static QuestEntity BuildEventEntity()
    {
        BehaviorNode defineEvent = new BehaviorNode
        {
            Id = "defineEvent",
            Type = "defineEvent",
            Category = "custom",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["eventName"] = "OnGift"
            }
        };
        BehaviorNode eventAction = new BehaviorNode
        {
            Id = "eventAction",
            Type = "showMessage",
            Category = "action",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["text"] = "Thanks",
                ["duration"] = "2.0"
            }
        };
        BehaviorNode trigger = new BehaviorNode
        {
            Id = "trigger",
            Type = "onHeroUsed",
            Category = "trigger"
        };
        BehaviorNode callEvent = new BehaviorNode
        {
            Id = "callEvent",
            Type = "callEvent",
            Category = "custom",
            Config = new System.Collections.Generic.Dictionary<string, object>
            {
                ["eventName"] = "OnGift"
            }
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "EventEntity",
            MakeBehavioral = true,
            AcquireControl = true
        };

        entity.Nodes.Add(defineEvent);
        entity.Nodes.Add(eventAction);
        entity.Nodes.Add(trigger);
        entity.Nodes.Add(callEvent);

        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = defineEvent.Id,
            FromPort = "Output",
            ToNodeId = eventAction.Id,
            ToPort = "Input"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = trigger.Id,
            FromPort = "Output",
            ToNodeId = callEvent.Id,
            ToPort = "Input"
        });

        return entity;
    }

    private static QuestProject BuildExposedVariablesQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "ExposedQuest",
            DisplayName = "Exposed Quest"
        };

        quest.Regions.Add("Oakvale");

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "ExposedEntity"
        };
        entity.Variables.Add(new EntityVariable
        {
            Name = "IsReady",
            Type = "Boolean",
            DefaultValue = "true",
            IsExposed = true
        });
        entity.Variables.Add(new EntityVariable
        {
            Name = "Count",
            Type = "Integer",
            DefaultValue = "3",
            IsExposed = true
        });
        entity.Variables.Add(new EntityVariable
        {
            Name = "Ratio",
            Type = "Float",
            DefaultValue = "1.5",
            IsExposed = true
        });
        entity.Variables.Add(new EntityVariable
        {
            Name = "Note",
            Type = "String",
            DefaultValue = "Hello",
            IsExposed = true
        });

        quest.Entities.Add(entity);

        return quest;
    }

    private static QuestProject BuildThreadQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "ThreadQuest",
            DisplayName = "Thread Quest"
        };

        quest.Regions.Add("Oakvale");

        QuestEntity spawnEntity = new QuestEntity
        {
            ScriptName = "SpawnedNpc",
            DefName = "CREATURE_VILLAGER_FARMER",
            SpawnMethod = SpawnMethod.AtMarker,
            SpawnMarker = "MK_OVID_DAD"
        };
        quest.Entities.Add(spawnEntity);

        quest.Threads.Add(new QuestThread
        {
            FunctionName = "WatcherThread",
            Region = "Oakvale",
            Description = "Watch for conditions",
            IntervalSeconds = 0.25f,
            ExitStateName = "StopThread",
            ExitStateValue = true
        });

        return quest;
    }

    private static QuestProject BuildQuestCardQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "CardQuest",
            DisplayName = "Card Quest",
            Description = "A story quest",
            ObjectiveText = "Reach the gate",
            ObjectiveRegion1 = "Oakvale",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true,
            IsStoryQuest = true,
            IsGoldQuest = false
        };

        quest.Regions.Add("Oakvale");
        quest.Rewards.Gold = 50;
        quest.Rewards.Renown = 25;
        quest.Rewards.Items.Add("OBJECT_APPLE");

        return quest;
    }

    private static QuestProject BuildStatePersistenceQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "PersistQuest",
            DisplayName = "Persist Quest"
        };

        quest.Regions.Add("Oakvale");
        quest.States.Add(new QuestState { Name = "Flag", Type = "bool", Persist = true, DefaultValue = true });
        quest.States.Add(new QuestState { Name = "Count", Type = "int", Persist = true, DefaultValue = 2 });
        quest.States.Add(new QuestState { Name = "Ratio", Type = "float", Persist = true, DefaultValue = 1.25 });
        quest.States.Add(new QuestState { Name = "Note", Type = "string", Persist = true, DefaultValue = "Hi" });

        return quest;
    }

    private static QuestProject BuildRegistrationQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "RegisterQuest",
            DisplayName = "Register Quest",
            Id = 60000
        };

        quest.Entities.Add(new QuestEntity { ScriptName = "NpcOne" });
        quest.Entities.Add(new QuestEntity { ScriptName = "NpcTwo" });
        quest.Rewards.Container = new ContainerReward
        {
            ContainerScriptName = "RegisterChest",
            AutoGiveOnComplete = false
        };
        quest.Rewards.Container.Items.Add("OBJECT_APPLE");

        return quest;
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd('\n');
    }

    private static string LoadSnapshot(string snapshotPath, string actual)
    {
        if (System.Environment.GetEnvironmentVariable("FQT_UPDATE_SNAPSHOTS") == "1")
        {
            File.WriteAllText(snapshotPath, actual);
        }

        return NormalizeLineEndings(File.ReadAllText(snapshotPath));
    }
}
