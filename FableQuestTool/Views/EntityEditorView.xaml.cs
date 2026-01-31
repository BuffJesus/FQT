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

    public EntityEditorView()
    {
        InitializeComponent();
        DataContext = new EntityEditorViewModel();

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
            if (DataContext is not EntityEditorViewModel viewModel)
            {
                return;
            }

            // Close node menu with Escape
            if (e.Key == Key.Escape && viewModel.IsNodeMenuOpen)
            {
                viewModel.CloseNodeMenuCommand.Execute(null);
                e.Handled = true;
                return;
            }

            // Create redirection node immediately when Ctrl is pressed during drag
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (viewModel.PendingConnection == null)
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
                if (viewModel.CreateRedirectionNodeCommand.CanExecute(position))
                {
                    viewModel.CreateRedirectionNodeCommand.Execute(position);
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

    private void OnEditorMouseUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (DataContext is not EntityEditorViewModel vm)
            {
                return;
            }

            if (sender is not Nodify.NodifyEditor editor)
            {
                return;
            }

            // Get the element that was clicked
            var position = e.GetPosition(editor);
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
            if (vm.PendingConnection != null && !clickedOnConnector && !_redirectNodeCreated)
            {
                // Show node menu to create and connect to a new node
                if (vm.OpenNodeMenuCommand.CanExecute(position))
                {
                    vm.OpenNodeMenuCommand.Execute(position);
                }
                return;
            }

            // If we created a redirect node or clicked empty space, clear the pending connection
            if (vm.PendingConnection != null && (!clickedOnConnector || _redirectNodeCreated))
            {
                vm.PendingConnection = null;
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
            if (DataContext is not EntityEditorViewModel vm)
            {
                return;
            }

            if (sender is not Nodify.NodifyEditor editor)
            {
                return;
            }

            var position = e.GetPosition(editor);
            
            // Open node menu at right-click position
            if (vm.OpenNodeMenuCommand.CanExecute(position))
            {
                vm.OpenNodeMenuCommand.Execute(position);
                e.Handled = true;
            }
        }
        catch
        {
            // Silently handle any errors
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
}
