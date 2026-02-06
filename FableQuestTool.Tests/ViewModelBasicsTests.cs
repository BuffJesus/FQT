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

    [Fact]
    public void VariableDefinition_TypeColorAndIconMatchType()
    {
        VariableDefinition variable = new VariableDefinition
        {
            Type = "String"
        };

        Assert.Equal("#F0A1D4", variable.TypeColor);
        Assert.Equal("T", variable.TypeIcon);

        variable.Type = "Object";

        Assert.Equal("#0099DB", variable.TypeColor);
        Assert.Equal("O", variable.TypeIcon);
    }

    [Fact]
    public void NodeOption_StoresInitMetadata()
    {
        NodeOption option = new NodeOption("Label", "action", "icon", "desc")
        {
            Type = "showMessage",
            MenuIndex = 2
        };

        Assert.Equal("Label", option.Label);
        Assert.Equal("action", option.Category);
        Assert.Equal("icon", option.Icon);
        Assert.Equal("desc", option.Description);
        Assert.Equal("showMessage", option.Type);
        Assert.Equal(2, option.MenuIndex);
    }

    [Fact]
    public void NodeCategoryGroup_DerivesCategoryFromNodes()
    {
        NodeOption option = new NodeOption("Label", "trigger", "icon");
        NodeCategoryGroup group = new NodeCategoryGroup("Triggers", new() { option });

        Assert.Equal("Triggers", group.Name);
        Assert.Equal("trigger", group.Category);
        Assert.Equal("#27AE60", group.CategoryColor);
        Assert.NotEmpty(group.Nodes);
    }

    [Fact]
    public void ExternalVariableInfo_StoresDetails()
    {
        ExternalVariableInfo info = new ExternalVariableInfo("EntityA", "VarB", "String", "Default");

        Assert.Equal("EntityA", info.EntityScriptName);
        Assert.Equal("VarB", info.VariableName);
        Assert.Equal("String", info.VariableType);
        Assert.Equal("Default", info.DefaultValue);
    }
}
