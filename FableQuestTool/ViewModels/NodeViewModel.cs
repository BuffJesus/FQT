using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FableQuestTool.Data;

namespace FableQuestTool.ViewModels;

public sealed partial class NodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string icon = string.Empty;

    [ObservableProperty]
    private string type = string.Empty;

    [ObservableProperty]
    private bool isRedirectionNode;

    [ObservableProperty]
    private System.Windows.Point location;

    [ObservableProperty]
    private NodeDefinition? definition;

    public Dictionary<string, object> Properties
    {
        get => properties;
        set
        {
            if (SetProperty(ref properties, value))
            {
                UpdateTitleForEventNodes();
            }
        }
    }
    private Dictionary<string, object> properties = new();

    public ObservableCollection<ConnectorViewModel> Input { get; } = new();
    public ObservableCollection<ConnectorViewModel> Output { get; } = new();

    public NodeViewModel()
    {
        // Most nodes have one input and one output
        Input.Add(new ConnectorViewModel { Title = "Input" });
        Output.Add(new ConnectorViewModel { Title = "Output" });
    }

    public void InitializeConnectors()
    {
        // Initialize connectors based on node type
        if (Definition?.HasBranching == true)
        {
            Output.Clear();

            // Check if node has custom branch labels (e.g., Yes/No/Unsure)
            if (Definition.BranchLabels != null && Definition.BranchLabels.Count > 0)
            {
                // Use custom labels
                foreach (var label in Definition.BranchLabels)
                {
                    Output.Add(new ConnectorViewModel { Title = label });
                }
            }
            else
            {
                // Default to True/False for branching nodes
                Output.Add(new ConnectorViewModel { Title = "True" });
                Output.Add(new ConnectorViewModel { Title = "False" });
            }
        }
    }

    public void UpdateTitleForEventNodes()
    {
        // Update title for event nodes to show the event name
        if (Type == "defineEvent" || Type == "callEvent")
        {
            if (Properties.TryGetValue("eventName", out var eventName))
            {
                var eventNameStr = eventName?.ToString();
                if (!string.IsNullOrWhiteSpace(eventNameStr))
                {
                    var baseLabel = Type == "defineEvent" ? "Define Event" : "Call Event";
                    Title = $"{baseLabel}: {eventNameStr}";
                    return;
                }
            }
            // Fallback to default label
            Title = Type == "defineEvent" ? "Define Event" : "Call Event";
        }
    }

    public void SetProperty(string propertyName, object value)
    {
        Properties[propertyName] = value;
        OnPropertyChanged(nameof(Properties));
        UpdateTitleForEventNodes();
    }
}
