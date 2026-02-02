namespace FableQuestTool.Models;

/// <summary>
/// Represents a state variable used to track quest progress.
///
/// Quest states are variables that persist across game sessions and can be
/// read/written from Lua scripts via Quest:GetState() and Quest:SetState().
/// They are the primary mechanism for tracking quest progress, recording
/// player decisions, and coordinating between different parts of a quest.
/// </summary>
/// <remarks>
/// States are stored in the game's save file when Persist is true.
/// State names must be valid Lua identifiers (no spaces, alphanumeric + underscore).
///
/// Supported types:
/// - "bool": True/false values (default: false)
/// - "int": Integer numbers (default: 0)
/// - "float": Decimal numbers (default: 0.0)
/// - "string": Text values (default: "")
/// </remarks>
/// <example>
/// Common state patterns:
/// - "QuestStarted" (bool): Track if quest has begun
/// - "EnemiesKilled" (int): Count progress toward kill objective
/// - "ChosenPath" (string): Record player's decision at branch point
/// </example>
public sealed class QuestState
{
    /// <summary>
    /// Unique identifier for this state within the project.
    /// Auto-generated GUID used internally.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the state variable.
    /// Used in Lua code: Quest:GetState("StateName") / Quest:SetState("StateName", value)
    /// Must be a valid Lua identifier.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Data type of the state variable.
    /// Supported values: "bool", "int", "float", "string"
    /// </summary>
    public string Type { get; set; } = "bool";

    /// <summary>
    /// Whether this state persists in the game's save file.
    /// Persistent states survive game exits and reloads.
    /// Non-persistent states reset when the game restarts.
    /// </summary>
    public bool Persist { get; set; } = true;

    /// <summary>
    /// Initial value for this state when the quest starts.
    /// Type should match the Type property.
    /// </summary>
    public object? DefaultValue { get; set; }
}
