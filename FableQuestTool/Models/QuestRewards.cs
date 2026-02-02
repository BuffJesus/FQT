using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace FableQuestTool.Models;

public sealed class QuestRewards
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public int Renown { get; set; }
    public float Morality { get; set; }

    // Simple single-item reward (limited by game engine)
    public string? DirectRewardItem { get; set; }

    // Container-based rewards (for multiple items)
    public ContainerReward? Container { get; set; }

    public ObservableCollection<string> Abilities { get; set; } = new();

    // Backward compatibility: old quest files have Items array
    // This property handles migration during deserialization
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ObservableCollection<string>? Items
    {
        get => null; // Never serialize this property
        set
        {
            // Migrate old Items to new format during deserialization
            if (value != null && value.Count > 0)
            {
                if (value.Count == 1)
                {
                    // Single item -> use DirectRewardItem
                    DirectRewardItem = value[0];
                }
                else
                {
                    // Multiple items -> use Container
                    Container ??= new ContainerReward();
                    foreach (string item in value)
                    {
                        Container.Items.Add(item);
                    }
                }
            }
        }
    }
}

public sealed class ContainerReward
{
    // Container object definition (e.g., "OBJECT_CHEST", "OBJECT_BARREL", or any OBJECT_*)
    public string ContainerDefName { get; set; } = "OBJECT_CHEST";

    // Script name for the container instance
    public string ContainerScriptName { get; set; } = "QuestRewardContainer";

    // Where to spawn the container
    public ContainerSpawnLocation SpawnLocation { get; set; } = ContainerSpawnLocation.NearMarker;

    // Marker/entity to spawn near (if SpawnLocation = NearMarker or NearEntity)
    public string? SpawnReference { get; set; }

    // Position (if SpawnLocation = FixedPosition)
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    // Items to add to the container
    public ObservableCollection<string> Items { get; set; } = new();

    // Whether to make container glow/highlighted
    public bool HighlightContainer { get; set; } = true;

    // Whether to auto-give items when quest completes or require player to open
    public bool AutoGiveOnComplete { get; set; } = true;
}

public enum ContainerSpawnLocation
{
    NearMarker,      // Spawn near a marker (SpawnReference = marker name)
    NearEntity,      // Spawn near an entity (SpawnReference = entity script name)
    FixedPosition    // Spawn at fixed X,Y,Z coordinates
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
