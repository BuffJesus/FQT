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
