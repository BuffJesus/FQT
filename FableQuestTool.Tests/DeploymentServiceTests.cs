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
        using TempFableInstall tempInstall = TempFableInstall.Create();

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

    private sealed class TempFableInstall : IDisposable
    {
        private TempFableInstall(string rootPath)
        {
            RootPath = rootPath;
        }

        public string RootPath { get; }

        public static TempFableInstall Create()
        {
            string root = Path.Combine(Path.GetTempPath(), "FqtTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, "data", "Levels"));
            Directory.CreateDirectory(Path.Combine(root, "FSE", "Master"));

            File.WriteAllText(Path.Combine(root, "FSE_Launcher.exe"), string.Empty);
            File.WriteAllText(Path.Combine(root, "FableScriptExtender.dll"), string.Empty);

            string qstSample = Path.Combine(
                TestPaths.GetRepoRoot(),
                "FSE_Source",
                "SampleQuests",
                "NewQuest",
                "FinalAlbion.qst.example");
            string qstTarget = Path.Combine(root, "data", "Levels", "FinalAlbion.qst");
            File.WriteAllText(qstTarget, File.ReadAllText(qstSample));

            string masterPath = Path.Combine(root, "FSE", "Master", "FSE_Master.lua");
            File.WriteAllText(masterPath, "function Main(quest)\nend\n");

            return new TempFableInstall(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
            }
        }
    }
}
