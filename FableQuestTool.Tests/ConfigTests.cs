using System;
using System.IO;
using FableQuestTool.Config;
using FableQuestTool.Core;
using Xunit;

namespace FableQuestTool.Tests;

[Collection("IniFileTests")]
public sealed class ConfigTests
{
    [Fact]
    public void IniFile_SaveAndLoadRoundTrip()
    {
        using TestTempDirectory temp = new TestTempDirectory();
        string path = Path.Combine(temp.Path, "settings.ini");

        IniFile ini = IniFile.CreateEmpty();
        ini.Set("Settings", "FablePath", "C:\\Fable");
        ini.Set("UI", "FavoriteNodes", "nodeA|nodeB");
        ini.Save(path);

        IniFile loaded = IniFile.Load(path);

        Assert.Equal("C:\\Fable", loaded.Get("Settings", "FablePath"));
        Assert.Equal("nodeA|nodeB", loaded.Get("UI", "FavoriteNodes"));
        Assert.NotNull(loaded.GetSection("Settings"));
    }

    [Fact]
    public void FableConfig_LoadsIniAndNormalizesPath()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        string exePath = Path.Combine(tempInstall.RootPath, "Fable.exe");
        File.WriteAllText(exePath, string.Empty);

        string contents = $"[Settings]{Environment.NewLine}FablePath = \"{exePath}\"{Environment.NewLine}";
        using IniScope ini = IniScope.WithContents(contents);

        FableConfig config = FableConfig.Load();

        Assert.Equal(tempInstall.RootPath, config.FablePath);
        Assert.Equal(Path.Combine(tempInstall.RootPath, "FSE"), config.GetFseFolder());
        Assert.Equal(Path.Combine(tempInstall.RootPath, "FSE_Launcher.exe"), config.GetFseLauncherPath());
    }

    [Fact]
    public void FableConfig_PersistsFavoritesAndStartupImage()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        string contents = $"[Settings]{Environment.NewLine}FablePath = {tempInstall.RootPath}{Environment.NewLine}";
        using IniScope ini = IniScope.WithContents(contents);

        FableConfig config = FableConfig.Load();
        config.SetFavoriteNodeTypes(new[] { "nodeA", "nodeB" });
        config.SetShowStartupImage(false);
        config.Save();

        FableConfig reloaded = FableConfig.Load();

        Assert.Equal(new[] { "nodeA", "nodeB" }, reloaded.GetFavoriteNodeTypes());
        Assert.False(reloaded.GetShowStartupImage());
    }

    private sealed class IniScope : IDisposable
    {
        private readonly string iniPath;
        private readonly string? originalContents;

        private IniScope(string iniPath, string? originalContents)
        {
            this.iniPath = iniPath;
            this.originalContents = originalContents;
        }

        public static IniScope WithContents(string contents)
        {
            string iniPath = Path.Combine(AppContext.BaseDirectory, "FableQuestTool.ini");
            string? original = File.Exists(iniPath) ? File.ReadAllText(iniPath) : null;

            File.WriteAllText(iniPath, contents);

            return new IniScope(iniPath, original);
        }

        public void Dispose()
        {
            if (originalContents == null)
            {
                if (File.Exists(iniPath))
                {
                    File.Delete(iniPath);
                }
                return;
            }

            File.WriteAllText(iniPath, originalContents);
        }
    }
}
