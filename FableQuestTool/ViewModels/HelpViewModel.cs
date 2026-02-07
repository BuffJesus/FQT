using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

/// <summary>
/// ViewModel for the in-app user guide.
/// </summary>
public sealed partial class HelpViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "FQT User Guide";

    [ObservableProperty]
    private string content = string.Empty;
}
