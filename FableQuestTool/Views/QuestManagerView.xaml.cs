using System.Windows;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class QuestManagerView : Window
{
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
