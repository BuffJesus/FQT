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

    [Fact]
    public void ToggleQuest_MissingFiles_StillReturnsMessage()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        File.Delete(tempInstall.QuestsLuaPath);
        File.Delete(tempInstall.MasterPath);
        File.Delete(tempInstall.QstPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());

        Assert.True(service.ToggleQuest("MissingQuest", false, out string message));
        Assert.Contains("quests.lua updated: Not found", message, System.StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FinalAlbion.qst updated: Not found", message, System.StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FSE_Master.lua updated: Not found", message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToggleQuest_CommentsAndUncommentsQuestBlock()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string questsLua = string.Join("\n", new[]
        {
            "Quests = {",
            "    ToggleQuest = {",
            "        name = \"ToggleQuest\",",
            "        file = \"ToggleQuest/ToggleQuest\",",
            "        id = 50001,",
            "        entity_scripts = {",
            "            { name = \"Npc\", file = \"ToggleQuest/Entities/Npc\", id = 50002 },",
            "        }",
            "    },",
            "}",
            string.Empty
        });
        File.WriteAllText(tempInstall.QuestsLuaPath, questsLua);
        File.WriteAllText(tempInstall.MasterPath, string.Join("\n", new[]
        {
            "function Main(quest)",
            "    quest:ActivateQuest(\"ToggleQuest\")",
            "    quest:Log(\"FSE_Master: Activated ToggleQuest\")",
            "end",
            string.Empty
        }));

        DeploymentService service = new DeploymentService(config, new CodeGenerator());

        Assert.True(service.ToggleQuest("ToggleQuest", false, out string disableMessage), disableMessage);
        string disabledLua = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.Matches(new Regex(@"^\s*--\s*ToggleQuest\s*=\s*\{", RegexOptions.Multiline), disabledLua);
        Assert.Matches(new Regex(@"^\s*--\s*name\s*=", RegexOptions.Multiline), disabledLua);

        string disabledMaster = File.ReadAllText(tempInstall.MasterPath);
        Assert.Matches(new Regex(@"^\s*--\s*quest:ActivateQuest\(""ToggleQuest""\)", RegexOptions.Multiline), disabledMaster);
        Assert.Matches(new Regex(@"^\s*--\s*quest:Log\(""FSE_Master: Activated ToggleQuest""\)", RegexOptions.Multiline), disabledMaster);

        Assert.True(service.ToggleQuest("ToggleQuest", true, out string enableMessage), enableMessage);
        string enabledLua = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.DoesNotMatch(new Regex(@"^\s*--\s*ToggleQuest\s*=\s*\{", RegexOptions.Multiline), enabledLua);

        string enabledMaster = File.ReadAllText(tempInstall.MasterPath);
        Assert.DoesNotMatch(new Regex(@"^\s*--\s*quest:ActivateQuest\(""ToggleQuest""\)", RegexOptions.Multiline), enabledMaster);
        Assert.DoesNotMatch(new Regex(@"^\s*--\s*quest:Log\(""FSE_Master: Activated ToggleQuest""\)", RegexOptions.Multiline), enabledMaster);
    }

    [Fact]
    public void ToggleQuest_LeavesOtherEntriesUntouched()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string questsLua = string.Join("\n", new[]
        {
            "Quests = {",
            "    ToggleQuest = {",
            "        name = \"ToggleQuest\",",
            "        id = 50001,",
            "    },",
            "    OtherQuest = {",
            "        name = \"OtherQuest\",",
            "        id = 50002,",
            "    },",
            "}",
            string.Empty
        });
        File.WriteAllText(tempInstall.QuestsLuaPath, questsLua);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());

        Assert.True(service.ToggleQuest("ToggleQuest", false, out string message), message);
        string content = File.ReadAllText(tempInstall.QuestsLuaPath);
        Assert.Matches(new Regex(@"^\s*--\s*ToggleQuest\s*=\s*\{", RegexOptions.Multiline), content);
        Assert.Matches(new Regex(@"^\s*OtherQuest\s*=\s*\{", RegexOptions.Multiline), content);
    }

    [Fact]
    public void DeleteQuest_ReturnsNotFoundWhenMissing()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        DeploymentService service = new DeploymentService(config, new CodeGenerator());

        Assert.False(service.DeleteQuest("MissingQuest", out string message));
        Assert.Contains("[FQT-IO-024]", message, System.StringComparison.Ordinal);
        Assert.Contains("not found", message, System.StringComparison.OrdinalIgnoreCase);
    }
}
