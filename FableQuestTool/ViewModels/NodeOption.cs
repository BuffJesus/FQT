using System.Linq;
using FableQuestTool.Data;

namespace FableQuestTool.ViewModels;

/// <summary>
/// Represents a node type entry in the node picker UI.
/// </summary>
public sealed class NodeOption
{
    /// <summary>
    /// Creates a new instance of NodeOption.
    /// </summary>
    public NodeOption(string label, string category, string icon, string description = "")
    {
        Label = label;
        Category = category;
        Icon = icon;
        Description = description;
    }

    /// <summary>
    /// Gets Label.
    /// </summary>
    public string Label { get; }
    /// <summary>
    /// Gets Category.
    /// </summary>
    public string Category { get; }
    /// <summary>
    /// Gets Icon.
    /// </summary>
    public string Icon { get; }
    public string IconDisplay => NormalizeIcon(Icon, Category);
    /// <summary>
    /// Gets Description.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Gets or sets Type.
    /// </summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets Definition.
    /// </summary>
    public NodeDefinition? Definition { get; init; }
    /// <summary>
    /// Gets or sets MenuIndex.
    /// </summary>
    public int MenuIndex { get; set; } = -1;
    /// <summary>
    /// Gets or sets ExternalEntityName.
    /// </summary>
    public string? ExternalEntityName { get; init; }
    /// <summary>
    /// Gets or sets ExternalVariableName.
    /// </summary>
    public string? ExternalVariableName { get; init; }
    /// <summary>
    /// Gets or sets ExternalVariableType.
    /// </summary>
    public string? ExternalVariableType { get; init; }
    /// <summary>
    /// Gets or sets ExternalVariableDefault.
    /// </summary>
    public string? ExternalVariableDefault { get; init; }

    private static string NormalizeIcon(string icon, string category)
    {
        if (!IsPlaceholderIcon(icon))
        {
            return icon;
        }

        return category switch
        {
            "trigger" => "TRG",
            "action" => "ACT",
            "condition" => "IF",
            "flow" => "FLW",
            "custom" => "EVT",
            "variable" => "VAR",
            _ => "NOD"
        };
    }

    private static bool IsPlaceholderIcon(string icon)
    {
        return string.IsNullOrWhiteSpace(icon) || icon.All(ch => ch == '?');
    }
}
