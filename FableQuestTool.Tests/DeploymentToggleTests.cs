using System.IO;
using System.Text.RegularExpressions;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class DeploymentToggleTests
{
    [Fact]
    public void ToggleQuest_DisableThenEnable_UpdatesAllFiles()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        CodeGenerator generator = new CodeGenerator();
        DeploymentService service = new DeploymentService(config, generator);

        QuestProject quest = new QuestProject
        {
            Name = "ToggleQuest",
            Id = 51010
        };
        quest.Entities.Add(new QuestEntity { ScriptName = "ToggleNpc" });

        Assert.True(service.DeployQuest(quest, out string deployMessage), deployMessage);

        Assert.True(service.ToggleQuest("ToggleQuest", false, out string disableMessage), disableMessage);
        string questsLua = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.Matches(new Regex(@"^\s*--\s*ToggleQuest\s*=\s*\{", RegexOptions.Multiline), questsLua);

        string masterLua = File.ReadAllText(tempInstall.MasterPath);
        Assert.Matches(new Regex(@"^\s*--\s*quest:ActivateQuest\(""ToggleQuest""\)", RegexOptions.Multiline), masterLua);

        string qstText = File.ReadAllText(tempInstall.QstPath);
        Assert.DoesNotContain("AddQuest(\"ToggleQuest\"", qstText);

        Assert.True(service.ToggleQuest("ToggleQuest", true, out string enableMessage), enableMessage);
        string questsLuaEnabled = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.DoesNotMatch(new Regex(@"^\s*--\s*ToggleQuest\s*=\s*\{", RegexOptions.Multiline), questsLuaEnabled);

        string masterLuaEnabled = File.ReadAllText(tempInstall.MasterPath);
        Assert.DoesNotMatch(new Regex(@"^\s*--\s*quest:ActivateQuest\(""ToggleQuest""\)", RegexOptions.Multiline), masterLuaEnabled);

        string qstTextEnabled = File.ReadAllText(tempInstall.QstPath);
        Assert.Contains("AddQuest(\"ToggleQuest\"", qstTextEnabled);
    }

    [Fact]
    public void DeleteQuest_RemovesFilesAndRegistrations()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        CodeGenerator generator = new CodeGenerator();
        DeploymentService service = new DeploymentService(config, generator);

        QuestProject quest = new QuestProject
        {
            Name = "DeleteQuest",
            Id = 51011
        };
        quest.Entities.Add(new QuestEntity { ScriptName = "DeleteNpc" });

        Assert.True(service.DeployQuest(quest, out string deployMessage), deployMessage);

        Assert.True(service.DeleteQuest("DeleteQuest", out string deleteMessage), deleteMessage);

        string questFolder = Path.Combine(tempInstall.FseFolder, "DeleteQuest");
        Assert.False(Directory.Exists(questFolder));

        string questsLua = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.DoesNotContain("DeleteQuest", questsLua);

        string masterLua = File.ReadAllText(tempInstall.MasterPath);
        Assert.DoesNotContain("DeleteQuest", masterLua);

        string qstText = File.ReadAllText(tempInstall.QstPath);
        Assert.DoesNotContain("AddQuest(\"DeleteQuest\"", qstText);
    }
}
