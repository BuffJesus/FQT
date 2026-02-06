using System.Collections.ObjectModel;
using System.Windows;
using FableQuestTool.Data;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ViewModelBasicsTests
{
    [Fact]
    public void NodeViewModel_SetProperty_UpdatesEventTitle()
    {
        NodeViewModel node = new NodeViewModel
        {
            Type = "defineEvent",
            Category = "custom"
        };

        node.SetProperty("eventName", "MyEvent");

        Assert.Equal("Define Event: MyEvent", node.Title);
    }

    [Fact]
    public void NodeViewModel_InitializeConnectors_AddsBranchOutputs()
    {
        NodeViewModel node = new NodeViewModel
        {
            Type = "ifTest",
            Category = "condition",
            Definition = new NodeDefinition
            {
                HasBranching = true,
                Properties = new()
            }
        };

        node.InitializeConnectors();

        Assert.Single(node.Input);
        Assert.Equal(2, node.Output.Count);
    }

    [Fact]
    public void ConnectorViewModel_ConnectorColor_TracksType()
    {
        ConnectorViewModel connector = new ConnectorViewModel
        {
            ConnectorType = ConnectorType.String
        };

        Assert.Equal("#F0A1D4", connector.ConnectorColor);
    }

    [Fact]
    public void ConnectionViewModel_SourceUpdatesFlags()
    {
        ConnectionViewModel connection = new ConnectionViewModel();
        ConnectorViewModel intSource = new ConnectorViewModel { ConnectorType = ConnectorType.Integer };

        connection.Source = intSource;

        Assert.Equal(intSource.ConnectorColor, connection.WireColor);
        Assert.False(connection.IsExecConnection);

        connection.Source = new ConnectorViewModel { ConnectorType = ConnectorType.Exec };

        Assert.True(connection.IsExecConnection);
    }

    [Fact]
    public void PendingConnectionViewModel_StoresSourceAndTarget()
    {
        PendingConnectionViewModel pending = new PendingConnectionViewModel();
        ConnectorViewModel source = new ConnectorViewModel { Title = "Exec" };

        pending.Source = source;
        pending.Target = new Point(5, 8);

        Assert.Same(source, pending.Source);
        Assert.Equal(new Point(5, 8), pending.Target);
    }

    [Fact]
    public void LuaPreviewViewModel_ExposesItems()
    {
        ObservableCollection<LuaPreviewItem> items = new()
        {
            new LuaPreviewItem("Quest", "print('hi')")
        };

        LuaPreviewViewModel viewModel = new LuaPreviewViewModel(items);

        Assert.Same(items, viewModel.Items);
        Assert.Equal("Quest", viewModel.Items[0].Title);
        Assert.Equal("print('hi')", viewModel.Items[0].Content);
    }

    [Fact]
    public void QuestConfigViewModel_ExposesSuggestCommand()
    {
        QuestConfigViewModel viewModel = new QuestConfigViewModel();

        Assert.NotNull(viewModel.SuggestQuestIdCommand);
    }

    [Fact]
    public void MainViewModel_NewProject_ResetsState()
    {
        MainViewModel viewModel = new MainViewModel
        {
            IsModified = false
        };

        viewModel.Project.Name = "OldQuest";
        viewModel.NewProjectCommand.Execute(null);

        Assert.Null(viewModel.ProjectPath);
        Assert.False(viewModel.IsModified);
        Assert.Equal("New project created.", viewModel.StatusText);
    }

    [Fact]
    public void MainViewModel_ProjectChangeMarksModified()
    {
        MainViewModel viewModel = new MainViewModel
        {
            IsModified = false
        };

        viewModel.Project = new QuestProject();
        viewModel.IsModified = false;
        viewModel.Project.Name = "ChangedQuest";

        Assert.True(viewModel.IsModified);
    }
}
