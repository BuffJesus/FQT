using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ViewModelEntityTabTests
{
    [Fact]
    public void EntityTab_AddNode_AddsNodeAndRecent()
    {
        QuestEntity entity = new QuestEntity { ScriptName = "TestEntity" };
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeOption option = viewModel.SimpleNodes.First(n =>
            n.Definition != null && !string.Equals(n.Definition.Category, "trigger", System.StringComparison.OrdinalIgnoreCase));
        viewModel.AddNodeCommand.Execute(option);

        Assert.Single(viewModel.Nodes);
        Assert.Equal(option.Type, viewModel.Nodes[0].Type);
        Assert.Contains(viewModel.RecentNodes, recent => recent.Type == option.Type);
    }

    [Fact]
    public void EntityTab_ApplyTalkTemplate_BuildsNodes()
    {
        QuestEntity entity = new QuestEntity { ScriptName = "TemplateEntity" };
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.ApplyTemplateCommand.Execute("Talk");

        Assert.Equal(3, viewModel.Nodes.Count);
        var types = viewModel.Nodes.Select(n => n.Type).ToList();
        Assert.Contains("onHeroTalks", types);
        Assert.Contains("showDialogue", types);
        Assert.Contains("completeQuest", types);
    }

    [Fact]
    public void EntityTab_ApplyRewardTemplate_ConfiguresObjectReward()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.ApplyTemplateCommand.Execute("Reward");

        Assert.Equal(EntityType.Object, entity.EntityType);
        Assert.Equal(SpawnMethod.AtMarker, entity.SpawnMethod);
        Assert.Equal("OBJECT_CHEST_SILVER", entity.DefName);
        Assert.Equal("RewardObject", entity.ScriptName);
        Assert.NotNull(entity.ObjectReward);
        Assert.True(viewModel.HasObjectReward);
        Assert.Equal(100, viewModel.ObjectRewardGold);
        Assert.Contains("OBJECT_HEALTH_POTION", viewModel.ObjectRewardItems);
    }

    [Fact]
    public void EntityTab_ObjectReward_AddsAndRemovesItems()
    {
        QuestEntity entity = new QuestEntity
        {
            EntityType = EntityType.Object,
            DefName = "OBJECT_CHEST"
        };
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        Assert.True(viewModel.IsObjectRewardSupported);

        viewModel.HasObjectReward = true;
        viewModel.NewRewardItem = "OBJECT_APPLE";
        viewModel.AddRewardItemCommand.Execute(null);

        Assert.Single(viewModel.ObjectRewardItems);
        Assert.Contains("OBJECT_APPLE", entity.ObjectReward!.Items);

        viewModel.RemoveRewardItemCommand.Execute("OBJECT_APPLE");

        Assert.Empty(viewModel.ObjectRewardItems);
        Assert.Empty(entity.ObjectReward!.Items);
    }

    [Fact]
    public void EntityTab_DuplicateNode_CopiesProperties()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeOption option = viewModel.SimpleNodes.First(n => n.Definition?.Properties?.Count > 0);
        viewModel.AddNodeCommand.Execute(option);

        NodeViewModel original = viewModel.Nodes[0];
        string propertyName = original.Definition!.Properties[0].Name;
        original.SetProperty(propertyName, "ValueA");

        viewModel.SelectedNode = original;
        viewModel.DuplicateNodeCommand.Execute(null);

        Assert.Equal(2, viewModel.Nodes.Count);
        Assert.NotSame(original, viewModel.SelectedNode);
        Assert.Equal("ValueA", viewModel.SelectedNode!.Properties[propertyName]);
    }

    [Fact]
    public void EntityTab_DeleteSelectedNodes_RemovesConnections()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel source = new NodeViewModel();
        NodeViewModel target = new NodeViewModel();
        viewModel.Nodes.Add(source);
        viewModel.Nodes.Add(target);
        viewModel.Connections.Add(new ConnectionViewModel
        {
            Source = source.Output.First(),
            Target = target.Input.First()
        });

        viewModel.SelectedNodes.Clear();
        viewModel.SelectedNodes.Add(source);
        viewModel.DeleteSelectedNodesCommand.Execute(null);

        Assert.Single(viewModel.Nodes);
        Assert.Empty(viewModel.Connections);
    }

    [Fact]
    public void EntityTab_CreateVariableNodeAtPosition_UsesExistingVariable()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.Variables.Add(new VariableDefinition
        {
            Name = "MyVar",
            Type = "String",
            DefaultValue = "Hello"
        });

        NodeViewModel? node = viewModel.CreateVariableNodeAtPosition("MyVar", isSetNode: false, new System.Windows.Point(10, 20));

        Assert.NotNull(node);
        Assert.Equal("var_get_MyVar", node!.Type);
        Assert.Single(viewModel.Nodes);
    }

    [Fact]
    public void EntityTab_CreateVariableNodeFromConnector_WiresGetterAndProperty()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel target = new NodeViewModel
        {
            Type = "setProperty",
            Category = "action",
            Definition = new FableQuestTool.Data.NodeDefinition
            {
                Type = "setProperty",
                Category = "action",
                Properties = new()
                {
                    new FableQuestTool.Data.NodeProperty { Name = "value", Type = "string", Label = "Value" }
                }
            }
        };
        target.InitializeConnectors();
        viewModel.Nodes.Add(target);

        ConnectorViewModel valueInput = target.Input.First(i => i.PropertyName == "value");
        valueInput.ConnectorType = ConnectorType.String;
        valueInput.IsInput = true;

        NodeViewModel? created = viewModel.CreateVariableNodeFromConnector(valueInput, new System.Windows.Point(30, 40));

        Assert.NotNull(created);
        Assert.Single(viewModel.Variables);
        Assert.Single(viewModel.Connections);
        Assert.Equal("$StrVar1", target.Properties["value"]);
    }
}
