using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.Models;

/// <summary>
/// Represents an entity (NPC, creature, object, or effect) managed by a quest.
///
/// QuestEntity defines both the spawn configuration and behavior of game entities.
/// The behavior is defined through a visual node graph (Nodes and Connections) that
/// gets compiled to Lua code by the CodeGenerator.
///
/// Entities can be spawned in multiple ways:
/// - AtMarker: Spawn at a named marker in the level
/// - AtPosition: Spawn at specific X/Y/Z coordinates
/// - OnEntity: Spawn attached to another entity
/// - CreateCreature: Use the game's creature creation system
/// - BindExisting: Attach to an existing entity in the level
/// </summary>
/// <remarks>
/// The ScriptName is used to reference this entity in Lua code and must be unique
/// within the quest. The DefName specifies the entity template from the game's
/// definition files (e.g., "CREATURE_VILLAGER_FARMER", "OBJECT_CHEST").
/// </remarks>
public sealed partial class QuestEntity : ObservableObject
{
    /// <summary>
    /// Unique identifier for this entity within the project.
    /// Auto-generated GUID used internally for tracking.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("Id")]
    private string id = string.Empty;

    /// <summary>
    /// Script name used to reference this entity in Lua code.
    /// Must be unique within the quest and a valid Lua identifier.
    /// This becomes the variable name in generated code.
    /// </summary>
    /// <example>"QuestGiver", "TargetBandit", "RewardChest"</example>
    [ObservableProperty]
    [property: JsonPropertyName("ScriptName")]
    private string scriptName = string.Empty;

    /// <summary>
    /// Definition name from Fable's entity definitions.
    /// Determines the entity's appearance, stats, and base behaviors.
    /// </summary>
    /// <example>"CREATURE_VILLAGER_FARMER", "CREATURE_BANDIT", "OBJECT_CHEST"</example>
    [ObservableProperty]
    [property: JsonPropertyName("DefName")]
    private string defName = string.Empty;

    /// <summary>
    /// The type category of this entity.
    /// Affects available spawn methods and applicable behaviors.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("EntityType")]
    private EntityType entityType = EntityType.Creature;

    /// <summary>
    /// Whether to take exclusive control of this entity.
    /// When true, other systems cannot modify the entity's behavior.
    /// Use for quest-critical NPCs that must follow scripted actions.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("ExclusiveControl")]
    private bool exclusiveControl;

    /// <summary>
    /// Whether to acquire control of the entity when the quest starts.
    /// Typically true - allows quest scripts to command the entity.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("AcquireControl")]
    private bool acquireControl = true;

    /// <summary>
    /// Whether to enable behavioral AI for this entity.
    /// When true, the entity can be commanded through Entity:* API calls.
    /// Required for movement, combat, and interaction behaviors.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("MakeBehavioral")]
    private bool makeBehavioral = true;

    /// <summary>
    /// Whether this entity cannot take damage.
    /// Useful for important NPCs that must survive quest events.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("Invulnerable")]
    private bool invulnerable;

    /// <summary>
    /// Whether this entity cannot be killed (HP cannot reach zero).
    /// Different from Invulnerable - entity can still take damage.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("Unkillable")]
    private bool unkillable;

    /// <summary>
    /// Whether this entity persists across level transitions.
    /// Persistent entities remain when leaving and re-entering the area.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("Persistent")]
    private bool persistent;

    /// <summary>
    /// Whether to destroy this entity when the level unloads.
    /// Useful for temporary quest entities that shouldn't persist.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("KillOnLevelUnload")]
    private bool killOnLevelUnload;

    /// <summary>
    /// When true, entity will glow green when targeted (like quest NPCs in base game).
    /// Uses Quest:SetThingHasInformation() API.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("IsQuestTarget")]
    private bool isQuestTarget;

    /// <summary>
    /// When true and IsQuestTarget is true, a minimap marker will be added for this entity.
    /// Uses Quest:MiniMapAddMarker() API.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("ShowOnMinimap")]
    private bool showOnMinimap;

    /// <summary>
    /// Rewards to give when this object entity is interacted with.
    /// This allows any OBJECT_ type to function as a reward-giving container.
    /// Only applicable when EntityType is Object.
    /// </summary>
    [JsonPropertyName("ObjectReward")]
    public ObjectReward? ObjectReward { get; set; }

    /// <summary>
    /// Method used to spawn this entity into the game world.
    /// Determines which spawn parameters (marker, position, etc.) are used.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("SpawnMethod")]
    private SpawnMethod spawnMethod = SpawnMethod.AtMarker;

    /// <summary>
    /// Region/level where this entity should be spawned.
    /// Must match a valid Fable region name (e.g., "LOOKOUT_POINT").
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("SpawnRegion")]
    private string spawnRegion = string.Empty;

    /// <summary>
    /// Marker name for AtMarker spawn method.
    /// Markers are predefined positions in Fable levels (e.g., "MK_OVID_DAD").
    /// Entity spawns at the marker's position and rotation.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("SpawnMarker")]
    private string spawnMarker = "MK_OVID_DAD";

    /// <summary>
    /// X coordinate for AtPosition spawn method.
    /// World-space position in Fable's coordinate system.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("SpawnX")]
    private float spawnX;

    /// <summary>
    /// Y coordinate (height) for AtPosition spawn method.
    /// World-space position in Fable's coordinate system.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("SpawnY")]
    private float spawnY;

    /// <summary>
    /// Z coordinate for AtPosition spawn method.
    /// World-space position in Fable's coordinate system.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("SpawnZ")]
    private float spawnZ;

    /// <summary>
    /// Collection of behavior nodes that define this entity's script.
    /// Nodes are visual programming blocks that get compiled to Lua code.
    /// </summary>
    [JsonPropertyName("Nodes")]
    public List<BehaviorNode> Nodes { get; set; } = new();

    /// <summary>
    /// Collection of variables scoped to this entity's script.
    /// These are emitted as local Lua variables in the generated entity script.
    /// </summary>
    [JsonPropertyName("Variables")]
    public ObservableCollection<EntityVariable> Variables { get; set; } = new();

    /// <summary>
    /// Collection of connections between behavior nodes.
    /// Defines the execution flow of the entity's behavior script.
    /// </summary>
    [JsonPropertyName("Connections")]
    public List<NodeConnection> Connections { get; set; } = new();

    partial void OnScriptNameChanged(string value)
    {
        // Notify that we need to update tab title
        OnPropertyChanged(nameof(ScriptName));
    }
}

/// <summary>
/// Categories of game entities that can be managed by quests.
/// </summary>
public enum EntityType
{
    /// <summary>Living entities: NPCs, enemies, animals</summary>
    Creature,
    /// <summary>Interactive objects: chests, doors, levers</summary>
    Object,
    /// <summary>Visual effects: particles, lights, sounds</summary>
    Effect,
    /// <summary>Light sources for scene illumination</summary>
    Light
}

/// <summary>
/// Methods for spawning entities into the game world.
/// </summary>
public enum SpawnMethod
{
    /// <summary>Spawn at a named marker position in the level</summary>
    AtMarker,
    /// <summary>Spawn at specific X/Y/Z world coordinates</summary>
    AtPosition,
    /// <summary>Spawn attached to another entity (parent-child relationship)</summary>
    OnEntity,
    /// <summary>Use Fable's native creature creation system</summary>
    CreateCreature,
    /// <summary>Bind to an existing entity already in the level (don't spawn new)</summary>
    BindExisting
}
