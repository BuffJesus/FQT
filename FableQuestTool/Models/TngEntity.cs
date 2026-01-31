using System;

namespace FableQuestTool.Models;

/// <summary>
/// Represents an entity extracted from a TNG (Thing) file.
/// </summary>
public sealed class TngEntity
{
    /// <summary>
    /// The entity type (e.g., "AICreature", "Object", "Marker").
    /// </summary>
    public string ThingType { get; set; } = string.Empty;

    /// <summary>
    /// The ScriptName property - used by FSE to reference the entity.
    /// </summary>
    public string ScriptName { get; set; } = string.Empty;

    /// <summary>
    /// The DefinitionType property - entity template type (e.g., "CREATURE_VILLAGER_FARMER").
    /// </summary>
    public string DefinitionType { get; set; } = string.Empty;

    /// <summary>
    /// The UID (Unique Identifier) as a string.
    /// </summary>
    public string Uid { get; set; } = string.Empty;

    /// <summary>
    /// Position X coordinate.
    /// </summary>
    public float PositionX { get; set; }

    /// <summary>
    /// Position Y coordinate.
    /// </summary>
    public float PositionY { get; set; }

    /// <summary>
    /// Position Z coordinate.
    /// </summary>
    public float PositionZ { get; set; }

    /// <summary>
    /// Whether this entity persists in the game world.
    /// </summary>
    public bool IsGamePersistent { get; set; }

    /// <summary>
    /// Whether this entity persists at the level.
    /// </summary>
    public bool IsLevelPersistent { get; set; }

    /// <summary>
    /// ScriptData property (often "NULL" or empty).
    /// </summary>
    public string ScriptData { get; set; } = string.Empty;

    /// <summary>
    /// The TNG file this entity was found in.
    /// </summary>
    public string SourceFile { get; set; } = string.Empty;

    /// <summary>
    /// The region/level name this entity belongs to.
    /// </summary>
    public string RegionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets whether this entity has a ScriptName (can be referenced by FSE).
    /// </summary>
    public bool HasScriptName => !string.IsNullOrWhiteSpace(ScriptName) && ScriptName != "NULL";

    /// <summary>
    /// Gets a short display name for this entity.
    /// </summary>
    public string DisplayName => HasScriptName ? ScriptName : $"[{DefinitionType}]";

    /// <summary>
    /// Gets the entity category based on DefinitionType.
    /// </summary>
    public EntityCategory Category
    {
        get
        {
            if (string.IsNullOrEmpty(DefinitionType))
                return EntityCategory.Unknown;

            string upper = DefinitionType.ToUpperInvariant();

            if (upper.StartsWith("CREATURE_"))
            {
                if (upper.Contains("VILLAGER") || upper.Contains("GUARD") || upper.Contains("TRADER"))
                    return EntityCategory.NPC;
                return EntityCategory.Creature;
            }

            if (upper.StartsWith("OBJECT_"))
            {
                if (upper.Contains("CHEST"))
                    return EntityCategory.Chest;
                if (upper.Contains("DOOR"))
                    return EntityCategory.Door;
                if (upper.Contains("QUEST"))
                    return EntityCategory.QuestItem;
                return EntityCategory.Object;
            }

            if (upper.StartsWith("MARKER_"))
                return EntityCategory.Marker;

            if (upper.StartsWith("CAMERA_POINT"))
                return EntityCategory.CameraPoint;

            if (upper.StartsWith("HOLY_SITE"))
                return EntityCategory.HolySite;

            if (ThingType == "CreatureGenerator")
                return EntityCategory.CreatureGenerator;

            return EntityCategory.Unknown;
        }
    }

    public override string ToString()
    {
        return $"{DisplayName} ({DefinitionType})";
    }
}

/// <summary>
/// Categories of entities for filtering.
/// </summary>
public enum EntityCategory
{
    Unknown,
    Creature,
    NPC,
    Enemy,
    Object,
    Chest,
    Door,
    QuestItem,
    Marker,
    CameraPoint,
    HolySite,
    CreatureGenerator
}
