using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.ViewModels;

/// <summary>
/// Represents a user-defined variable for the node graph (UE5 Blueprint-style)
/// </summary>
public sealed partial class VariableDefinition : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string type = "String";

    [ObservableProperty]
    private string defaultValue = string.Empty;

    [ObservableProperty]
    private int usageCount;

    /// <summary>
    /// Gets the color for this variable type (UE5 Blueprint colors)
    /// </summary>
    public string TypeColor => Type switch
    {
        "Boolean" => "#CC0000",   // Red
        "Integer" => "#1CC4AF",   // Cyan/teal
        "Float" => "#9EEF5A",     // Light green
        "String" => "#F0A1D4",    // Pink
        _ => "#808080"            // Gray
    };

    /// <summary>
    /// Gets the icon for this variable type
    /// </summary>
    public string TypeIcon => Type switch
    {
        "Boolean" => "?",
        "Integer" => "#",
        "Float" => ".",
        "String" => "T",
        _ => "?"
    };
}
