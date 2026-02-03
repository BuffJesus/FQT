using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class EntityEditorView : System.Windows.Controls.UserControl
{
    private Nodify.NodifyEditor? _editor;
    private bool _redirectNodeCreated = false;
    private System.Windows.Point? _variableDragStart;
    private readonly Dictionary<string, string> _literalPropertyCache = new();

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

            if (currentTab.IsNodeMenuOpen && IsSourceWithinNodeMenu(e.OriginalSource as DependencyObject))
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
                    CancelEditorDrag(editor, currentTab);
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
            var hitElement = editor.InputHitTest(position) as DependencyObject;

            if (IsSourceWithinNodeMenu(hitElement))
            {
                return;
            }

            if (IsGraphElement(hitElement, editor))
            {
                return;
            }

            // Open node menu at right-click position
            if (currentTab.OpenNodeMenuCommand.CanExecute((position, graphPosition)))
            {
                currentTab.OpenNodeMenuCommand.Execute((position, graphPosition));
                CancelEditorDrag(editor, currentTab);
                FocusNodeSearchBox();
                e.Handled = true;
            }
        }
        catch
        {
            // Silently handle any errors
        }
    }

    private void CancelEditorDrag(Nodify.NodifyEditor editor, EntityTabViewModel currentTab)
    {
        try
        {
            currentTab.PendingConnection = null;
            _redirectNodeCreated = false;
            if (Mouse.Captured != null)
            {
                Mouse.Capture(null);
            }

            var escapeEvent = new System.Windows.Input.KeyEventArgs(
                Keyboard.PrimaryDevice,
                Keyboard.PrimaryDevice.ActiveSource,
                0,
                Key.Escape)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };
            editor.RaiseEvent(escapeEvent);
        }
        catch
        {
            // Silently handle any errors
        }
    }

    private void OnRootPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not EntityEditorViewModel editorViewModel)
        {
            return;
        }

        var currentTab = editorViewModel.SelectedTab;
        if (currentTab == null || !currentTab.IsNodeMenuOpen)
        {
            return;
        }

        if (IsSourceWithinNodeMenu(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (currentTab.CloseNodeMenuCommand.CanExecute(null))
        {
            currentTab.CloseNodeMenuCommand.Execute(null);
        }
    }

    private void OnNodeSearchKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (DataContext is not EntityEditorViewModel editorViewModel)
        {
            return;
        }

        var currentTab = editorViewModel.SelectedTab;
        if (currentTab == null || !currentTab.IsNodeMenuOpen)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (currentTab.CloseNodeMenuCommand.CanExecute(null))
            {
                currentTab.CloseNodeMenuCommand.Execute(null);
                e.Handled = true;
            }
            return;
        }

        if (currentTab.FilteredNodes.Count == 0)
        {
            return;
        }

        if (e.Key == Key.Up || e.Key == Key.Down)
        {
            int count = currentTab.FilteredNodes.Count;
            int index = currentTab.NodeMenuSelectedIndex;
            if (index < 0 || index >= count)
            {
                index = 0;
            }

            index = e.Key == Key.Up
                ? (index - 1 + count) % count
                : (index + 1) % count;

            currentTab.NodeMenuSelectedIndex = index;
            e.Handled = true;
            return;
        }

        if (e.Key != Key.Enter)
        {
            return;
        }

        int selectedIndex = currentTab.NodeMenuSelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= currentTab.FilteredNodes.Count)
        {
            selectedIndex = 0;
        }

        var selected = currentTab.FilteredNodes[selectedIndex];
        if (currentTab.SelectNodeFromMenuCommand.CanExecute(selected))
        {
            currentTab.SelectNodeFromMenuCommand.Execute(selected);
            e.Handled = true;
        }
    }

    private void OnNodePreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.DataContext is not NodeViewModel node)
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

        currentTab.SelectedNode = node;
    }

    private bool IsSourceWithinNodeMenu(DependencyObject? source)
    {
        if (source == null)
        {
            return false;
        }

        var menuBorder = FindVisualChild<FrameworkElement>(this, "NodeMenuBorder");
        if (menuBorder == null)
        {
            return false;
        }

        var current = source;
        while (current != null)
        {
            if (current == menuBorder)
            {
                return true;
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private static bool IsGraphElement(DependencyObject? source, Nodify.NodifyEditor editor)
    {
        if (source == null)
        {
            return false;
        }

        var current = source;
        while (current != null && current != editor)
        {
            if (current is Nodify.Node ||
                current is Nodify.Connection ||
                current is Nodify.Connector)
            {
                return true;
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private void OnNodeContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    {
        if (sender is not FrameworkElement element || element.ContextMenu == null)
        {
            return;
        }

        if (DataContext is not EntityEditorViewModel editorViewModel)
        {
            return;
        }

        element.ContextMenu.DataContext = editorViewModel.SelectedTab;
    }

    private void OnConnectionContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    {
        if (sender is not FrameworkElement element || element.ContextMenu == null)
        {
            return;
        }

        if (DataContext is not EntityEditorViewModel editorViewModel)
        {
            return;
        }

        element.ContextMenu.DataContext = editorViewModel.SelectedTab;
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
        CacheLiteralIfPresent(currentTab, propertyName);
    }

    private void OnPropertyBindToggleLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.CheckBox checkBox)
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

        string? propertyName = checkBox.Tag as string;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        if (currentTab.SelectedNode.Properties.TryGetValue(propertyName, out var value) &&
            value is string strValue &&
            strValue.StartsWith("$", StringComparison.Ordinal))
        {
            checkBox.IsChecked = true;
        }
    }

    private void OnPropertyBindChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.CheckBox checkBox)
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

        string? propertyName = checkBox.Tag as string;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        CacheLiteralIfPresent(currentTab, propertyName);

        var propertyDefinition = currentTab.SelectedNode.Definition?.Properties
            .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        string? expectedType = propertyDefinition?.Type;

        var variable = currentTab.Variables
            .FirstOrDefault(v => IsVariableTypeCompatible(v.Type, expectedType));
        if (variable == null)
        {
            checkBox.IsChecked = false;
            return;
        }

        if (!TryBindVariableToProperty(currentTab, propertyName, variable.Name))
        {
            checkBox.IsChecked = false;
        }
    }

    private void OnPropertyBindUnchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.CheckBox checkBox)
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

        string? propertyName = checkBox.Tag as string;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        string cacheKey = BuildPropertyCacheKey(currentTab, propertyName);
        if (_literalPropertyCache.TryGetValue(cacheKey, out string? cachedValue) && cachedValue != null)
        {
            currentTab.SelectedNode.SetProperty(propertyName, cachedValue);
        }
        else
        {
            var propertyDefinition = currentTab.SelectedNode.Definition?.Properties
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            string defaultValue = propertyDefinition?.DefaultValue?.ToString() ?? string.Empty;
            currentTab.SelectedNode.SetProperty(propertyName, defaultValue);
        }
    }

    private void OnPropertyVariableComboLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.ComboBox comboBox)
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

        string? propertyName = comboBox.Tag as string;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        var propertyDefinition = currentTab.SelectedNode.Definition?.Properties
            .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

        string? expectedType = propertyDefinition?.Type;
        var compatible = currentTab.Variables
            .Where(v => IsVariableTypeCompatible(v.Type, expectedType))
            .ToList();

        if (currentTab.SelectedNode.Properties.TryGetValue(propertyName, out var existingValue) &&
            existingValue is string existingStr &&
            existingStr.StartsWith("$", StringComparison.Ordinal))
        {
            string boundName = existingStr.Substring(1);
            var boundVariable = currentTab.Variables.FirstOrDefault(v =>
                v.Name.Equals(boundName, StringComparison.OrdinalIgnoreCase));
            if (boundVariable != null && !compatible.Contains(boundVariable))
            {
                compatible.Add(boundVariable);
            }
        }

        if (compatible.Count == 0)
        {
            compatible = currentTab.Variables.ToList();
        }

        comboBox.ItemsSource = compatible;

        if (currentTab.SelectedNode.Properties.TryGetValue(propertyName, out var value) &&
            value is string strValue &&
            strValue.StartsWith("$", StringComparison.Ordinal))
        {
            comboBox.SelectedValue = strValue.Substring(1);
        }
    }

    private void OnPropertyVariableSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is not System.Windows.Controls.ComboBox comboBox)
        {
            return;
        }

        if (comboBox.SelectedValue is not string variableName || string.IsNullOrWhiteSpace(variableName))
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

        string? propertyName = comboBox.Tag as string;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        TryBindVariableToProperty(currentTab, propertyName, variableName);
    }

    private void OnVariableDragStart(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.Button)
        {
            return;
        }

        _variableDragStart = e.GetPosition(null);
    }

    private void OnVariableMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _variableDragStart == null)
        {
            return;
        }

        var currentPosition = e.GetPosition(null);
        var delta = currentPosition - _variableDragStart.Value;

        if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.DataContext is not VariableDefinition variable || string.IsNullOrWhiteSpace(variable.Name))
        {
            return;
        }

        DragDrop.DoDragDrop(element, variable.Name, System.Windows.DragDropEffects.Copy);
        _variableDragStart = null;
    }

    private void OnPropertyTextBoxDragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(string)))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void OnPropertyTextBoxDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox textBox)
        {
            return;
        }

        if (!e.Data.GetDataPresent(typeof(string)))
        {
            return;
        }

        if (e.Data.GetData(typeof(string)) is not string variableName || string.IsNullOrWhiteSpace(variableName))
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

        string? propertyName = textBox.Tag as string;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        string? variableType = currentTab.Variables
            .FirstOrDefault(v => v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase))
            ?.Type;

        string? expectedType = currentTab.SelectedNode.Definition?.Properties
            .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            ?.Type;

        if (!IsVariableTypeCompatible(variableType, expectedType))
        {
            System.Windows.MessageBox.Show(
                $"Variable '{variableName}' is {variableType ?? "Unknown"}, but '{propertyName}' expects {expectedType ?? "Unknown"}.",
                "Variable Type Mismatch",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        textBox.Text = $"${variableName}";
        e.Handled = true;
    }

    private void OnEditorVariableDragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(string)))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void OnEditorVariableDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(string)))
        {
            return;
        }

        if (e.Data.GetData(typeof(string)) is not string variableName || string.IsNullOrWhiteSpace(variableName))
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

        bool isSetNode = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        var viewPosition = e.GetPosition(editor);
        var graphPosition = GetGraphPosition(editor, viewPosition);
        currentTab.CreateVariableNodeAtPosition(variableName, isSetNode, graphPosition);
        e.Handled = true;
    }

    private static bool IsVariableTypeCompatible(string? variableType, string? expectedType)
    {
        string? normalizedExpected = NormalizePropertyType(expectedType);
        string? normalizedVariable = NormalizeVariableType(variableType);

        if (string.IsNullOrWhiteSpace(normalizedExpected) || string.IsNullOrWhiteSpace(normalizedVariable))
        {
            return false;
        }

        return normalizedExpected == normalizedVariable;
    }

    private bool TryBindVariableToProperty(EntityTabViewModel currentTab, string propertyName, string variableName)
    {
        string? variableType = currentTab.Variables
            .FirstOrDefault(v => v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase))
            ?.Type;

        string? expectedType = currentTab.SelectedNode?.Definition?.Properties
            .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            ?.Type;

        if (!IsVariableTypeCompatible(variableType, expectedType))
        {
            System.Windows.MessageBox.Show(
                $"Variable '{variableName}' is {variableType ?? "Unknown"}, but '{propertyName}' expects {expectedType ?? "Unknown"}.",
                "Variable Type Mismatch",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        currentTab.SelectedNode?.SetProperty(propertyName, $"${variableName}");
        return true;
    }

    private void CacheLiteralIfPresent(EntityTabViewModel currentTab, string propertyName)
    {
        if (currentTab.SelectedNode == null)
        {
            return;
        }

        if (!currentTab.SelectedNode.Properties.TryGetValue(propertyName, out var value))
        {
            return;
        }

        if (value is not string strValue || strValue.StartsWith("$", StringComparison.Ordinal))
        {
            return;
        }

        string cacheKey = BuildPropertyCacheKey(currentTab, propertyName);
        _literalPropertyCache[cacheKey] = strValue;
    }

    private static string BuildPropertyCacheKey(EntityTabViewModel currentTab, string propertyName)
    {
        string nodeId = currentTab.SelectedNode?.Id ?? string.Empty;
        return $"{nodeId}::{propertyName}";
    }

    private static string? NormalizePropertyType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        return type switch
        {
            "text" => "string",
            "string" => "string",
            "bool" => "bool",
            "int" => "int",
            "float" => "float",
            _ => null
        };
    }

    private static string? NormalizeVariableType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        return type switch
        {
            "String" => "string",
            "Boolean" => "bool",
            "Integer" => "int",
            "Float" => "float",
            _ => null
        };
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
