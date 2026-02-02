using System.Collections.Generic;

namespace FableQuestTool.Models;

/// <summary>
/// Represents a single node in the visual behavior graph editor.
///
/// BehaviorNodes are the building blocks of entity behavior scripts in FableQuestTool.
/// Each node represents a specific action, condition, or control flow element that
/// gets converted to Lua code during code generation.
///
/// Nodes are connected via NodeConnection objects to form a directed graph that
/// defines the execution flow of entity behaviors.
/// </summary>
/// <remarks>
/// Node types are defined in NodeDefinitions.cs and include categories like:
/// - Flow: Start, branches, loops
/// - Actions: Movement, combat, dialogue
/// - Conditions: State checks, proximity tests
/// - Events: Triggers and callbacks
/// </remarks>
public sealed class BehaviorNode
{
    /// <summary>
    /// Unique identifier for this node instance within the entity's graph.
    /// Generated as a GUID when the node is created.
    /// Used to establish connections between nodes.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The node type identifier that determines its behavior.
    /// Maps to a NodeDefinition in NodeDefinitions.cs.
    /// Examples: "Start", "MoveTo", "Say", "WaitForState", "Branch"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Category grouping for the node (used in node picker UI).
    /// Examples: "Flow", "Actions", "Conditions", "Events", "Variables"
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Display label shown on the node in the visual editor.
    /// Can be customized by the user or defaults to the node type name.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Icon identifier for the node (displayed in node header).
    /// Uses Material Design icon names or custom icon paths.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// X coordinate position of the node in the visual editor canvas.
    /// Used to restore node positions when loading a project.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate position of the node in the visual editor canvas.
    /// Used to restore node positions when loading a project.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Configuration dictionary containing node-specific parameters.
    /// Keys and value types depend on the node Type.
    ///
    /// Common configuration keys include:
    /// - "target": Entity or marker to interact with
    /// - "message": Text for dialogue nodes
    /// - "state": State variable name for state nodes
    /// - "value": Value to set or compare
    /// - "duration": Time in seconds for wait nodes
    /// </summary>
    /// <example>
    /// MoveTo node: { "target": "MK_DESTINATION", "speed": "Walk" }
    /// Say node: { "message": "Hello adventurer!", "duration": 3.0 }
    /// </example>
    public Dictionary<string, object> Config { get; set; } = new();
}
