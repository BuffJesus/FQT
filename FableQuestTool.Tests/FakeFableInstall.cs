using System;
using System.IO;

namespace FableQuestTool.Tests;

internal sealed class FakeFableInstall : IDisposable
{
    private FakeFableInstall(string rootPath)
    {
        RootPath = rootPath;
    }

    public string RootPath { get; }
    public string FseFolder => Path.Combine(RootPath, "FSE");
    public string MasterPath => Path.Combine(FseFolder, "Master", "FSE_Master.lua");
    public string QuestsLuaPath => Path.Combine(FseFolder, "quests.lua");
    public string QstPath => Path.Combine(RootPath, "data", "Levels", "FinalAlbion.qst");

    public static FakeFableInstall Create()
    {
        string root = Path.Combine(Path.GetTempPath(), "FqtTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "data", "Levels", "FinalAlbion"));
        Directory.CreateDirectory(Path.Combine(root, "FSE", "Master"));

        File.WriteAllText(Path.Combine(root, "FSE_Launcher.exe"), string.Empty);
        File.WriteAllText(Path.Combine(root, "FableScriptExtender.dll"), string.Empty);

        File.WriteAllText(Path.Combine(root, "FSE", "quests.lua"), "Quests = {\n}\n");
        File.WriteAllText(Path.Combine(root, "FSE", "Master", "FSE_Master.lua"), "function Main(quest)\nend\n");

        string samplePath = Path.Combine(
            TestPaths.GetRepoRoot(),
            "FSE_Source",
            "SampleQuests",
            "NewQuest",
            "FinalAlbion.qst.example");

        if (File.Exists(samplePath))
        {
            File.WriteAllText(Path.Combine(root, "data", "Levels", "FinalAlbion.qst"), File.ReadAllText(samplePath));
        }
        else
        {
            File.WriteAllText(Path.Combine(root, "data", "Levels", "FinalAlbion.qst"), "AddQuest(\"FSE_Master\", TRUE);\n");
        }

        return new FakeFableInstall(root);
    }

    public void Dispose()
    {
        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, true);
        }
    }
}
