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
        Assert.Contains("[FQT-IO-003]", message, StringComparison.Ordinal);
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
        Assert.Contains("[FQT-IO-006]", message, StringComparison.Ordinal);
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
        Assert.Contains("[FQT-IO-022]", message, StringComparison.Ordinal);
        Assert.Contains("invalid", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LaunchFse_FailsWhenFseMissing()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        File.Delete(Path.Combine(tempInstall.RootPath, "FSE_Launcher.exe"));
        File.Delete(Path.Combine(tempInstall.RootPath, "FableScriptExtender.dll"));

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());

        bool result = service.LaunchFse(out string message);

        Assert.False(result);
        Assert.Contains("[FQT-IO-010]", message, StringComparison.Ordinal);
        Assert.Contains("Cannot launch FSE", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Mods.ini", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeployQuest_DoesNotDuplicateActivation()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        string masterPath = tempInstall.MasterPath;
        File.WriteAllText(masterPath, string.Join("\n", new[]
        {
            "function Main(quest)",
            "    quest:ActivateQuest(\"DupQuest\")",
            "end",
            string.Empty
        }));

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());
        QuestProject quest = new QuestProject { Name = "DupQuest", Id = 53000 };
        quest.Entities.Add(new QuestEntity { ScriptName = "NpcDup" });

        Assert.True(service.DeployQuest(quest, out string message), message);

        string masterText = File.ReadAllText(masterPath);
        int activationCount = masterText.Split("quest:ActivateQuest(\"DupQuest\")").Length - 1;
        Assert.Equal(1, activationCount);
    }

    [Fact]
    public void DeployQuest_MissingMainFunction_ReturnsWarning()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        File.WriteAllText(tempInstall.MasterPath, "function NotMain(quest)\nend\n");

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());
        QuestProject quest = new QuestProject { Name = "NoMainQuest", Id = 53001 };
        quest.Entities.Add(new QuestEntity { ScriptName = "NpcNoMain" });

        bool result = service.DeployQuest(quest, out string message);

        Assert.True(result);
        Assert.Contains("deployed successfully", message, StringComparison.OrdinalIgnoreCase);
        string masterText = File.ReadAllText(tempInstall.MasterPath);
        Assert.DoesNotContain("quest:ActivateQuest(\"NoMainQuest\")", masterText);
    }

    [Fact]
    public void DeployQuest_UpdatesExistingQuestEntry()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        string existingQuestsLua = string.Join("\n", new[]
        {
            "Quests = {",
            "    ExistingQuest = {",
            "        name = \"ExistingQuest\",",
            "        file = \"ExistingQuest/ExistingQuest\",",
            "        id = 50000,",
            "        entity_scripts = {",
            "        }",
            "    },",
            "}",
            string.Empty
        });
        File.WriteAllText(tempInstall.QuestsLuaPath, existingQuestsLua);

        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());
        QuestProject quest = new QuestProject { Name = "ExistingQuest", Id = 54000 };
        quest.Entities.Add(new QuestEntity { ScriptName = "UpdatedNpc" });

        bool result = service.DeployQuest(quest, out string message);

        Assert.True(result, message);
        string questsLua = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.Contains("ExistingQuest = {", questsLua);
        Assert.Contains("UpdatedNpc", questsLua);
        Assert.Contains("id = 54000", questsLua);
    }
}
