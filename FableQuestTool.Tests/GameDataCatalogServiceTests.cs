using System.IO;
using System.Linq;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class GameDataCatalogServiceTests
{
    [Fact]
    public void BuildCompleteCatalog_UsesLooseTngFiles()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string levelsPath = Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion");
        File.WriteAllText(Path.Combine(levelsPath, "OakValeEast_v2.tng"),
            "NewThing Object;\nScriptName MK_TEST;\nDefinitionType OBJECT_CHEST_WOOD;\nUID 1;\nEndThing\n");

        GameDataCatalogService service = new GameDataCatalogService(config);
        var entities = service.BuildCompleteCatalog(forceRefresh: true);

        Assert.NotEmpty(entities);
        var entity = entities.FirstOrDefault(e => e.ScriptName == "MK_TEST");
        Assert.NotNull(entity);
        Assert.Equal("Oakvale", entity!.RegionName);
        Assert.StartsWith("[LOOSE]", entity.SourceFile);
    }

    [Fact]
    public void GetFilteredEntities_FiltersByCategoryAndScriptable()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string levelsPath = Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion");
        File.WriteAllText(Path.Combine(levelsPath, "StartOakValeWest.tng"),
            "NewThing Object;\nScriptName MK_CHEST;\nDefinitionType OBJECT_CHEST_WOOD;\nUID 1;\nEndThing\n");

        GameDataCatalogService service = new GameDataCatalogService(config);
        var chests = service.GetFilteredEntities(EntityCategory.Chest, "Oakvale", scriptableOnly: true);

        Assert.Single(chests);
        Assert.Equal("MK_CHEST", chests[0].ScriptName);
        Assert.Equal(EntityCategory.Chest, chests[0].Category);
    }

    [Fact]
    public void ExportCatalogToFile_WritesSummary()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string levelsPath = Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion");
        File.WriteAllText(Path.Combine(levelsPath, "OakValeEast_v2.tng"),
            "NewThing Object;\nScriptName MK_TEST;\nDefinitionType OBJECT_BARREL;\nUID 1;\nEndThing\n");

        GameDataCatalogService service = new GameDataCatalogService(config);
        string outputPath = Path.Combine(tempInstall.RootPath, "catalog.txt");

        service.ExportCatalogToFile(outputPath);

        string output = File.ReadAllText(outputPath);
        Assert.Contains("STATISTICS", output);
        Assert.Contains("Total Entities:", output);
        Assert.Contains("Oakvale", output);
        Assert.Contains("MK_TEST", output);
    }
}
