using System.Text;
using System.Windows;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class LuaPreviewView : Window
{
    public LuaPreviewView()
    {
        InitializeComponent();
    }

    private void OnCopyCurrent(object sender, RoutedEventArgs e)
    {
        if (PreviewTabs.SelectedItem is LuaPreviewItem item)
        {
            System.Windows.Clipboard.SetText(item.Content ?? string.Empty);
        }
    }

    private void OnCopyAll(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LuaPreviewViewModel viewModel || viewModel.Items == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (var item in viewModel.Items)
        {
            sb.AppendLine($"-- {item.Title}");
            sb.AppendLine(item.Content);
            sb.AppendLine();
        }

        System.Windows.Clipboard.SetText(sb.ToString().TrimEnd());
    }
}
