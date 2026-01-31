using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using System.Windows;

namespace FableQuestTool.Views;

public partial class EntityBrowserView : Window
{
    public EntityBrowserView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public TngEntity? SelectedEntity { get; private set; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Auto-load entities when dialog opens
        if (DataContext is EntityBrowserViewModel viewModel)
        {
            viewModel.LoadEntitiesCommand.Execute(null);
        }
    }

    private void OnSelectClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is EntityBrowserViewModel viewModel)
        {
            SelectedEntity = viewModel.SelectedEntity;
            DialogResult = true;
            Close();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        SelectedEntity = null;
        DialogResult = false;
        Close();
    }
}
