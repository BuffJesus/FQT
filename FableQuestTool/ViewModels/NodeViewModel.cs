using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FableQuestTool.Data;

namespace FableQuestTool.ViewModels;

/// <summary>
/// ViewModel for a single node in the visual behavior graph editor.
///
/// NodeViewModel represents the visual state of a BehaviorNode in the Nodify-based
/// graph editor. It manages the node's appearance (title, icon, category colors),
/// position, connectors (input/output pins), and configuration properties.
///
/// The visual editor uses UE5 Blueprint-style coloring:
/// - Green: Triggers/Events (entry points)
/// - Blue: Actions (do something)
/// - Orange: Conditions (branching logic)
/// - Purple: Flow control (loops, sequences)
/// - Pink: Custom events (user-defined)
/// - Teal: Variables (state access)
/// </summary>
/// <remarks>
/// NodeViewModel maintains two-way binding:
/// - From BehaviorNode model: Type, Config, position loaded when opening project
/// - To BehaviorNode model: Changes saved back via EntityTabViewModel.SaveTabs()
///
/// Connectors are dynamically created based on node type:
/// - Triggers have no input connector (they're entry points)
/// - Branching nodes have multiple output connectors (True/False or custom labels)
/// - Regular nodes have one input and one output connector
///
/// The Properties dictionary stores node-specific configuration values that
/// correspond to the Config in BehaviorNode model.
/// </remarks>
public sealed partial class NodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string icon = string.Empty;

    [ObservableProperty]
    private string type = string.Empty;

    /// <summary>
    /// Legacy redirection node flag (for backwards compatibility)
    /// </summary>
    [ObservableProperty]
    private bool isRedirectionNode;

    /// <summary>
    /// UE5-style reroute node - a small circular node for organizing wires
    /// </summary>
    [ObservableProperty]
    private bool isRerouteNode;

    [ObservableProperty]
    private System.Windows.Point location;

    [ObservableProperty]
    private NodeDefinition? definition;

    /// <summary>
    /// Gets the header color based on category (UE5 Blueprint style)
    /// </summary>
    public string HeaderColor => Category switch
    {
        "trigger" => "#27AE60",    // Green for events/triggers
        "action" => "#3498DB",     // Blue for actions
        "condition" => "#F39C12",  // Orange for conditions (branches)
        "flow" => "#9B59B6",       // Purple for flow control
        "custom" => "#E91E63",     // Pink for custom events
        "variable" => "#00AA66",   // Teal for variables (UE5 style)
        _ => "#3498DB"
    };

    /// <summary>
    /// Gets the header gradient end color (slightly darker)
    /// </summary>
    public string HeaderColorDark => Category switch
    {
        "trigger" => "#1E8449",
        "action" => "#2980B9",
        "condition" => "#D68910",
        "flow" => "#7D3C98",
        "custom" => "#C2185B",
        "variable" => "#008855",
        _ => "#2980B9"
    };

    public Dictionary<string, object> Properties
    {
        get => properties;
        set
        {
            if (SetProperty(ref properties, value))
            {
                UpdateTitleForEventNodes();
            }
        }
    }
    private Dictionary<string, object> properties = new();

    public ObservableCollection<ConnectorViewModel> Input { get; } = new();
    public ObservableCollection<ConnectorViewModel> Output { get; } = new();

    public NodeViewModel()
    {
        // Most nodes have one input and one output (exec flow by default)
        Input.Add(new ConnectorViewModel { Title = "Exec", ConnectorType = ConnectorType.Exec, IsInput = true });
        Output.Add(new ConnectorViewModel { Title = "Exec", ConnectorType = ConnectorType.Exec, IsInput = false });
    }

    public void InitializeConnectors()
    {
        // Clear default connectors first
        Input.Clear();
        Output.Clear();

        bool isVariableGet = Type.StartsWith("var_get_", StringComparison.OrdinalIgnoreCase) ||
                             Type.StartsWith("var_get_ext", StringComparison.OrdinalIgnoreCase);
        bool isVariableSet = Type.StartsWith("var_set_", StringComparison.OrdinalIgnoreCase) ||
                             Type.StartsWith("var_set_ext", StringComparison.OrdinalIgnoreCase);

        ConnectorType? variableConnectorType = null;
        if (Definition?.ValueType != null)
        {
            variableConnectorType = MapVariableTypeToConnectorType(Definition.ValueType);
        }

        // Add input exec pin (all nodes have at least one input, except triggers and defineEvent)
        // defineEvent nodes are entry points like triggers - they define a callable function
        if (!isVariableGet && Category != "trigger" && Type != "defineEvent")
        {
            Input.Add(new ConnectorViewModel
            {
                Title = "▶",
                ConnectorType = ConnectorType.Exec,
                IsInput = true
            });
        }

        // Initialize output connectors based on node type
        if (!isVariableGet)
        {
            if (Definition?.HasBranching == true)
            {
                // Check if node has custom branch labels (e.g., Yes/No/Unsure)
                if (Definition.BranchLabels != null && Definition.BranchLabels.Count > 0)
                {
                    // Use custom labels
                    foreach (var label in Definition.BranchLabels)
                    {
                        Output.Add(new ConnectorViewModel
                        {
                            Title = label,
                            ConnectorType = ConnectorType.Exec,
                            IsInput = false
                        });
                    }
                }
                else
                {
                    // Default to True/False for branching nodes (condition style)
                    Output.Add(new ConnectorViewModel
                    {
                        Title = "True",
                        ConnectorType = ConnectorType.Exec,
                        IsInput = false
                    });
                    Output.Add(new ConnectorViewModel
                    {
                        Title = "False",
                        ConnectorType = ConnectorType.Exec,
                        IsInput = false
                    });
                }
            }
            else if (!isVariableGet)
            {
                // Single output exec pin
                Output.Add(new ConnectorViewModel
                {
                    Title = "▶",
                    ConnectorType = ConnectorType.Exec,
                    IsInput = false
                });
            }
        }

        // Data input pins for node properties
        if (Definition?.Properties != null)
        {
            foreach (var prop in Definition.Properties)
            {
                var connectorType = MapPropertyTypeToConnectorType(prop.Type);
                Input.Add(new ConnectorViewModel
                {
                    Title = string.IsNullOrWhiteSpace(prop.Label) ? prop.Name : prop.Label,
                    ConnectorType = connectorType,
                    IsInput = true,
                    PropertyName = prop.Name
                });
            }
        }

        // Variable data pins
        if (isVariableGet && variableConnectorType.HasValue)
        {
            string variableName = Type.StartsWith("var_get_ext", StringComparison.OrdinalIgnoreCase)
                ? Type.Substring("var_get_ext".Length).TrimStart('_')
                : Type.Substring("var_get_".Length);
            Output.Add(new ConnectorViewModel
            {
                Title = "Value",
                ConnectorType = variableConnectorType.Value,
                IsInput = false,
                VariableName = variableName
            });
        }
        else if (isVariableSet && variableConnectorType.HasValue)
        {
            string variableName = Type.StartsWith("var_set_ext", StringComparison.OrdinalIgnoreCase)
                ? Type.Substring("var_set_ext".Length).TrimStart('_')
                : Type.Substring("var_set_".Length);
            Output.Add(new ConnectorViewModel
            {
                Title = "Value",
                ConnectorType = variableConnectorType.Value,
                IsInput = false,
                VariableName = variableName
            });
        }

        // Notify that colors may have changed
        OnPropertyChanged(nameof(HeaderColor));
        OnPropertyChanged(nameof(HeaderColorDark));
    }

    public void UpdateTitleForEventNodes()
    {
        // Update title for event nodes to show the event name
        if (Type == "defineEvent" || Type == "callEvent")
        {
            if (Properties.TryGetValue("eventName", out var eventName))
            {
                var eventNameStr = eventName?.ToString();
                if (!string.IsNullOrWhiteSpace(eventNameStr))
                {
                    var baseLabel = Type == "defineEvent" ? "Define Event" : "Call Event";
                    Title = $"{baseLabel}: {eventNameStr}";
                    return;
                }
            }
            // Fallback to default label
            Title = Type == "defineEvent" ? "Define Event" : "Call Event";
        }
    }

    public void SetProperty(string propertyName, object value)
    {
        Properties[propertyName] = value;
        OnPropertyChanged(nameof(Properties));
        UpdateTitleForEventNodes();
    }

    private static ConnectorType MapPropertyTypeToConnectorType(string? type)
    {
        return type switch
        {
            "text" => ConnectorType.String,
            "string" => ConnectorType.String,
            "bool" => ConnectorType.Boolean,
            "int" => ConnectorType.Integer,
            "float" => ConnectorType.Float,
            _ => ConnectorType.Wildcard
        };
    }

    private static ConnectorType MapVariableTypeToConnectorType(string type)
    {
        return type switch
        {
            "Boolean" => ConnectorType.Boolean,
            "Integer" => ConnectorType.Integer,
            "Float" => ConnectorType.Float,
            "String" => ConnectorType.String,
            "Object" => ConnectorType.Object,
            _ => ConnectorType.Wildcard
        };
    }
}
