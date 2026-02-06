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
}
