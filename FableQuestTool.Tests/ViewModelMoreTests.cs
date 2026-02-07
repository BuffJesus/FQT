using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using Xunit;

namespace FableQuestTool.Tests;

[Collection("IniFileTests")]
public sealed class ViewModelMoreTests
{
    [Fact]
    public void QuestManager_LoadsQuestsFromLua()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        string questsLua = string.Join("\n", new[]
        {
            "Quests = {",
            "    TestQuest = {",
            "        name = \"TestQuest\",",
            "        file = \"TestQuest/TestQuest\",",
            "        id = 50001,",
            "    },",
            "    -- DisabledQuest = {",
            "    --     name = \"DisabledQuest\",",
            "    --     file = \"DisabledQuest/DisabledQuest\",",
            "    --     id = 50002,",
            "    -- },",
            "}",
            string.Empty
        });

        Directory.CreateDirectory(Path.Combine(tempInstall.FseFolder, "TestQuest"));
        File.WriteAllText(tempInstall.QuestsLuaPath, questsLua);

        using IniScope ini = IniScope.WithFablePath(tempInstall.RootPath);
        QuestManagerViewModel viewModel = new QuestManagerViewModel();

        Assert.Equal(2, viewModel.Quests.Count);
        var enabled = viewModel.Quests.First(q => q.Name == "TestQuest");
        var disabled = viewModel.Quests.First(q => q.Name == "DisabledQuest");

        Assert.True(enabled.IsEnabled);
        Assert.False(disabled.IsEnabled);
        Assert.True(enabled.FolderExists);
        Assert.False(disabled.FolderExists);
    }

    [Fact]
    public void EntityBrowser_FiltersEntitiesAndItems()
    {
        EntityBrowserViewModel viewModel = new EntityBrowserViewModel();

        var entities = new ObservableCollection<TngEntity>
        {
            new TngEntity { ScriptName = "NPC1", DefinitionType = "CREATURE_VILLAGER_FARMER" },
            new TngEntity { ScriptName = string.Empty, DefinitionType = "OBJECT_CHEST_WOOD" }
        };
        viewModel.Entities = entities;

        viewModel.ApplyFiltersCommand.Execute(null);
        Assert.Single(viewModel.FilteredEntities);
        Assert.Equal("NPC1", viewModel.FilteredEntities[0].ScriptName);

        viewModel.ScriptableOnly = false;
        viewModel.SelectedCategory = EntityCategory.Chest;
        viewModel.ApplyFiltersCommand.Execute(null);
        Assert.Single(viewModel.FilteredEntities);
        Assert.Equal(EntityCategory.Chest, viewModel.FilteredEntities[0].Category);

        viewModel.ItemSearchText = "apple";
        Assert.Contains(viewModel.FilteredItems, i => i.Equals("OBJECT_APPLE", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TemplatesViewModel_FiltersByCategory()
    {
        TemplatesViewModel viewModel = new TemplatesViewModel();
        Assert.NotEmpty(viewModel.FilteredTemplates);

        string category = viewModel.FilteredTemplates[0].Category;
        viewModel.SelectedCategory = category;

        Assert.All(viewModel.FilteredTemplates, t => Assert.Equal(category, t.Category));
    }

    [Fact]
    public void QuestManager_ParsesCommentedQuestAndFilePath()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        string questsLua = string.Join("\n", new[]
        {
            "Quests = {",
            "    ActiveQuest = {",
            "        name = \"ActiveQuest\",",
            "        file = \"ActiveQuest/ActiveQuest\",",
            "        id = 51000,",
            "    },",
            "    -- DisabledQuest = {",
            "    --     name = \"DisabledQuest\",",
            "    --     file = \"DisabledQuest/DisabledQuest\",",
            "    --     id = 51001,",
            "    -- },",
            "}",
            string.Empty
        });
        File.WriteAllText(tempInstall.QuestsLuaPath, questsLua);

        using IniScope ini = IniScope.WithFablePath(tempInstall.RootPath);
        QuestManagerViewModel viewModel = new QuestManagerViewModel();

        var active = viewModel.Quests.First(q => q.Name == "ActiveQuest");
        var disabled = viewModel.Quests.First(q => q.Name == "DisabledQuest");

        Assert.True(active.IsEnabled);
        Assert.False(disabled.IsEnabled);
        Assert.Equal("ActiveQuest/ActiveQuest", active.FilePath);
        Assert.Equal("DisabledQuest/DisabledQuest", disabled.FilePath);
    }

    [Fact]
    public void QuestManager_ReportsMissingQuestsLua()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        File.Delete(tempInstall.QuestsLuaPath);

        using IniScope ini = IniScope.WithFablePath(tempInstall.RootPath);
        QuestManagerViewModel viewModel = new QuestManagerViewModel();

        Assert.Empty(viewModel.Quests);
        Assert.Equal("quests.lua not found", viewModel.StatusText);
    }

    [Fact]
    public void QuestManager_ReportsMissingFseFolder()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        Directory.Delete(tempInstall.FseFolder, true);

        using IniScope ini = IniScope.WithFablePath(tempInstall.RootPath);
        QuestManagerViewModel viewModel = new QuestManagerViewModel();

        Assert.Empty(viewModel.Quests);
        Assert.Equal("quests.lua not found", viewModel.StatusText);
    }

    [Fact]
    public void QuestManager_ReportsParseError()
    {
        using FakeFableInstall tempInstall = FakeFableInstall.Create();
        File.WriteAllText(tempInstall.QuestsLuaPath, "\0bad");

        using IniScope ini = IniScope.WithFablePath(tempInstall.RootPath);
        QuestManagerViewModel viewModel = new QuestManagerViewModel();

        Assert.Empty(viewModel.Quests);
        Assert.Equal("Loaded 0 quests", viewModel.StatusText);
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

        public static IniScope WithFablePath(string fablePath)
        {
            string iniPath = Path.Combine(AppContext.BaseDirectory, "FableQuestTool.ini");
            string? original = File.Exists(iniPath) ? File.ReadAllText(iniPath) : null;

            string contents = $"[Settings]{Environment.NewLine}FablePath = {fablePath}{Environment.NewLine}";
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
