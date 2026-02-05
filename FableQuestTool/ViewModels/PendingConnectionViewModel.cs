using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class PendingConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    private ConnectorViewModel? source;

    [ObservableProperty]
    private System.Windows.Point target;
}
