using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

public sealed partial class ConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    private ConnectorViewModel? source;

    [ObservableProperty]
    private ConnectorViewModel? target;

    /// <summary>
    /// Gets the wire color based on the source connector type (UE5 Blueprint style)
    /// </summary>
    public string WireColor => Source?.ConnectorColor ?? "#FFFFFF";

    /// <summary>
    /// Gets whether this is an execution flow connection
    /// </summary>
    public bool IsExecConnection => Source?.ConnectorType == ConnectorType.Exec;

    partial void OnSourceChanged(ConnectorViewModel? value)
    {
        OnPropertyChanged(nameof(WireColor));
        OnPropertyChanged(nameof(IsExecConnection));
    }
}
