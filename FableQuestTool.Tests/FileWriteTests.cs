using System;
using System.IO;
using FableQuestTool.Core;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class FileWriteTests
{
    [Fact]
    public void WriteAllTextAtomic_WritesNewFile()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "output.txt");

        FileWrite.WriteAllTextAtomic(path, "Hello");

        Assert.True(File.Exists(path));
        Assert.Equal("Hello", File.ReadAllText(path));
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void WriteAllTextAtomic_ReplacesFileAndCreatesBackup()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "output.txt");
        File.WriteAllText(path, "Old");

        FileWrite.WriteAllTextAtomic(path, "New", createBackup: true);

        Assert.Equal("New", File.ReadAllText(path));
        Assert.True(File.Exists(path + ".bak"));
        Assert.Equal("Old", File.ReadAllText(path + ".bak"));
    }

    [Fact]
    public void WriteAllTextAtomic_ReplacesFileWithoutBackup()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "output.txt");
        File.WriteAllText(path, "Old");

        FileWrite.WriteAllTextAtomic(path, "New", createBackup: false);

        Assert.Equal("New", File.ReadAllText(path));
        Assert.False(File.Exists(path + ".bak"));
    }

    [Fact]
    public void WriteAllTextAtomic_RequiresPath()
    {
        Assert.Throws<ArgumentException>(() => FileWrite.WriteAllTextAtomic("", "data"));
    }
}
