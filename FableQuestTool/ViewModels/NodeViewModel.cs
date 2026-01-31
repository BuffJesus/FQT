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

    [ObservableProperty]
    private Dictionary<string, object> properties = new();

    public ObservableCollection<ConnectorViewModel> Input { get; } = new();
    public ObservableCollection<ConnectorViewModel> Output { get; } = new();

    public NodeViewModel()
    {
        // Most nodes have one input and one output
        Input.Add(new ConnectorViewModel { Title = "Input" });
        Output.Add(new ConnectorViewModel { Title = "Output" });
    }
}
