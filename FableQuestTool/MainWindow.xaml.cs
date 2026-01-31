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

        // Initialize ViewModels with MainViewModel reference
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

        // Find TemplatesView and wire up the template selection event
        var templatesView = FindName("TemplatesViewControl") as Views.TemplatesView;
        if (templatesView != null)
        {
            var templatesViewModel = new TemplatesViewModel();
            templatesView.DataContext = templatesViewModel;

            // When a template is selected, load it into the main project
            templatesViewModel.TemplateSelected += (project) =>
            {
                mainViewModel.Project = project;
                mainViewModel.EntityEditorViewModel?.RefreshFromProject();
                System.Windows.MessageBox.Show(
                    $"Template loaded successfully!\n\n" +
                    $"Quest: {project.Name}\n" +
                    $"Entities: {project.Entities.Count}\n\n" +
                    "Switch to the 'Quest Setup' or 'Entities' tab to view and edit.",
                    "Template Loaded",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            };
        }
    }
}
