using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityEditorViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();

    public EntityEditorViewModel()
    {
        Nodes.Add(new NodeViewModel { Title = "Trigger: On Hero Talks", Location = new System.Windows.Point(120, 120) });
        Nodes.Add(new NodeViewModel { Title = "Action: Show Dialogue", Location = new System.Windows.Point(420, 200) });
    }
}
