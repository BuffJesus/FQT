using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using System.Windows;

namespace FableQuestTool.Views;

/// <summary>
/// Modal window for browsing and selecting game entities.
/// </summary>
public partial class EntityBrowserView : Window
{
    /// <summary>
    /// Creates a new instance of EntityBrowserView.
    /// </summary>
    public EntityBrowserView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Gets SelectedEntity.
    /// </summary>
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
