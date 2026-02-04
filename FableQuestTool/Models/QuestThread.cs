namespace FableQuestTool.Models;

/// <summary>
/// Represents a parallel execution thread for a quest.
///
/// Quest threads allow running multiple Lua coroutines simultaneously,
/// enabling quests that span multiple regions or have concurrent objectives.
/// Each thread executes independently and can be region-specific.
/// </summary>
/// <remarks>
/// Threads are useful for:
/// - Multi-region quests where different things happen in each area
/// - Concurrent objectives (e.g., defend location while collecting items)
/// - Background monitoring (e.g., tracking time limits, player actions)
///
/// The FSE runtime manages thread scheduling and ensures proper
/// synchronization when threads interact with shared quest state.
/// </remarks>
/// <example>
/// A quest defending two villages might have:
/// - Thread "DefendVillageA" running in "OAKVALE"
/// - Thread "DefendVillageB" running in "BOWERSTONE_SOUTH"
/// Both threads update shared state like "VillagersKilled".
/// </example>
public sealed class QuestThread
{
    /// <summary>
    /// Name of the Lua function that implements this thread.
    /// Must be a valid Lua function name defined in the generated quest script.
    /// Called when the thread starts execution.
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// Region where this thread executes.
    /// The thread activates when the player enters this region.
    /// Use Fable region names like "LOOKOUT_POINT" or "BOWERSTONE_SOUTH".
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of what this thread does.
    /// For documentation purposes - not used at runtime.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Pause interval (seconds) between iterations of the thread loop.
    /// Keeps the thread from running every frame when not needed.
    /// </summary>
    public float IntervalSeconds { get; set; } = 0.5f;

    /// <summary>
    /// Optional quest state name to use as a stop condition.
    /// When set, the thread exits when the state matches ExitStateValue.
    /// </summary>
    public string ExitStateName { get; set; } = string.Empty;

    /// <summary>
    /// The boolean value that triggers thread exit for ExitStateName.
    /// </summary>
    public bool ExitStateValue { get; set; } = true;
}
