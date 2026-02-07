using System.IO;
using System.Linq;
using FableQuestTool.Config;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class EntityBrowserServiceTests
{
    [Fact]
    public void GetDefinitions_ReturnExpectedCategories()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string levelsPath = Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion");
        File.WriteAllText(Path.Combine(levelsPath, "StartOakValeWest.tng"), string.Join("\n", new[]
        {
            "NewThing Object;",
            "ScriptName MK_BARREL;",
            "DefinitionType OBJECT_BARREL;",
            "UID 1;",
            "EndThing",
            "NewThing Object;",
            "ScriptName MK_CHEST;",
            "DefinitionType OBJECT_CHEST_WOOD;",
            "UID 2;",
            "EndThing",
            "NewThing Object;",
            "ScriptName MK_DOOR;",
            "DefinitionType OBJECT_DOOR_WOOD;",
            "UID 3;",
            "EndThing",
            "NewThing Object;",
            "ScriptName MK_QUEST_ITEM;",
            "DefinitionType OBJECT_QUEST_CARD_GENERIC;",
            "UID 4;",
            "EndThing",
            "NewThing AICreature;",
            "ScriptName MK_BANDIT;",
            "DefinitionType CREATURE_BANDIT;",
            "UID 5;",
            "EndThing",
            "NewThing AICreature;",
            "ScriptName MK_VILLAGER;",
            "DefinitionType CREATURE_VILLAGER_FARMER;",
            "UID 6;",
            "EndThing",
            string.Empty
        }));

        EntityBrowserService service = new EntityBrowserService(config);

        var objects = service.GetAllObjectDefinitions();
        var chests = service.GetAllChestDefinitions();
        var creatures = service.GetAllCreatureDefinitions();

        Assert.Contains("OBJECT_BARREL", objects);
        Assert.Contains("OBJECT_CHEST_WOOD", objects);
        Assert.Contains("OBJECT_DOOR_WOOD", objects);
        Assert.Contains("OBJECT_QUEST_CARD_GENERIC", objects);
        Assert.Contains("OBJECT_CHEST_WOOD", chests);
        Assert.Contains("CREATURE_BANDIT", creatures);
        Assert.DoesNotContain("CREATURE_VILLAGER_FARMER", creatures);
    }

    [Fact]
    public void FindEntityByScriptName_IgnoresCase()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string levelsPath = Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion");
        File.WriteAllText(Path.Combine(levelsPath, "OakValeEast_v2.tng"), string.Join("\n", new[]
        {
            "NewThing Object;",
            "ScriptName MK_TEST;",
            "DefinitionType OBJECT_BARREL;",
            "UID 1;",
            "EndThing",
            string.Empty
        }));

        EntityBrowserService service = new EntityBrowserService(config);

        var entity = service.FindEntityByScriptName("mk_test");

        Assert.NotNull(entity);
        Assert.Equal("MK_TEST", entity!.ScriptName);
    }
}
