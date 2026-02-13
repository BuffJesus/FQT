using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FableQuestTool.Core;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class DiagnosticsCatalogTests
{
    private static readonly Regex DiagnosticPattern = new(@"FQT-(VAL|CG|IO)-\d{3}", RegexOptions.Compiled);

    [Fact]
    public void AllDiagnosticsInSource_AreRegisteredInCatalog()
    {
        string repoRoot = TestPaths.GetRepoRoot();
        string sourceRoot = Path.Combine(repoRoot, "FableQuestTool");

        HashSet<string> discovered = new();
        foreach (string file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string normalized = file.Replace('\\', '/');
            if (normalized.Contains("/bin/") || normalized.Contains("/obj/"))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            foreach (Match match in DiagnosticPattern.Matches(text))
            {
                discovered.Add(match.Value);
            }
        }

        HashSet<string> catalog = DiagnosticCatalog.Entries.Keys.ToHashSet();
        List<string> missing = discovered.Where(code => !catalog.Contains(code)).OrderBy(code => code).ToList();

        Assert.True(missing.Count == 0,
            "Diagnostics used in source but missing from catalog: " + string.Join(", ", missing));
    }

    [Fact]
    public void CatalogEntries_UseExpectedCodeFormat()
    {
        foreach (string code in DiagnosticCatalog.Entries.Keys)
        {
            Assert.Matches(@"^FQT-(VAL|CG|IO)-\d{3}$", code);
        }
    }
}
