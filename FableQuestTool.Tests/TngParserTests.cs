using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class TngParserTests
{
    [Fact]
    public void ParseTngFile_ReadsEntitiesAndProperties()
    {
        string tngPath = TestPaths.GetFixturePath("TestRegion.tng");

        var entities = TngParser.ParseTngFile(tngPath, "TEST_REGION");

        Assert.Equal(2, entities.Count);
        var chest = entities[0];
        Assert.Equal("Object", chest.ThingType);
        Assert.Equal("MK_TEST_CHEST", chest.ScriptName);
        Assert.Equal("OBJECT_CHEST_WOOD", chest.DefinitionType);
        Assert.Equal("TEST_REGION", chest.RegionName);
        Assert.True(chest.IsGamePersistent);
        Assert.False(chest.IsLevelPersistent);
        Assert.Equal(EntityCategory.Chest, chest.Category);
    }

    [Fact]
    public void ParseTngFile_FiltersByCategoryAndScriptable()
    {
        string tngPath = TestPaths.GetFixturePath("TestRegion.tng");

        var entities = TngParser.ParseTngFile(tngPath, "TEST_REGION");
        var npcs = TngParser.FilterByCategory(entities, EntityCategory.NPC);
        var scriptable = TngParser.FilterScriptableOnly(entities);

        Assert.Single(npcs);
        Assert.Equal("TEST_VILLAGER", npcs[0].ScriptName);
        Assert.Equal(2, scriptable.Count);
        Assert.True(scriptable.All(e => e.HasScriptName));
    }

    [Fact]
    public void GenerateLuaSnippet_HandlesMissingScriptName()
    {
        TngEntity entity = new TngEntity
        {
            ScriptName = string.Empty
        };

        string snippet = TngParser.GenerateLuaSnippet(entity);

        Assert.Contains("no ScriptName", snippet, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLuaSnippet_IncludesScriptName()
    {
        TngEntity entity = new TngEntity
        {
            ScriptName = "MK_TEST",
            DefinitionType = "OBJECT_BARREL",
            RegionName = "Oakvale"
        };

        string snippet = TngParser.GenerateLuaSnippet(entity);

        Assert.Contains("MK_TEST", snippet);
        Assert.Contains("Quest:GetThingWithScriptName", snippet);
    }

    [Fact]
    public void FilterBySearch_MatchesScriptNameOrDefinition()
    {
        var entities = new[]
        {
            new TngEntity { ScriptName = "MK_BARREL", DefinitionType = "OBJECT_BARREL" },
            new TngEntity { ScriptName = "MK_CHEST", DefinitionType = "OBJECT_CHEST_WOOD" }
        }.ToList();

        var resultByScript = TngParser.FilterBySearch(entities, "barrel");
        var resultByDef = TngParser.FilterBySearch(entities, "chest");

        Assert.Single(resultByScript);
        Assert.Equal("MK_BARREL", resultByScript[0].ScriptName);
        Assert.Single(resultByDef);
        Assert.Equal("MK_CHEST", resultByDef[0].ScriptName);
    }
}
