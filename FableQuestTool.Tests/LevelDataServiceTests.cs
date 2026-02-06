using System.IO;
using FableQuestTool.Config;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class LevelDataServiceTests
{
    [Fact]
    public void GetBigFiles_NoDataFolder_ReturnsEmpty()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(temp.Path);

        LevelDataService service = new LevelDataService(config);
        var bigFiles = service.GetBigFiles();

        Assert.Empty(bigFiles);
    }

    [Fact]
    public void GetLevelMetadata_FindsLevFilesAndMappedTngs()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        FableConfig config = FableConfig.Load();
        config.SetFablePath(tempInstall.RootPath);

        string levelsPath = Path.Combine(tempInstall.RootPath, "data", "Levels", "FinalAlbion");
        File.WriteAllText(Path.Combine(levelsPath, "StartOakValeWest.lev"), string.Empty);
        File.WriteAllText(Path.Combine(levelsPath, "StartOakValeWest.tng"), "NewThing Object;\nEndThing\n");

        LevelDataService service = new LevelDataService(config);
        var metadata = service.GetLevelMetadata();

        Assert.True(metadata.ContainsKey("StartOakValeWest"));
        var info = metadata["StartOakValeWest"];
        Assert.True(info.HasLevFile);
        Assert.Single(info.TngFiles);
        Assert.Contains("StartOakValeWest.tng", info.TngFiles[0]);
    }
}
