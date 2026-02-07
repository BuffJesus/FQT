using FableQuestTool.Models;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class TngEntityTests
{
    [Fact]
    public void Category_DetectsCreaturesAndNpcs()
    {
        TngEntity npc = new TngEntity { DefinitionType = "CREATURE_VILLAGER_FARMER" };
        TngEntity creature = new TngEntity { DefinitionType = "CREATURE_BANDIT" };

        Assert.Equal(EntityCategory.NPC, npc.Category);
        Assert.Equal(EntityCategory.Creature, creature.Category);
    }

    [Fact]
    public void Category_DetectsObjectsAndMarkers()
    {
        TngEntity chest = new TngEntity { DefinitionType = "OBJECT_CHEST_WOOD" };
        TngEntity door = new TngEntity { DefinitionType = "OBJECT_DOOR_WOOD" };
        TngEntity questItem = new TngEntity { DefinitionType = "OBJECT_QUEST_CARD_GENERIC" };
        TngEntity obj = new TngEntity { DefinitionType = "OBJECT_BARREL" };
        TngEntity marker = new TngEntity { DefinitionType = "MARKER_START" };

        Assert.Equal(EntityCategory.Chest, chest.Category);
        Assert.Equal(EntityCategory.Door, door.Category);
        Assert.Equal(EntityCategory.QuestItem, questItem.Category);
        Assert.Equal(EntityCategory.Object, obj.Category);
        Assert.Equal(EntityCategory.Marker, marker.Category);
    }

    [Fact]
    public void Category_DetectsSpecialTypes()
    {
        TngEntity camera = new TngEntity { DefinitionType = "CAMERA_POINT_01" };
        TngEntity holySite = new TngEntity { DefinitionType = "HOLY_SITE_ALTAR" };
        TngEntity generator = new TngEntity { DefinitionType = "UNKNOWN", ThingType = "CreatureGenerator" };

        Assert.Equal(EntityCategory.CameraPoint, camera.Category);
        Assert.Equal(EntityCategory.HolySite, holySite.Category);
        Assert.Equal(EntityCategory.CreatureGenerator, generator.Category);
    }

    [Fact]
    public void Category_UnknownWhenDefinitionMissing()
    {
        TngEntity entity = new TngEntity();

        Assert.Equal(EntityCategory.Unknown, entity.Category);
    }
}
