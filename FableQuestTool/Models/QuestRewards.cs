using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace FableQuestTool.Models;

/// <summary>
/// Defines rewards given to the player upon quest completion.
///
/// QuestRewards supports multiple reward types that can be combined:
/// - Currency: Gold, experience points, renown
/// - Morality: Alignment shift (positive = good, negative = evil)
/// - Items: Single direct item or multiple items via container
/// - Abilities: New spells or skills unlocked
/// </summary>
/// <remarks>
/// Due to Fable engine limitations, only one item can be given directly.
/// For multiple item rewards, use ContainerReward which spawns a chest
/// containing all reward items for the player to collect.
/// </remarks>
public sealed class QuestRewards
{
    /// <summary>
    /// Gold coins awarded to the player.
    /// Added directly to player's inventory.
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// General experience points awarded.
    /// Contributes to hero's overall level and available upgrade points.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Renown points awarded.
    /// Renown affects how NPCs react to the hero and unlocks certain content.
    /// </summary>
    public int Renown { get; set; }

    /// <summary>
    /// Morality alignment change.
    /// Positive values shift toward good, negative toward evil.
    /// Range typically -100 to +100.
    /// </summary>
    public float Morality { get; set; }

    /// <summary>
    /// Item rewards given directly to the player on quest completion.
    /// Use item definition names like "OBJECT_SWORD_MASTER" or "OBJECT_HEALTH_POTION".
    /// </summary>
    public ObservableCollection<string> Items { get; set; } = new();

    /// <summary>
    /// Legacy single-item reward (for backward compatibility).
    /// When loaded, this value is migrated into Items.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DirectRewardItem
    {
        get => null; // Do not serialize forward
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Items.Add(value);
            }
        }
    }

    /// <summary>
    /// Container-based rewards for giving multiple items.
    /// Spawns a chest or other container with items inside.
    /// Player must interact with container to receive items.
    /// </summary>
    public ContainerReward? Container { get; set; }

    /// <summary>
    /// Abilities (spells/skills) unlocked upon quest completion.
    /// Use ability definition names like "ABILITY_FIREBALL" or "ABILITY_FLOURISH".
    /// </summary>
    public ObservableCollection<string> Abilities { get; set; } = new();

    // Legacy Items field is handled by the Items collection itself.
}

/// <summary>
/// Configuration for spawning a container with multiple reward items.
///
/// Since Fable's engine can only give one item directly to the player,
/// ContainerReward provides a workaround by spawning a container object
/// (chest, barrel, etc.) filled with reward items.
/// </summary>
public sealed class ContainerReward
{
    /// <summary>
    /// Definition name of the container object to spawn.
    /// Common values: "OBJECT_CHEST", "OBJECT_BARREL", "OBJECT_CRATE"
    /// Can use any valid OBJECT_* definition from the game.
    /// </summary>
    public string ContainerDefName { get; set; } = "OBJECT_CHEST";

    /// <summary>
    /// Script name for the spawned container instance.
    /// Used to reference the container in Lua code if needed.
    /// </summary>
    public string ContainerScriptName { get; set; } = "QuestRewardContainer";

    /// <summary>
    /// Method for determining where to spawn the container.
    /// </summary>
    public ContainerSpawnLocation SpawnLocation { get; set; } = ContainerSpawnLocation.NearMarker;

    /// <summary>
    /// Reference for NearMarker or NearEntity spawn locations.
    /// For NearMarker: marker name (e.g., "MK_REWARD_SPOT")
    /// For NearEntity: entity script name (e.g., "QuestGiver")
    /// </summary>
    public string? SpawnReference { get; set; }

    /// <summary>
    /// X coordinate for FixedPosition spawn location.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Y coordinate (height) for FixedPosition spawn location.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Z coordinate for FixedPosition spawn location.
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// Collection of item definition names to place in the container.
    /// Items are added to container's inventory for player to collect.
    /// </summary>
    public ObservableCollection<string> Items { get; set; } = new();

    /// <summary>
    /// Whether to make the container glow green (quest highlight).
    /// Helps players locate the reward container in the world.
    /// </summary>
    public bool HighlightContainer { get; set; } = true;

    /// <summary>
    /// Whether to automatically give items when quest completes.
    /// If true, items are transferred directly without player interaction.
    /// If false, player must open the container manually.
    /// </summary>
    public bool AutoGiveOnComplete { get; set; } = true;
}

/// <summary>
/// Methods for determining container spawn location.
/// </summary>
public enum ContainerSpawnLocation
{
    /// <summary>Spawn near a named marker (SpawnReference = marker name)</summary>
    NearMarker,
    /// <summary>Spawn near an entity (SpawnReference = entity script name)</summary>
    NearEntity,
    /// <summary>Spawn at fixed world coordinates (X, Y, Z)</summary>
    FixedPosition
}

/// <summary>
/// Rewards that can be given when an object entity is interacted with (opened/used).
/// This allows any OBJECT_ type to function as a reward container.
/// </summary>
public sealed class ObjectReward
{
    /// <summary>
    /// Items to give when the object is interacted with
    /// </summary>
    public ObservableCollection<string> Items { get; set; } = new();

    /// <summary>
    /// Gold to give when the object is interacted with
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// Experience points to give when the object is interacted with
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Whether to destroy the object after giving rewards
    /// </summary>
    public bool DestroyAfterReward { get; set; } = true;

    /// <summary>
    /// Whether to show a message when giving rewards
    /// </summary>
    public bool ShowMessage { get; set; } = true;

    /// <summary>
    /// Custom message to display (if empty, uses default "You received...")
    /// </summary>
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Whether the object can only be used once (prevents repeated rewards)
    /// </summary>
    public bool OneTimeOnly { get; set; } = true;

    /// <summary>
    /// Check if this reward has any actual rewards configured
    /// </summary>
    public bool HasRewards => Items.Count > 0 || Gold > 0 || Experience > 0;
}
