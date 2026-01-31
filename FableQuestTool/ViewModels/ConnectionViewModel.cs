using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class ConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    private ConnectorViewModel? source;

    [ObservableProperty]
    private ConnectorViewModel? target;
}
