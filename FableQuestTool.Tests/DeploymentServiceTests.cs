using System;
using System.IO;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class DeploymentServiceTests
{
    [Fact]
    public void DeployQuest_WritesScriptsAndRegistersQuest()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        CodeGenerator generator = new CodeGenerator();
        DeploymentService service = new DeploymentService(config, generator);

        QuestProject quest = new QuestProject
        {
            Name = "UnitQuest",
            Id = 50001,
            DisplayName = "Unit Quest"
        };
        quest.Entities.Add(new QuestEntity { ScriptName = "TestNpc" });
        quest.Rewards.Container = new ContainerReward
        {
            AutoGiveOnComplete = false,
            ContainerScriptName = "RewardChest"
        };
        quest.Rewards.Container.Items.Add("OBJECT_APPLE");
        quest.Rewards.Container.Items.Add("OBJECT_CARROT");

        bool result = service.DeployQuest(quest, out string message);

        Assert.True(result, message);
        string questFolder = Path.Combine(tempInstall.RootPath, "FSE", quest.Name);
        Assert.True(File.Exists(Path.Combine(questFolder, $"{quest.Name}.lua")));
        Assert.True(File.Exists(Path.Combine(questFolder, "Entities", "TestNpc.lua")));
        Assert.True(File.Exists(Path.Combine(questFolder, "Entities", "RewardChest.lua")));

        string questsLua = File.ReadAllText(Path.Combine(tempInstall.RootPath, "FSE", "quests.lua"));
        Assert.Contains("UnitQuest = {", questsLua);
        Assert.Contains("TestNpc", questsLua);
        Assert.Contains("RewardChest", questsLua);

        string qstText = File.ReadAllText(Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion.qst"));
        Assert.Contains("AddQuest(\"UnitQuest\"", qstText);
        Assert.Contains("AddQuest(\"FSE_Master\"", qstText);

        string masterText = File.ReadAllText(Path.Combine(tempInstall.RootPath, "FSE", "Master", "FSE_Master.lua"));
        Assert.Contains("quest:ActivateQuest(\"UnitQuest\")", masterText);
    }

    [Fact]
    public void DeployQuest_FailsWhenFseMissing()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        File.Delete(Path.Combine(tempInstall.RootPath, "FSE_Launcher.exe"));
        File.Delete(Path.Combine(tempInstall.RootPath, "FableScriptExtender.dll"));

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());
        QuestProject quest = new QuestProject { Name = "MissingFseQuest", Id = 52000 };
        quest.Entities.Add(new QuestEntity { ScriptName = "NpcA" });

        bool result = service.DeployQuest(quest, out string message);

        Assert.False(result);
        Assert.Contains("FSE installation check failed", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeployQuest_FailsWhenFinalAlbionMissing()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        File.Delete(tempInstall.QstPath);

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());
        QuestProject quest = new QuestProject { Name = "MissingQstQuest", Id = 52001 };
        quest.Entities.Add(new QuestEntity { ScriptName = "NpcB" });

        bool result = service.DeployQuest(quest, out string message);

        Assert.False(result);
        Assert.Contains("FinalAlbion.qst", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeleteQuest_FailsForInvalidName()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());

        bool result = service.DeleteQuest("Bad Name", out string message);

        Assert.False(result);
        Assert.Contains("invalid", message, StringComparison.OrdinalIgnoreCase);
    }
}
