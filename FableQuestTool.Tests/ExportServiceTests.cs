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
