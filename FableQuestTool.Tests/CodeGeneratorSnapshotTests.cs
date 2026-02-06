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
        string expected = NormalizeLineEndings(File.ReadAllText(snapshotPath));

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
        string expected = NormalizeLineEndings(File.ReadAllText(snapshotPath));

        Assert.Equal(expected, NormalizeLineEndings(script));
    }

    [Fact]
    public void GenerateQuestScript_ContainerReward_MatchesSnapshot()
    {
        QuestProject quest = BuildContainerRewardQuest();

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        string snapshotPath = TestPaths.GetFixturePath(Path.Combine("Snapshots", "QuestScript_ContainerReward.lua"));
        string expected = NormalizeLineEndings(File.ReadAllText(snapshotPath));

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

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").TrimEnd('\n', '\r');
    }
}
