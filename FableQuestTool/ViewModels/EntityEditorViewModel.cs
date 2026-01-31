using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityEditorViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<NodeOption> SimpleNodes { get; } = new()
    {
        new NodeOption("When Hero Talks", "trigger", "T"),
        new NodeOption("When Hero Hits", "trigger", "T"),
        new NodeOption("When Nearby", "trigger", "T"),
        new NodeOption("Show Dialogue", "action", "A"),
        new NodeOption("Give Reward", "action", "A"),
        new NodeOption("Follow Hero", "action", "A"),
        new NodeOption("Complete Quest", "action", "A")
    };

    public ObservableCollection<NodeOption> AdvancedNodes { get; } = new()
    {
        new NodeOption("Check State", "condition", "C"),
        new NodeOption("Check Global", "condition", "C"),
        new NodeOption("Sequence", "flow", "F"),
        new NodeOption("Parallel", "flow", "F"),
        new NodeOption("Lua Block", "custom", "L")
    };

    private int nodeSeed = 0;

    public EntityEditorViewModel()
    {
        Nodes.Add(CreateNode(new NodeOption("When Hero Talks", "trigger", "T"), 120, 120));
        Nodes.Add(CreateNode(new NodeOption("Show Dialogue", "action", "A"), 420, 200));
    }

    [RelayCommand]
    private void AddNode(NodeOption option)
    {
        if (option == null)
        {
            return;
        }

        nodeSeed++;
        double offset = 30 * (nodeSeed % 6);
        Nodes.Add(CreateNode(option, 140 + offset, 140 + offset));
    }

    [RelayCommand]
    private void ApplyTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return;
        }

        Nodes.Clear();
        if (template == "Talk")
        {
            Nodes.Add(CreateNode(new NodeOption("When Hero Talks", "trigger", "T"), 120, 120));
            Nodes.Add(CreateNode(new NodeOption("Show Dialogue", "action", "A"), 420, 160));
            Nodes.Add(CreateNode(new NodeOption("Complete Quest", "action", "A"), 720, 200));
        }
        else if (template == "Kill")
        {
            Nodes.Add(CreateNode(new NodeOption("When Hero Hits", "trigger", "T"), 120, 120));
            Nodes.Add(CreateNode(new NodeOption("Give Reward", "action", "A"), 420, 160));
            Nodes.Add(CreateNode(new NodeOption("Complete Quest", "action", "A"), 720, 200));
        }
        else if (template == "Fetch")
        {
            Nodes.Add(CreateNode(new NodeOption("When Item Given", "trigger", "T"), 120, 120));
            Nodes.Add(CreateNode(new NodeOption("Give Reward", "action", "A"), 420, 160));
            Nodes.Add(CreateNode(new NodeOption("Complete Quest", "action", "A"), 720, 200));
        }
    }

    private static NodeViewModel CreateNode(NodeOption option, double x, double y)
    {
        return new NodeViewModel
        {
            Title = option.Label,
            Category = option.Category,
            Icon = option.Icon,
            Location = new System.Windows.Point(x, y)
        };
    }
}
