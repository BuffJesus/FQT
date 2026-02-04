using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

/// <summary>
/// Connector type for UE5 Blueprint-style visualization
/// </summary>
public enum ConnectorType
{
    /// <summary>Execution flow (white arrow in UE5)</summary>
    Exec,
    /// <summary>Boolean data (red in UE5)</summary>
    Boolean,
    /// <summary>Integer data (cyan in UE5)</summary>
    Integer,
    /// <summary>Float data (green in UE5)</summary>
    Float,
    /// <summary>String data (magenta/pink in UE5)</summary>
    String,
    /// <summary>Object reference (blue in UE5)</summary>
    Object,
    /// <summary>Struct data (dark blue in UE5)</summary>
    Struct,
    /// <summary>Wildcard/any type (gray)</summary>
    Wildcard
}

public sealed partial class ConnectorViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private System.Windows.Point anchor;

    /// <summary>
    /// The type of connector for visual styling (UE5 Blueprint-style)
    /// </summary>
    [ObservableProperty]
    private ConnectorType connectorType = ConnectorType.Exec;

    /// <summary>
    /// Whether this is an input or output connector
    /// </summary>
    [ObservableProperty]
    private bool isInput;

    /// <summary>
    /// Whether this connector currently has a connection
    /// </summary>
    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string? propertyName;

    [ObservableProperty]
    private string? variableName;

    /// <summary>
    /// Gets the color for this connector type (UE5 Blueprint colors)
    /// </summary>
    public string ConnectorColor => ConnectorType switch
    {
        ConnectorType.Exec => "#FFFFFF",      // White for execution
        ConnectorType.Boolean => "#CC0000",   // Red for boolean
        ConnectorType.Integer => "#1CC4AF",   // Cyan/teal for integer
        ConnectorType.Float => "#9EEF5A",     // Light green for float
        ConnectorType.String => "#F0A1D4",    // Pink for string
        ConnectorType.Object => "#0099DB",    // Blue for object
        ConnectorType.Struct => "#0039A6",    // Dark blue for struct
        ConnectorType.Wildcard => "#808080",  // Gray for wildcard
        _ => "#808080"
    };
}
