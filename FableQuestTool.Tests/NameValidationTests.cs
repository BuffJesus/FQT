using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class NameValidationTests
{
    [Fact]
    public void ValidateProject_FindsInvalidQuestName()
    {
        QuestProject project = new QuestProject { Name = "Bad Name" };
        var errors = NameValidation.ValidateProject(project);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateProject_DetectsDuplicateEntityNames()
    {
        QuestProject project = new QuestProject { Name = "ValidQuest" };
        project.Entities.Add(new QuestEntity { ScriptName = "NPC1" });
        project.Entities.Add(new QuestEntity { ScriptName = "NPC1" });

        var errors = NameValidation.ValidateProject(project);
        Assert.Contains(errors, e => e.Contains("Duplicate", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateProject_FlagsInvalidFileNameCharacters()
    {
        QuestProject project = new QuestProject { Name = "Quest/Bad" };

        var errors = NameValidation.ValidateProject(project);

        Assert.Contains(errors, e => e.Contains("invalid file name characters", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateProject_FlagsPathTraversal()
    {
        QuestProject project = new QuestProject { Name = "Quest..Bad" };

        var errors = NameValidation.ValidateProject(project);

        Assert.Contains(errors, e => e.Contains("cannot contain '..'", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateProject_DetectsDuplicateContainerName()
    {
        QuestProject project = new QuestProject { Name = "ValidQuest" };
        project.Entities.Add(new QuestEntity { ScriptName = "RewardChest" });
        project.Rewards.Container = new ContainerReward { ContainerScriptName = "RewardChest" };

        var errors = NameValidation.ValidateProject(project);

        Assert.Contains(errors, e => e.Contains("Duplicate", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateProject_FlagsInvalidEntityScriptName()
    {
        QuestProject project = new QuestProject { Name = "ValidQuest" };
        project.Entities.Add(new QuestEntity { ScriptName = "1BadName" });

        var errors = NameValidation.ValidateProject(project);

        Assert.Contains(errors, e => e.Contains("Entity Script Name", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("valid Lua identifier", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateProject_FlagsInvalidContainerScriptName()
    {
        QuestProject project = new QuestProject { Name = "ValidQuest" };
        project.Rewards.Container = new ContainerReward { ContainerScriptName = "Bad/Chest" };

        var errors = NameValidation.ValidateProject(project);

        Assert.Contains(errors, e => e.Contains("Reward Container Script Name", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("invalid file name characters", System.StringComparison.OrdinalIgnoreCase));
    }
}
