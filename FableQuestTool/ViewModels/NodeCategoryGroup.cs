using System.Collections.Generic;
using System.Linq;
using FableQuestTool.Data;

namespace FableQuestTool.ViewModels;

/// <summary>
/// Represents a group of nodes organized by category for UE5-style context menu display
/// </summary>
public class NodeCategoryGroup
{
    public string Name { get; }
    public string Category { get; }
    public List<NodeOption> Nodes { get; }
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// Gets the category color for visual styling
    /// </summary>
    public string CategoryColor => Category switch
    {
        "trigger" => "#27AE60",    // Green for events
        "action" => "#3498DB",     // Blue for actions
        "condition" => "#F39C12",  // Orange for conditions
        "flow" => "#9B59B6",       // Purple for flow control
        "custom" => "#E91E63",     // Pink for custom events
        "variable" => "#00AA66",   // Teal for variables
        "variable-external" => "#00AA66",
        _ => "#808080"
    };

    /// <summary>
    /// Gets the category icon
    /// </summary>
    public string CategoryIcon => Category switch
    {
        "trigger" => "⚡",
        "action" => "▶",
        "condition" => "◆",
        "flow" => "↔",
        "custom" => "✦",
        "variable" => "📦",
        _ => "●"
    };

    /// <summary>
    /// Constructor with display name only (category derived from nodes)
    /// </summary>
    public NodeCategoryGroup(string name, List<NodeOption> nodes)
    {
        Name = name;
        Category = nodes.FirstOrDefault()?.Category ?? "";
        Nodes = nodes;
    }

    /// <summary>
    /// Constructor with explicit category
    /// </summary>
    public NodeCategoryGroup(string name, string category, List<NodeOption> nodes)
    {
        Name = name;
        Category = category;
        Nodes = nodes;
    }
}
