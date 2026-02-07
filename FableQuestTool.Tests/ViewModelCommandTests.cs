using System;
using System.IO;
using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using Xunit;

namespace FableQuestTool.Tests;

[Collection("IniFileTests")]
public sealed class ViewModelCommandTests
{
    [Fact]
    public void EntityEditor_AddNewEntity_UpdatesSelectionAndProject()
    {
        MainViewModel main = new MainViewModel();
        main.Project = new QuestProject();
        main.IsModified = false;

        EntityEditorViewModel editor = new EntityEditorViewModel(main);

        editor.AddNewEntityCommand.Execute(null);

        Assert.Single(main.Project.Entities);
        Assert.Single(editor.EntityTabs);
        Assert.NotNull(editor.SelectedTab);
        Assert.Equal(0, editor.SelectedTabIndex);
        Assert.True(main.IsModified);
        Assert.Equal("New entity added.", main.StatusText);
    }

    [Fact]
    public void EntityEditor_RemoveEntity_RemovesTabAndKeepsSelectionValid()
    {
        MainViewModel main = new MainViewModel();
        main.Project = new QuestProject();
        main.Project.Entities.Add(new QuestEntity { ScriptName = "EntityA" });
        main.Project.Entities.Add(new QuestEntity { ScriptName = "EntityB" });

        EntityEditorViewModel editor = new EntityEditorViewModel(main);
        EntityTabViewModel? firstTab = editor.EntityTabs.FirstOrDefault();

        editor.RemoveEntityCommand.Execute(firstTab);

        Assert.Single(main.Project.Entities);
        Assert.Single(editor.EntityTabs);
        Assert.NotNull(editor.SelectedTab);
        Assert.Equal(0, editor.SelectedTabIndex);
        Assert.True(main.IsModified);
        Assert.Equal("Entity removed.", main.StatusText);
    }

    [Fact]
    public void EntityEditor_DuplicateEntity_CreatesCopy()
    {
        MainViewModel main = new MainViewModel();
        main.Project = new QuestProject();
        QuestEntity entity = new QuestEntity { ScriptName = "Original" };
        entity.Nodes.Add(new BehaviorNode { Id = "node1", Type = "showMessage", Category = "action" });
        main.Project.Entities.Add(entity);

        EntityEditorViewModel editor = new EntityEditorViewModel(main);
        EntityTabViewModel? tab = editor.EntityTabs.FirstOrDefault();

        editor.DuplicateEntityCommand.Execute(tab);

        Assert.Equal(2, main.Project.Entities.Count);
        Assert.Equal(2, editor.EntityTabs.Count);
        Assert.NotNull(editor.SelectedTab);
        Assert.EndsWith("_Copy", editor.SelectedTab!.Entity.ScriptName, StringComparison.Ordinal);
        Assert.Equal("Entity duplicated.", main.StatusText);
    }

    [Fact]
    public void ApiReference_Filtering_UpdatesList()
    {
        string originalCwd = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(TestPaths.GetRepoRoot());
            ApiReferenceViewModel viewModel = new ApiReferenceViewModel();

            Assert.True(viewModel.HasApiData);

            viewModel.SelectedCategory = "Quest API";
            Assert.All(viewModel.FilteredFunctions, f => Assert.Equal("Quest API", f.Category));

            viewModel.SearchText = "ActivateQuest";
            Assert.Contains(viewModel.FilteredFunctions, f => f.Name == "ActivateQuest");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCwd);
        }
    }
}
