using System.Collections.Generic;

namespace FableQuestTool.Models;

public sealed class QuestEntity
{
    public string Id { get; set; } = string.Empty;
    public string ScriptName { get; set; } = string.Empty;
    public string DefName { get; set; } = string.Empty;
    public EntityType EntityType { get; set; } = EntityType.Creature;

    public bool ExclusiveControl { get; set; }
    public bool AcquireControl { get; set; } = true;
    public bool MakeBehavioral { get; set; } = true;
    public bool Invulnerable { get; set; }
    public bool Unkillable { get; set; }
    public bool Persistent { get; set; }
    public bool KillOnLevelUnload { get; set; }

    public SpawnMethod SpawnMethod { get; set; } = SpawnMethod.BindExisting;
    public string SpawnRegion { get; set; } = string.Empty;
    public string SpawnMarker { get; set; } = string.Empty;
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float SpawnZ { get; set; }

    public List<BehaviorNode> Nodes { get; set; } = new();
    public List<NodeConnection> Connections { get; set; } = new();
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
