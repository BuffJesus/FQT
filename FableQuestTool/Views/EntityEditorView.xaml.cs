using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class EntityEditorView : System.Windows.Controls.UserControl
{
    private Nodify.NodifyEditor? _editor;
    private bool _redirectNodeCreated = false;
    private System.Windows.Point? _rightClickDownPosition;

    public EntityEditorView()
    {
        InitializeComponent();
        // DataContext will be set by MainWindow

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Clean up event handlers
        if (_editor != null)
        {
            _editor.PreviewKeyDown -= OnEditorKeyDown;
            _editor.PreviewMouseLeftButtonUp -= OnEditorMouseUp;
            _editor.PreviewMouseLeftButtonDown -= OnEditorMouseLeftButtonDown;
            _editor.PreviewMouseRightButtonDown -= OnEditorRightClickDown;
            _editor.PreviewMouseRightButtonUp -= OnEditorRightClick;
            _editor = null;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Handle Ctrl key during drag to create redirection nodes
        try
        {
            _editor = FindNodifyEditor(this);
            if (_editor != null)
            {
                _editor.PreviewKeyDown -= OnEditorKeyDown;
                _editor.PreviewKeyDown += OnEditorKeyDown;

                // Add mouse down handler for double-click on connections
                _editor.PreviewMouseLeftButtonDown -= OnEditorMouseLeftButtonDown;
                _editor.PreviewMouseLeftButtonDown += OnEditorMouseLeftButtonDown;

                // Add mouse up handler to properly clean up after Ctrl+drag
                _editor.PreviewMouseLeftButtonUp -= OnEditorMouseUp;
                _editor.PreviewMouseLeftButtonUp += OnEditorMouseUp;

                // Add right-click handler for node menu
                _editor.PreviewMouseRightButtonDown -= OnEditorRightClickDown;
                _editor.PreviewMouseRightButtonDown += OnEditorRightClickDown;
                _editor.PreviewMouseRightButtonUp -= OnEditorRightClick;
                _editor.PreviewMouseRightButtonUp += OnEditorRightClick;
            }
        }
        catch
        {
            // Silently handle any initialization errors
        }
    }

    private void OnEditorKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        try
        {
            if (DataContext is not EntityEditorViewModel editorViewModel)
            {
                return;
            }

            var currentTab = editorViewModel.SelectedTab;
            if (currentTab == null)
            {
                return;
            }

            // Close node menu with Escape
            if (e.Key == Key.Escape && currentTab.IsNodeMenuOpen)
            {
                currentTab.CloseNodeMenuCommand.Execute(null);
                e.Handled = true;
                return;
            }

            // Create redirection node immediately when Ctrl is pressed during drag
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (currentTab.PendingConnection == null)
                {
                    return;
                }

                if (sender is not Nodify.NodifyEditor editor)
                {
                    return;
                }

                // Get current mouse position
                var position = Mouse.GetPosition(editor);

                // Check if we're on empty space (not on a connector)
                var hitElement = editor.InputHitTest(position) as DependencyObject;
                if (hitElement != null)
                {
                    var current = hitElement;
                    while (current != null && current != editor)
                    {
                        if (current is Nodify.Connector)
                        {
                            // On a connector, don't create redirection node
                            return;
                        }
                        current = System.Windows.Media.VisualTreeHelper.GetParent(current);
                    }
                }

                // Create redirection node immediately at current mouse position
                if (currentTab.CreateRedirectionNodeCommand.CanExecute(position))
                {
                    currentTab.CreateRedirectionNodeCommand.Execute(position);
                    _redirectNodeCreated = true; // Flag that we created a redirect node

                    // Simulate an Escape key press to cancel Nodify's internal drag state
                    var escapeEvent = new System.Windows.Input.KeyEventArgs(
                        Keyboard.PrimaryDevice,
                        Keyboard.PrimaryDevice.ActiveSource,
                        0,
                        Key.Escape)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    editor.RaiseEvent(escapeEvent);

                    e.Handled = true;
                }
            }
        }
        catch
        {
            // Silently handle key event errors
        }
    }

    private void OnEditorMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Handle double-click on connections to create reroute nodes
            if (e.ClickCount != 2)
            {
                return;
            }

            if (DataContext is not EntityEditorViewModel editorViewModel)
            {
                return;
            }

            var currentTab = editorViewModel.SelectedTab;
            if (currentTab == null)
            {
                return;
            }

            if (sender is not Nodify.NodifyEditor editor)
            {
                return;
            }

            var position = e.GetPosition(editor);
            var hitElement = editor.InputHitTest(position) as DependencyObject;

            // Check if we double-clicked on a connection/wire
            if (hitElement != null)
            {
                var current = hitElement;
                while (current != null && current != editor)
                {
                    if (current is Nodify.Connection connection && connection.DataContext is ConnectionViewModel connVm)
                    {
                        // Get graph position for creating the reroute node
                        var graphPosition = GetGraphPosition(editor, position);

                        // Create reroute node on the connection
                        if (currentTab.CreateRerouteOnConnectionCommand.CanExecute((connVm, graphPosition)))
                        {
                            currentTab.CreateRerouteOnConnectionCommand.Execute((connVm, graphPosition));
                            e.Handled = true;
                        }
                        return;
                    }
                    current = System.Windows.Media.VisualTreeHelper.GetParent(current);
                }
            }
        }
        catch
        {
            // Silently handle errors
        }
    }

    private void OnEditorMouseUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (DataContext is not EntityEditorViewModel editorViewModel)
            {
                return;
            }

            var currentTab = editorViewModel.SelectedTab;
            if (currentTab == null)
            {
                return;
            }

            if (sender is not Nodify.NodifyEditor editor)
            {
                return;
            }

            // Get the element that was clicked
            var position = e.GetPosition(editor);
            var graphPosition = GetGraphPosition(editor, position);
            var hitElement = editor.InputHitTest(position) as DependencyObject;

            bool clickedOnConnector = false;
            if (hitElement != null)
            {
                var current = hitElement;
                while (current != null && current != editor)
                {
                    if (current is Nodify.Connector)
                    {
                        clickedOnConnector = true;
                        break;
                    }
                    current = System.Windows.Media.VisualTreeHelper.GetParent(current);
                }
            }

            // If we have a pending connection and clicked on empty space (not a connector)
            if (currentTab.PendingConnection != null && !clickedOnConnector && !_redirectNodeCreated)
            {
                // Show node menu to create and connect to a new node
                if (currentTab.OpenNodeMenuCommand.CanExecute((position, graphPosition)))
                {
                    currentTab.OpenNodeMenuCommand.Execute((position, graphPosition));
                    FocusNodeSearchBox();
                    // Mark as handled to prevent Nodify from clearing the pending connection
                    e.Handled = true;
                }
                return;
            }

            // If we created a redirect node or clicked empty space, clear the pending connection
            if (currentTab.PendingConnection != null && (!clickedOnConnector || _redirectNodeCreated))
            {
                currentTab.PendingConnection = null;
                _redirectNodeCreated = false;
            }
        }
        catch
        {
            _redirectNodeCreated = false;
            // Silently handle any mouse up errors
        }
    }

    private void OnEditorRightClickDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Mark the event as handled to prevent Nodify from using right-click for panning
            e.Handled = true;
        }
        catch
        {
            // Silently handle any errors
        }
    }

    private void OnEditorRightClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (DataContext is not EntityEditorViewModel editorViewModel)
            {
                return;
            }

            var currentTab = editorViewModel.SelectedTab;
            if (currentTab == null)
            {
                return;
            }

            if (sender is not Nodify.NodifyEditor editor)
            {
                return;
            }

            var position = e.GetPosition(editor);
            var graphPosition = GetGraphPosition(editor, position);

            // Open node menu at right-click position
            if (currentTab.OpenNodeMenuCommand.CanExecute((position, graphPosition)))
            {
                currentTab.OpenNodeMenuCommand.Execute((position, graphPosition));
                FocusNodeSearchBox();
                e.Handled = true;
            }
        }
        catch
        {
            // Silently handle any errors
        }
    }

    private void FocusNodeSearchBox()
    {
        // Use dispatcher to focus the search box after the menu is rendered
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                // Find the NodeSearchTextBox within the visual tree
                var searchBox = FindVisualChild<System.Windows.Controls.TextBox>(this, "NodeSearchTextBox");
                if (searchBox != null)
                {
                    searchBox.Focus();
                    Keyboard.Focus(searchBox);
                }
            }
            catch
            {
                // Silently handle any focus errors
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null)
        {
            return null;
        }

        int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild && typedChild.Name == name)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static System.Windows.Point GetGraphPosition(Nodify.NodifyEditor editor, System.Windows.Point viewPosition)
    {
        try
        {
            double zoom = editor.ViewportZoom;
            var location = editor.ViewportLocation;
            if (zoom <= 0)
            {
                return viewPosition;
            }

            return new System.Windows.Point((viewPosition.X / zoom) + location.X, (viewPosition.Y / zoom) + location.Y);
        }
        catch
        {
            return viewPosition;
        }
    }

    private static Nodify.NodifyEditor? FindNodifyEditor(DependencyObject parent)
    {
        if (parent is Nodify.NodifyEditor editor)
        {
            return editor;
        }

        int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            var result = FindNodifyEditor(child);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void OnPropertyTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox textBox)
        {
            return;
        }

        if (DataContext is not EntityEditorViewModel editorViewModel)
        {
            return;
        }

        var currentTab = editorViewModel.SelectedTab;
        if (currentTab?.SelectedNode == null)
        {
            return;
        }

        var propertyName = textBox.Tag as string;
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        // Load current value from node properties
        if (currentTab.SelectedNode.Properties.TryGetValue(propertyName, out var value))
        {
            textBox.Text = value?.ToString() ?? string.Empty;
        }
    }

    private void OnPropertyTextBoxChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox textBox)
        {
            return;
        }

        if (DataContext is not EntityEditorViewModel editorViewModel)
        {
            return;
        }

        var currentTab = editorViewModel.SelectedTab;
        if (currentTab?.SelectedNode == null)
        {
            return;
        }

        var propertyName = textBox.Tag as string;
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        // Update node property and trigger title update
        currentTab.SelectedNode.SetProperty(propertyName, textBox.Text);
    }
}

// Converter to access dictionary values by key
public class DictionaryItemConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is System.Collections.Generic.Dictionary<string, object> dict && values[1] is string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }
        }
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { value };
    }
}
