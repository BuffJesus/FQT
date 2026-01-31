using System.Windows;
using FableQuestTool.ViewModels;

namespace FableQuestTool;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
