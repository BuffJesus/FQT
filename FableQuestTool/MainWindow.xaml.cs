using System.Windows;
using FableQuestTool.ViewModels;

namespace FableQuestTool;

public partial class MainWindow : Window
{
    private readonly MainViewModel mainViewModel;

    public MainWindow()
    {
        InitializeComponent();
        mainViewModel = new MainViewModel();
        DataContext = mainViewModel;

        // Initialize EntityEditorViewModel with MainViewModel reference
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find EntityEditorView and set its DataContext
        var entityEditorView = FindName("EntityEditorViewControl") as Views.EntityEditorView;
        if (entityEditorView != null)
        {
            var entityEditorViewModel = new EntityEditorViewModel(mainViewModel);
            entityEditorView.DataContext = entityEditorViewModel;
            mainViewModel.EntityEditorViewModel = entityEditorViewModel;
        }
    }
}
