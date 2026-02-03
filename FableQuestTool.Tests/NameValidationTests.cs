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
}
