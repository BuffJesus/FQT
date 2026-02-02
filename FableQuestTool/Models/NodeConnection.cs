namespace FableQuestTool.Models;

/// <summary>
/// Represents a connection between two nodes in the visual behavior graph.
///
/// NodeConnections define the execution flow between BehaviorNodes. When a node
/// completes execution, control passes to all nodes connected to its output ports.
/// The CodeGenerator uses these connections to determine the order of Lua statements.
/// </summary>
/// <remarks>
/// Connections are directional: execution flows from FromNode to ToNode.
/// A single output port can connect to multiple input ports (branching).
/// A single input port can receive connections from multiple outputs (merging).
///
/// Port names vary by node type:
/// - Most nodes: "Input" and "Output"
/// - Branch nodes: "True" and "False" outputs
/// - Event nodes: Event-specific ports like "OnDeath", "OnInteract"
/// </remarks>
public sealed class NodeConnection
{
    /// <summary>
    /// ID of the source node where execution originates.
    /// References a BehaviorNode.Id in the same entity's Nodes collection.
    /// </summary>
    public string FromNodeId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the output port on the source node.
    /// Common values: "Output", "True", "False", "OnComplete"
    /// </summary>
    public string FromPort { get; set; } = "Output";

    /// <summary>
    /// ID of the destination node where execution continues.
    /// References a BehaviorNode.Id in the same entity's Nodes collection.
    /// </summary>
    public string ToNodeId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the input port on the destination node.
    /// Usually "Input" but may vary for special node types.
    /// </summary>
    public string ToPort { get; set; } = "Input";
}
