using System.Windows;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

/// <summary>
/// Window for enabling/disabling and deleting deployed quests.
/// </summary>
public partial class QuestManagerView : Window
{
    /// <summary>
    /// Creates a new instance of QuestManagerView.
    /// </summary>
    public QuestManagerView()
    {
        InitializeComponent();
        DataContext = new QuestManagerViewModel();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
