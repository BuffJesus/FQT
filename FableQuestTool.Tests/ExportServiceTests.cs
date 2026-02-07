using System.IO;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ExportServiceTests
{
    [Fact]
    public void Export_WritesQuestAndEntityScripts()
    {
        QuestProject quest = BuildExportQuest();
        ExportService service = new ExportService(new CodeGenerator());

        using TestTempDirectory temp = new TestTempDirectory();
        string output = service.Export(quest, temp.Path);

        string questPath = Path.Combine(output, "ExportQuest.lua");
        string entityPath = Path.Combine(output, "Entities", "ExportEntity.lua");
        string containerPath = Path.Combine(output, "Entities", "ExportChest.lua");
        string registrationPath = Path.Combine(output, "_quests_registration.lua");

        Assert.True(File.Exists(questPath));
        Assert.True(File.Exists(entityPath));
        Assert.True(File.Exists(containerPath));
        Assert.True(File.Exists(registrationPath));

        string registration = File.ReadAllText(registrationPath);
        Assert.Contains("ExportQuest", registration);
    }

    [Fact]
    public void Export_ThrowsOnInvalidBaseDirectory()
    {
        QuestProject quest = BuildExportQuest();
        ExportService service = new ExportService(new CodeGenerator());

        string invalidBase = "?:\\invalid";

        Assert.ThrowsAny<IOException>(() => service.Export(quest, invalidBase));
    }

    [Fact]
    public void Export_SkipsContainerScriptWhenAutoGive()
    {
        QuestProject quest = new QuestProject
        {
            Name = "AutoGiveQuest",
            DisplayName = "Auto Give Quest"
        };
        quest.Entities.Add(new QuestEntity { ScriptName = "AutoEntity" });
        quest.Rewards.Container = new ContainerReward
        {
            ContainerScriptName = "AutoChest",
            AutoGiveOnComplete = true
        };
        quest.Rewards.Container.Items.Add("OBJECT_APPLE");

        ExportService service = new ExportService(new CodeGenerator());

        using TestTempDirectory temp = new TestTempDirectory();
        string output = service.Export(quest, temp.Path);

        string containerPath = Path.Combine(output, "Entities", "AutoChest.lua");
        Assert.False(File.Exists(containerPath));
    }

    private static QuestProject BuildExportQuest()
    {
        QuestProject quest = new QuestProject
        {
            Name = "ExportQuest",
            DisplayName = "Export Quest"
        };
        quest.Entities.Add(new QuestEntity { ScriptName = "ExportEntity" });
        quest.Rewards.Container = new ContainerReward
        {
            ContainerScriptName = "ExportChest",
            AutoGiveOnComplete = false
        };
        quest.Rewards.Container.Items.Add("OBJECT_APPLE");

        return quest;
    }
}
