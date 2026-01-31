using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class ConnectorViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private System.Windows.Point anchor;
}
