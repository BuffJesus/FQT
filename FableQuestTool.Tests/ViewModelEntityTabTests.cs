using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using Xunit;

namespace FableQuestTool.Tests;

[Collection("IniFileTests")]
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

    [Fact]
    public void EntityTab_AddAndRemoveOutputPins_UpdatesConnections()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel flowNode = new NodeViewModel
        {
            Category = "flow"
        };
        viewModel.Nodes.Add(flowNode);
        viewModel.SelectedNode = flowNode;

        int initialOutputs = flowNode.Output.Count;
        viewModel.AddOutputPinCommand.Execute(null);

        Assert.Equal(initialOutputs + 1, flowNode.Output.Count);

        ConnectorViewModel lastOutput = flowNode.Output.Last();
        NodeViewModel target = new NodeViewModel();
        viewModel.Nodes.Add(target);
        viewModel.Connections.Add(new ConnectionViewModel
        {
            Source = lastOutput,
            Target = target.Input.First()
        });

        viewModel.RemoveOutputPinCommand.Execute(null);

        Assert.Equal(initialOutputs, flowNode.Output.Count);
        Assert.Empty(viewModel.Connections);
    }

    [Fact]
    public void EntityTab_BreakConnection_RemovesWire()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel source = new NodeViewModel();
        NodeViewModel target = new NodeViewModel();
        viewModel.Nodes.Add(source);
        viewModel.Nodes.Add(target);

        ConnectionViewModel connection = new ConnectionViewModel
        {
            Source = source.Output.First(),
            Target = target.Input.First()
        };
        viewModel.Connections.Add(connection);

        viewModel.BreakConnectionCommand.Execute(connection);

        Assert.Empty(viewModel.Connections);
    }

    [Fact]
    public void EntityTab_CreateRerouteOnConnection_ReplacesConnection()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel source = new NodeViewModel();
        NodeViewModel target = new NodeViewModel();
        viewModel.Nodes.Add(source);
        viewModel.Nodes.Add(target);

        ConnectionViewModel connection = new ConnectionViewModel
        {
            Source = source.Output.First(),
            Target = target.Input.First()
        };
        viewModel.Connections.Add(connection);

        viewModel.CreateRerouteOnConnectionCommand.Execute((connection, new System.Windows.Point(50, 60)));

        Assert.Equal(3, viewModel.Nodes.Count);
        Assert.Equal(2, viewModel.Connections.Count);
        Assert.DoesNotContain(connection, viewModel.Connections);
        Assert.Contains(viewModel.Nodes, node => node.IsRerouteNode);
    }

    [Fact]
    public void EntityTab_DefineEventNode_UpdatesAvailableEvents()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel eventNode = new NodeViewModel
        {
            Type = "defineEvent",
            Category = "custom"
        };
        eventNode.SetProperty("eventName", "MyEvent");

        viewModel.Nodes.Add(eventNode);

        Assert.Contains("MyEvent", viewModel.AvailableEvents);
    }

    [Fact]
    public void EntityTab_AddAndRemoveVariable_UpdatesPaletteAndEntity()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.NewVariableName = "Health";
        viewModel.NewVariableType = "Integer";
        viewModel.AddVariableCommand.Execute(null);

        Assert.Contains(viewModel.Variables, v => v.Name == "Health");
        Assert.Contains(entity.Variables, v => v.Name == "Health");
        Assert.Contains(viewModel.InternalVariableNodes, n => n.Type == "var_get_Health");
        Assert.Contains(viewModel.InternalVariableNodes, n => n.Type == "var_set_Health");

        VariableDefinition variable = viewModel.Variables.First(v => v.Name == "Health");
        viewModel.RemoveVariableCommand.Execute(variable);

        Assert.DoesNotContain(viewModel.Variables, v => v.Name == "Health");
        Assert.DoesNotContain(viewModel.InternalVariableNodes, n => n.Type == "var_get_Health");
    }

    [Fact]
    public void EntityTab_ToggleFavoriteNode_SavesFavorites()
    {
        string[]? savedTypes = null;
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity, saveFavorites: types => savedTypes = types.ToArray());

        NodeOption option = viewModel.SimpleNodes.First(n => n.Definition != null);
        viewModel.ToggleFavoriteNodeCommand.Execute(option);

        Assert.Contains(viewModel.FavoriteNodes, n => n.Type == option.Type);
        Assert.NotNull(savedTypes);
        Assert.Contains(option.Type, savedTypes!);

        viewModel.ToggleFavoriteNodeCommand.Execute(option);

        Assert.DoesNotContain(viewModel.FavoriteNodes, n => n.Type == option.Type);
    }

    [Fact]
    public void EntityTab_NodeSearchFiltersAndGroups()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.NodeSearchText = "hero";

        Assert.NotEmpty(viewModel.FilteredNodes);
        Assert.All(viewModel.FilteredNodes, n =>
            Assert.True(n.Label.Contains("hero", System.StringComparison.OrdinalIgnoreCase) ||
                        n.Category.Contains("hero", System.StringComparison.OrdinalIgnoreCase) ||
                        (n.Type?.Contains("hero", System.StringComparison.OrdinalIgnoreCase) == true)));
        Assert.False(viewModel.HasRecentNodes);
        Assert.NotEmpty(viewModel.GroupedFilteredNodes);
    }

    [Fact]
    public void EntityTab_ExternalVariables_AppearInMenuAndApplyMetadata()
    {
        var externals = new[]
        {
            new ExternalVariableInfo("EntityA", "VarB", "String", "Default")
        };
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(
            entity,
            getExternalVariables: () => externals);

        viewModel.NodeSearchText = "EntityA.VarB";

        Assert.Contains(viewModel.FilteredExternalVariableNodes, n => n.Type == "var_get_ext_EntityA.VarB");

        NodeOption option = viewModel.ExternalVariableNodes.First(n => n.Type == "var_get_ext_EntityA.VarB");
        viewModel.SelectNodeFromMenuCommand.Execute(option);

        NodeViewModel created = viewModel.Nodes.Last();
        Assert.Equal("EntityA", created.Properties["extEntity"]);
        Assert.Equal("VarB", created.Properties["extVariable"]);
        Assert.Equal("String", created.Properties["extType"]);
        Assert.Equal("Default", created.Properties["extDefault"]);
    }

    [Fact]
    public void EntityTab_GraphWarnings_FlagsEntryWithoutOutgoing()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel entry = new NodeViewModel
        {
            Type = "onHeroTalks",
            Category = "trigger"
        };

        viewModel.Nodes.Add(entry);

        Assert.Contains(viewModel.GraphWarnings, warning =>
            warning.Contains("Entry node 'onHeroTalks' has no outgoing connections.", System.StringComparison.Ordinal));
    }

    [Fact]
    public void EntityTab_GraphWarnings_FlagsNodeWithoutIncoming()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel node = new NodeViewModel
        {
            Type = "showMessage",
            Category = "action"
        };

        viewModel.Nodes.Add(node);

        Assert.Contains(viewModel.GraphWarnings, warning =>
            warning.Contains("Node 'showMessage' has no incoming connections.", System.StringComparison.Ordinal));
    }

    [Fact]
    public void EntityTab_GraphWarnings_FlagsUnconnectedFlowOutputs()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel flow = new NodeViewModel
        {
            Type = "sequence",
            Title = "Sequence",
            Category = "flow"
        };
        viewModel.Nodes.Add(flow);
        viewModel.SelectedNode = flow;
        viewModel.AddOutputPinCommand.Execute(null);
        viewModel.Connections.Add(new ConnectionViewModel());

        Assert.Contains(viewModel.GraphWarnings, warning =>
            warning.Contains("Flow node 'Sequence' has unconnected output", System.StringComparison.Ordinal));
    }

    [Fact]
    public void EntityTab_UpdateVariableUsageCounts_FromNodeProperties()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        VariableDefinition variable = new VariableDefinition
        {
            Name = "Count",
            Type = "Integer",
            DefaultValue = "0"
        };
        viewModel.Variables.Add(variable);

        NodeViewModel node = new NodeViewModel
        {
            Type = "setState",
            Category = "action"
        };
        node.SetProperty("value", "$Count");
        viewModel.Nodes.Add(node);

        Assert.Equal(1, variable.UsageCount);
    }

    [Fact]
    public void EntityTab_OpenNodeMenu_HidesTriggerNodesDuringConnection()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity)
        {
            PendingConnection = new PendingConnectionViewModel
            {
                Source = new ConnectorViewModel { ConnectorType = ConnectorType.Exec }
            }
        };

        viewModel.OpenNodeMenuCommand.Execute(new System.Windows.Point(10, 10));

        Assert.DoesNotContain(viewModel.FilteredNodes, n => n.Category == "trigger");
    }

    [Fact]
    public void EntityTab_FinishConnection_RejectsExecToDataTypeMismatch()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel source = new NodeViewModel();
        NodeViewModel target = new NodeViewModel();
        target.Input.First().ConnectorType = ConnectorType.String;
        viewModel.Nodes.Add(source);
        viewModel.Nodes.Add(target);

        viewModel.StartConnectionCommand.Execute(source.Output.First());
        viewModel.FinishConnectionCommand.Execute(target.Input.First());

        Assert.Empty(viewModel.Connections);
    }

    [Fact]
    public void EntityTab_FinishConnection_AllowsWildcardDataConnection()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel source = new NodeViewModel();
        source.Output.First().ConnectorType = ConnectorType.Wildcard;
        NodeViewModel target = new NodeViewModel();
        target.Input.First().ConnectorType = ConnectorType.String;
        viewModel.Nodes.Add(source);
        viewModel.Nodes.Add(target);

        viewModel.StartConnectionCommand.Execute(source.Output.First());
        viewModel.FinishConnectionCommand.Execute(target.Input.First());

        Assert.Single(viewModel.Connections);
    }

    [Fact]
    public void EntityTab_FinishConnection_EnforcesSingleIncomingConnectionPerInput()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel sourceA = new NodeViewModel();
        NodeViewModel sourceB = new NodeViewModel();
        NodeViewModel target = new NodeViewModel();
        viewModel.Nodes.Add(sourceA);
        viewModel.Nodes.Add(sourceB);
        viewModel.Nodes.Add(target);

        ConnectorViewModel targetInput = target.Input.First();

        viewModel.StartConnectionCommand.Execute(sourceA.Output.First());
        viewModel.FinishConnectionCommand.Execute(targetInput);
        Assert.Single(viewModel.Connections);
        Assert.Equal(sourceA.Output.First(), viewModel.Connections[0].Source);

        viewModel.StartConnectionCommand.Execute(sourceB.Output.First());
        viewModel.FinishConnectionCommand.Execute(targetInput);
        Assert.Single(viewModel.Connections);
        Assert.Equal(sourceB.Output.First(), viewModel.Connections[0].Source);
    }

    [Fact]
    public void EntityTab_FinishConnection_RejectsConnectingIntoTriggerNode()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeViewModel source = new NodeViewModel();
        NodeViewModel triggerTarget = new NodeViewModel
        {
            Type = "onHeroTalks",
            Category = "trigger"
        };
        viewModel.Nodes.Add(source);
        viewModel.Nodes.Add(triggerTarget);

        viewModel.StartConnectionCommand.Execute(source.Output.First());
        viewModel.FinishConnectionCommand.Execute(triggerTarget.Input.First());

        Assert.Empty(viewModel.Connections);
    }
}
