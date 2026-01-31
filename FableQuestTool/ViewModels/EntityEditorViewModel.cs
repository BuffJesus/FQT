using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FableQuestTool.Data;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityEditorViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();
    public ObservableCollection<NodeOption> SimpleNodes { get; } = new();
    public ObservableCollection<NodeOption> AdvancedNodes { get; } = new();

    [ObservableProperty]
    private NodeViewModel? selectedNode;

    [ObservableProperty]
    private PendingConnectionViewModel? pendingConnection;

    [ObservableProperty]
    private bool isNodeMenuOpen;

    [ObservableProperty]
    private System.Windows.Point nodeMenuPosition;

    [ObservableProperty]
    private string nodeSearchText = string.Empty;

    public ObservableCollection<NodeOption> FilteredNodes { get; } = new();

    private int nodeSeed = 0;

    public EntityEditorViewModel()
    {
        LoadNodePalette();

        // Add sample nodes for testing
        var talkNode = NodeDefinitions.GetAllNodes().First(n => n.Type == "onHeroTalks");
        var dialogueNode = NodeDefinitions.GetAllNodes().First(n => n.Type == "showDialogue");

        Nodes.Add(CreateNode(talkNode, 120, 120));
        Nodes.Add(CreateNode(dialogueNode, 420, 200));
    }

    private void LoadNodePalette()
    {
        var allNodes = NodeDefinitions.GetAllNodes();

        // Simple nodes (non-advanced)
        foreach (var node in allNodes.Where(n => !n.IsAdvanced))
        {
            SimpleNodes.Add(new NodeOption(node.Label, node.Category, node.Icon)
            {
                Type = node.Type,
                Definition = node
            });
        }

        // Advanced nodes
        foreach (var node in allNodes.Where(n => n.IsAdvanced))
        {
            AdvancedNodes.Add(new NodeOption(node.Label, node.Category, node.Icon)
            {
                Type = node.Type,
                Definition = node
            });
        }
    }

    [RelayCommand]
    private void AddNode(NodeOption option)
    {
        if (option?.Definition == null)
        {
            return;
        }

        nodeSeed++;
        double offset = 30 * (nodeSeed % 6);
        Nodes.Add(CreateNode(option.Definition, 140 + offset, 140 + offset));
    }

    [RelayCommand]
    private void ApplyTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return;
        }

        var allNodes = NodeDefinitions.GetAllNodes();
        Nodes.Clear();

        if (template == "Talk")
        {
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "onHeroTalks"), 120, 120));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "showDialogue"), 420, 160));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "completeQuest"), 720, 200));
        }
        else if (template == "Kill")
        {
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "onHeroHits"), 120, 120));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "giveReward"), 420, 160));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "completeQuest"), 720, 200));
        }
        else if (template == "Fetch")
        {
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "onItemPresented"), 120, 120));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "giveReward"), 420, 160));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "completeQuest"), 720, 200));
        }
    }

    [RelayCommand]
    private void DeleteNode()
    {
        if (SelectedNode == null)
        {
            return;
        }

        // Remove connections to/from this node
        var connectionsToRemove = Connections.Where(c =>
            c.Source?.Equals(SelectedNode.Output.FirstOrDefault()) == true ||
            c.Target?.Equals(SelectedNode.Input.FirstOrDefault()) == true).ToList();

        foreach (var conn in connectionsToRemove)
        {
            Connections.Remove(conn);
        }

        Nodes.Remove(SelectedNode);
        SelectedNode = null;
    }

    [RelayCommand]
    private void DuplicateNode()
    {
        if (SelectedNode?.Definition == null)
        {
            return;
        }

        var duplicate = CreateNode(SelectedNode.Definition,
            SelectedNode.Location.X + 30,
            SelectedNode.Location.Y + 30);

        // Copy properties
        foreach (var prop in SelectedNode.Properties)
        {
            duplicate.Properties[prop.Key] = prop.Value;
        }

        Nodes.Add(duplicate);
        SelectedNode = duplicate;
    }

    [RelayCommand]
    private void AddOutputPin()
    {
        if (SelectedNode == null)
        {
            return;
        }

        // Only allow adding outputs to sequence/flow nodes
        if (SelectedNode.Category != "flow")
        {
            return;
        }

        var pinNumber = SelectedNode.Output.Count;
        SelectedNode.Output.Add(new ConnectorViewModel 
        { 
            Title = $"Then {pinNumber}" 
        });
    }

    [RelayCommand]
    private void RemoveOutputPin()
    {
        if (SelectedNode == null || SelectedNode.Output.Count <= 1)
        {
            return;
        }

        // Only allow removing from sequence/flow nodes
        if (SelectedNode.Category != "flow")
        {
            return;
        }

        var lastOutput = SelectedNode.Output.LastOrDefault();
        if (lastOutput != null)
        {
            // Remove any connections to this output
            var connectionsToRemove = Connections.Where(c => c.Source == lastOutput).ToList();
            foreach (var conn in connectionsToRemove)
            {
                Connections.Remove(conn);
            }

            SelectedNode.Output.Remove(lastOutput);
        }
    }

    [RelayCommand]
    private void StartConnection(ConnectorViewModel? connector)
    {
        if (connector == null)
        {
            return;
        }

        PendingConnection = new PendingConnectionViewModel
        {
            Source = connector
        };
    }

    [RelayCommand]
    private void FinishConnection(object? parameter)
    {
        try
        {
            ConnectorViewModel? targetConnector = null;

            // Handle different parameter types from Nodify
            if (parameter is ConnectorViewModel connector)
            {
                targetConnector = connector;
            }
            else if (parameter is ValueTuple<object, object> tuple)
            {
                // Nodify passes (Source, Target) tuple
                targetConnector = tuple.Item2 as ConnectorViewModel;
            }

            if (PendingConnection?.Source == null || targetConnector == null)
            {
                PendingConnection = null;
                return;
            }

            // Don't connect to the same connector
            if (PendingConnection.Source == targetConnector)
            {
                PendingConnection = null;
                return;
            }

            // Don't allow duplicate connections
            var existingConnection = Connections.FirstOrDefault(c =>
                c.Source == PendingConnection.Source && c.Target == targetConnector);
            if (existingConnection != null)
            {
                PendingConnection = null;
                return;
            }

            // Create the connection
            var connection = new ConnectionViewModel
            {
                Source = PendingConnection.Source,
                Target = targetConnector
            };

            Connections.Add(connection);
            PendingConnection = null;
        }
        catch
        {
            PendingConnection = null;
        }
    }

    [RelayCommand]
    private void DisconnectConnector(ConnectorViewModel? connector)
    {
        if (connector == null)
        {
            return;
        }

        var connectionsToRemove = Connections.Where(c =>
            c.Source == connector || c.Target == connector).ToList();

        foreach (var conn in connectionsToRemove)
        {
            Connections.Remove(conn);
        }
    }

    [RelayCommand]
    private void OpenNodeMenu(System.Windows.Point position)
    {
        NodeMenuPosition = position;
        NodeSearchText = string.Empty;
        UpdateFilteredNodes();
        IsNodeMenuOpen = true;
    }

    [RelayCommand]
    private void CloseNodeMenu()
    {
        IsNodeMenuOpen = false;
        NodeSearchText = string.Empty;
    }

    [RelayCommand]
    private void SelectNodeFromMenu(NodeOption? option)
    {
        if (option?.Definition == null)
        {
            return;
        }

        nodeSeed++;
        var newNode = CreateNode(option.Definition, NodeMenuPosition.X, NodeMenuPosition.Y);
        Nodes.Add(newNode);

        // If there's a pending connection, connect it to the new node
        if (PendingConnection?.Source != null)
        {
            var targetConnector = newNode.Input.FirstOrDefault();
            if (targetConnector != null)
            {
                var connection = new ConnectionViewModel
                {
                    Source = PendingConnection.Source,
                    Target = targetConnector
                };
                Connections.Add(connection);
            }
            PendingConnection = null;
        }

        IsNodeMenuOpen = false;
    }

    partial void OnNodeSearchTextChanged(string value)
    {
        UpdateFilteredNodes();
    }

    private void UpdateFilteredNodes()
    {
        FilteredNodes.Clear();

        var allNodes = SimpleNodes.Concat(AdvancedNodes);

        if (string.IsNullOrWhiteSpace(NodeSearchText))
        {
            foreach (var node in allNodes)
            {
                FilteredNodes.Add(node);
            }
        }
        else
        {
            var searchLower = NodeSearchText.ToLower();
            foreach (var node in allNodes.Where(n => 
                n.Label.ToLower().Contains(searchLower) || 
                n.Category.ToLower().Contains(searchLower)))
            {
                FilteredNodes.Add(node);
            }
        }
    }

    [RelayCommand]
    private void CreateRedirectionNode(System.Windows.Point location)
    {
        if (PendingConnection?.Source == null)
        {
            return;
        }

        // Store the source connector before clearing
        var sourceConnector = PendingConnection.Source;

        // Create a redirection (flow) node
        var flowDef = NodeDefinitions.GetAllNodes().FirstOrDefault(n => n.Type == "sequence");
        if (flowDef == null)
        {
            return;
        }

        var redirectNode = CreateNode(flowDef, location.X, location.Y);
        redirectNode.IsRedirectionNode = true;

        // Ensure the node has input and output connectors
        if (redirectNode.Input.Count == 0)
        {
            redirectNode.Input.Add(new ConnectorViewModel { Title = "Input" });
        }
        if (redirectNode.Output.Count == 0)
        {
            redirectNode.Output.Add(new ConnectorViewModel { Title = "Output" });
        }

        Nodes.Add(redirectNode);

        // Connect source to redirect node input
        var targetConnector = redirectNode.Input.FirstOrDefault();
        if (targetConnector != null)
        {
            var connection = new ConnectionViewModel
            {
                Source = sourceConnector,
                Target = targetConnector
            };
            Connections.Add(connection);
        }

        // Clear the pending connection completely - don't try to continue
        PendingConnection = null;
    }

    private static NodeViewModel CreateNode(NodeDefinition nodeDef, double x, double y)
    {
        return new NodeViewModel
        {
            Title = nodeDef.Label,
            Category = nodeDef.Category,
            Icon = nodeDef.Icon,
            Type = nodeDef.Type,
            Definition = nodeDef,
            Location = new System.Windows.Point(x, y)
        };
    }
}
