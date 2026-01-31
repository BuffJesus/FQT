using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class NodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string icon = string.Empty;

    [ObservableProperty]
    private System.Windows.Point location;
}
