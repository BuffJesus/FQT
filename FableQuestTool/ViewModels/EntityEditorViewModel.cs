using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityEditorViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<string> SimpleNodes { get; } = new()
    {
        "Trigger: When Hero Talks",
        "Trigger: When Hero Hits",
        "Trigger: When Nearby",
        "Action: Show Dialogue",
        "Action: Give Reward",
        "Action: Follow Hero",
        "Action: Complete Quest"
    };

    public ObservableCollection<string> AdvancedNodes { get; } = new()
    {
        "Condition: Check State",
        "Condition: Check Global",
        "Flow: Sequence",
        "Flow: Parallel",
        "Custom: Lua Block"
    };

    private int nodeSeed = 0;

    public EntityEditorViewModel()
    {
        Nodes.Add(new NodeViewModel { Title = "Trigger: On Hero Talks", Location = new System.Windows.Point(120, 120) });
        Nodes.Add(new NodeViewModel { Title = "Action: Show Dialogue", Location = new System.Windows.Point(420, 200) });
    }

    [RelayCommand]
    private void AddNode(string nodeLabel)
    {
        nodeSeed++;
        double offset = 30 * (nodeSeed % 6);
        Nodes.Add(new NodeViewModel
        {
            Title = nodeLabel,
            Location = new System.Windows.Point(140 + offset, 140 + offset)
        });
    }
}
