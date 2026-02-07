using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FableQuestTool.Data;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class DataMappingTests
{
    [Fact]
    public void RegionTngMapping_ReturnsTngFilesWhenPresent()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string levelsPath = Path.Combine(temp.Path, "data", "Levels", "FinalAlbion");
        Directory.CreateDirectory(levelsPath);
        string tngPath = Path.Combine(levelsPath, "OakValeEast_v2.tng");
        File.WriteAllText(tngPath, string.Empty);

        List<string> files = RegionTngMapping.GetTngFilesForRegion("Oakvale", temp.Path);

        Assert.Contains(tngPath, files);
    }

    [Fact]
    public void RegionTngMapping_GetAllRegionNames_Sorted()
    {
        var names = RegionTngMapping.GetAllRegionNames();

        Assert.NotEmpty(names);
        var sorted = new List<string>(names.OrderBy(name => name));
        Assert.Equal(sorted, names);
    }

    [Fact]
    public void RegionTngMapping_InferRegionFromName()
    {
        Assert.Equal("Oakvale", RegionTngMapping.InferRegionFromTngFileName("OakValeEast_v2.tng"));
        Assert.Equal("Bowerstone_North", RegionTngMapping.InferRegionFromTngFileName("Bowerstone_North.tng"));
        Assert.Equal("HeroesGuild", RegionTngMapping.InferRegionFromTngFileName("GuildInt.tng"));
        Assert.Equal("LookoutPoint", RegionTngMapping.InferRegionFromTngFileName("StartLookoutPoint_3.tng"));
    }

    [Fact]
    public void RegionTngMapping_GetAllTngFiles_EmptyWhenMissing()
    {
        using TestTempDirectory temp = new TestTempDirectory();

        List<string> files = RegionTngMapping.GetAllTngFiles(temp.Path);

        Assert.Empty(files);
    }

    [Fact]
    public void RegionTngMapping_HasRegionMapping_ReturnsFalseForUnknown()
    {
        Assert.False(RegionTngMapping.HasRegionMapping("UnknownRegion"));
    }

    [Fact]
    public void RegionTngMapping_GetTngFilesForRegion_ReturnsEmptyWhenMissing()
    {
        using TestTempDirectory temp = new TestTempDirectory();

        List<string> files = RegionTngMapping.GetTngFilesForRegion("Oakvale", temp.Path);

        Assert.Empty(files);
    }

    [Fact]
    public void GameData_AbilityParsing_Works()
    {
        Assert.Equal(1, GameData.ParseAbilityId("1 - Blade"));
        Assert.Equal(10, GameData.ParseAbilityId("10"));
        Assert.Null(GameData.ParseAbilityId("NotAnAbility"));
        Assert.Equal("Blade", GameData.GetAbilityName(1));
        Assert.Equal("Ability 999", GameData.GetAbilityName(999));
    }
}
