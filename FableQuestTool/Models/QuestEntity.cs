using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.Models;

public sealed partial class QuestEntity : ObservableObject
{
    [ObservableProperty]
    [property: JsonPropertyName("Id")]
    private string id = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("ScriptName")]
    private string scriptName = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("DefName")]
    private string defName = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("EntityType")]
    private EntityType entityType = EntityType.Creature;

    [ObservableProperty]
    [property: JsonPropertyName("ExclusiveControl")]
    private bool exclusiveControl;

    [ObservableProperty]
    [property: JsonPropertyName("AcquireControl")]
    private bool acquireControl = true;

    [ObservableProperty]
    [property: JsonPropertyName("MakeBehavioral")]
    private bool makeBehavioral = true;

    [ObservableProperty]
    [property: JsonPropertyName("Invulnerable")]
    private bool invulnerable;

    [ObservableProperty]
    [property: JsonPropertyName("Unkillable")]
    private bool unkillable;

    [ObservableProperty]
    [property: JsonPropertyName("Persistent")]
    private bool persistent;

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

    [ObservableProperty]
    [property: JsonPropertyName("SpawnMethod")]
    private SpawnMethod spawnMethod = SpawnMethod.AtMarker;

    [ObservableProperty]
    [property: JsonPropertyName("SpawnRegion")]
    private string spawnRegion = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("SpawnMarker")]
    private string spawnMarker = "MK_OVID_DAD";

    [ObservableProperty]
    [property: JsonPropertyName("SpawnX")]
    private float spawnX;

    [ObservableProperty]
    [property: JsonPropertyName("SpawnY")]
    private float spawnY;

    [ObservableProperty]
    [property: JsonPropertyName("SpawnZ")]
    private float spawnZ;

    [JsonPropertyName("Nodes")]
    public List<BehaviorNode> Nodes { get; set; } = new();

    [JsonPropertyName("Connections")]
    public List<NodeConnection> Connections { get; set; } = new();

    partial void OnScriptNameChanged(string value)
    {
        // Notify that we need to update tab title
        OnPropertyChanged(nameof(ScriptName));
    }
}

public enum EntityType
{
    Creature,
    Object,
    Effect,
    Light
}

public enum SpawnMethod
{
    AtMarker,
    AtPosition,
    OnEntity,
    CreateCreature,
    BindExisting
}