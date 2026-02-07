using System;
using System.IO;
using System.Linq;
using FableQuestTool.Formats;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class QstFileTests
{
    [Fact]
    public void Load_ParsesAddQuestEntries()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "FinalAlbion.qst");
        string contents = string.Join("\n", new[]
        {
            "AddQuest(\"QuestA\", \t\t\tTRUE);",
            "AddQuest(\"QuestB\", \t\t\tFALSE);",
            "AddQuest(\"QuestC\", \t\t\t1);",
            "AddQuest(\"QuestD\", \t\t\t0);",
            string.Empty
        });
        File.WriteAllText(path, contents);

        QstFile file = QstFile.Load(path);

        Assert.True(file.HasQuest("QuestA"));
        Assert.Contains(file.Quests, q => q.Name == "QuestB" && q.Enabled == false);
        Assert.Contains(file.Quests, q => q.Name == "QuestC" && q.Enabled == true);
        Assert.Contains(file.Quests, q => q.Name == "QuestD" && q.Enabled == false);
    }

    [Fact]
    public void AddUpdateRemove_ManipulatesEntries()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "FinalAlbion.qst");
        File.WriteAllText(path, string.Empty);

        QstFile file = QstFile.Load(path);

        Assert.True(file.AddQuestIfMissing("QuestX", true));
        Assert.False(file.AddQuestIfMissing("QuestX", false));
        Assert.True(file.UpdateQuestStatus("QuestX", false));
        Assert.False(file.UpdateQuestStatus("MissingQuest", true));
        Assert.True(file.RemoveQuest("QuestX"));
    }

    [Fact]
    public void RemoveQuest_DoesNotRemoveProtected()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "FinalAlbion.qst");
        File.WriteAllText(path, "AddQuest(\"Q_Test\", \t\t\tTRUE);\n");

        QstFile file = QstFile.Load(path);

        Assert.False(file.RemoveQuest("Q_Test"));
        Assert.True(file.HasQuest("Q_Test"));
    }

    [Fact]
    public void SyncQuests_AddsAndRemovesNonProtected()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "FinalAlbion.qst");
        string contents = string.Join("\n", new[]
        {
            "AddQuest(\"QuestA\", \t\t\tTRUE);",
            "AddQuest(\"QuestB\", \t\t\tFALSE);",
            "AddQuest(\"Q_Protected\", \t\t\tTRUE);",
            string.Empty
        });
        File.WriteAllText(path, contents);

        QstFile file = QstFile.Load(path);
        file.SyncQuests(new[] { "QuestB", "QuestC" }, out int added, out int removed);

        Assert.Equal(1, added);
        Assert.Equal(1, removed);
        Assert.True(file.HasQuest("QuestB"));
        Assert.True(file.HasQuest("QuestC"));
        Assert.True(file.HasQuest("Q_Protected"));
        Assert.False(file.HasQuest("QuestA"));
    }

    [Fact]
    public void Save_WritesUpdatedEntries()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "FinalAlbion.qst");
        File.WriteAllText(path, string.Empty);

        QstFile file = QstFile.Load(path);
        file.AddQuestIfMissing("QuestSave", true);
        file.Save();

        string output = File.ReadAllText(path);
        Assert.Contains("AddQuest(\"QuestSave\"", output);
        Assert.Contains("TRUE", output);
    }
}
