using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class QuestConfigViewModel : ObservableObject
{
    private readonly QuestIdManager questIdManager;

    /// <summary>
    /// Creates a new instance of QuestConfigViewModel.
    /// </summary>
    public QuestConfigViewModel()
    {
        var config = FableConfig.Load();
        questIdManager = new QuestIdManager(config);
    }

    [RelayCommand]
    private void SuggestQuestId()
    {
        int suggestedId = questIdManager.SuggestNextQuestId();

        // Get the MainViewModel's current project and set the ID
        var mainWindow = System.Windows.Application.Current.MainWindow;
        if (mainWindow?.DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.Project.Id = suggestedId;
            System.Windows.MessageBox.Show(
                $"Suggested Quest ID: {suggestedId}\n\nThis ID is available and ready to use.",
                "Quest ID Suggestion",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}
