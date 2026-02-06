using System;
using System.IO;

namespace FableQuestTool.Tests;

internal static class TestPaths
{
    public static string GetRepoRoot()
    {
        string baseDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }

    public static string GetFixturePath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), "FableQuestTool.Tests", "Fixtures", relativePath);
    }
}
