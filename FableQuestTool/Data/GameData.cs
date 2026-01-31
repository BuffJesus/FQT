using System.Collections.ObjectModel;

namespace FableQuestTool.Data;

public static class GameData
{
    public static ObservableCollection<string> Regions { get; } = new()
    {
        "BarrowFields",
        "Oakvale",
        "LookoutPoint",
        "HeroGuild",
        "GuildWoods",
        "Bowerstone",
        "BowerstoneNorth",
        "BowerstoneSouth",
        "BowerstoneQuay",
        "Darkwood",
        "Witchwood",
        "Greatwood",
        "GreatwoodLake",
        "Knothole",
        "HookCoast",
        "TwinbladeCamp",
        "LychfieldGraveyard",
        "HeadsmansHill",
        "PrisonPath",
        "Snowspire",
        "NorthernWastes"
    };

    public static ObservableCollection<string> QuestCards { get; } = new()
    {
        "OBJECT_QUEST_CARD_GENERIC",
        "OBJECT_QUEST_CARD_WASP_MENACE",
        "OBJECT_QUEST_CARD_GHOST_GRANNY_NECKLACE",
        "OBJECT_QUEST_CARD_HERO_SOULS_MOTHER",
        "OBJECT_QUEST_CARD_BOUNTY_HUNT",
        "OBJECT_QUEST_CARD_ARENA",
        "OBJECT_QUEST_CARD_LOST_TRADER"
    };

    public static ObservableCollection<string> Objects { get; } = new()
    {
        "OBJECT_CHOCOLATE_BOX_01",
        "OBJECT_TEDDY_BEAR",
        "OBJECT_HEALTH_POTION",
        "OBJECT_WILL_POTION",
        "OBJECT_SILVER_KEY",
        "OBJECT_WEDDING_RING",
        "OBJECT_ROSE"
    };

    public static ObservableCollection<string> Abilities { get; } = new()
    {
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10",
        "11",
        "12",
        "13",
        "14",
        "15",
        "16",
        "17",
        "18",
        "19"
    };
}
