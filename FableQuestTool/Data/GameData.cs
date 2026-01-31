using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FableQuestTool.Data;

public static class GameData
{
    /// <summary>
    /// Region names used by the game. IMPORTANT: Region names must match exactly what the game loads.
    /// For childhood Oakvale at game start, use "StartOakVale" (NOT "Oakvale").
    /// </summary>
    public static ObservableCollection<string> Regions { get; } = new()
    {
        // Childhood regions (game start)
        "StartOakVale",           // Childhood Oakvale (has East, West, MemorialGarden sections)

        // Main game regions
        "BarrowFields",
        "LookoutPoint",
        "HeroGuild",
        "GuildWoods",
        "Bowerstone",
        "BowerstoneNorth",
        "BowerstoneSouth",
        "BowerstoneQuay",
        "Darkwood",
        "DarkwoodMarshes",
        "DarkwoodLake",
        "DarkwoodCamp",
        "Witchwood",
        "WitchwoodCullis",
        "WitchwoodStones",
        "WitchwoodLake",
        "Greatwood",
        "GreatwoodEntrance",
        "GreatwoodGorge",
        "GreatwoodLake",
        "GreatwoodCaves",
        "Knothole",
        "KnotholeGlade",
        "HookCoast",
        "TwinbladeCamp",
        "TwinbladeElite",
        "LychfieldGraveyard",
        "HeadsmansHill",
        "HeadsmansHollows",
        "HeadsmansWall",
        "PrisonPath",
        "Snowspire",
        "SnowspireVillage",
        "NorthernWastes",
        "NorthernWastesFoothill",
        "Archons",
        "ArchonsShrine",
        "NecropS",
        "CliffTop",
        "Orchard",
        "Gibbet",
        "Grey",
        "Temple",
        "Chamber"
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
        // Potions
        "OBJECT_HEALTH_POTION",
        "OBJECT_WILL_POTION",
        "OBJECT_RESURRECTION_PHIAL",

        // Keys
        "OBJECT_SILVER_KEY",
        "OBJECT_BRONZE_KEY",
        "OBJECT_IRON_KEY",

        // Weapons - Melee
        "OBJECT_IRON_SWORD",
        "OBJECT_STEEL_SWORD",
        "OBJECT_MASTER_SWORD",
        "OBJECT_IRON_CLEAVER",
        "OBJECT_STEEL_CLEAVER",
        "OBJECT_MASTER_CLEAVER",
        "OBJECT_IRON_MACE",
        "OBJECT_STEEL_MACE",
        "OBJECT_MASTER_MACE",
        "OBJECT_IRON_AXE",
        "OBJECT_STEEL_AXE",
        "OBJECT_MASTER_AXE",
        "OBJECT_IRON_PICKHAMMER",
        "OBJECT_STEEL_PICKHAMMER",
        "OBJECT_MASTER_PICKHAMMER",
        "OBJECT_SOLUS_GREATSWORD",
        "OBJECT_URCHIN_SMASHER",
        "OBJECT_SWORD_OF_AEONS",

        // Weapons - Ranged
        "OBJECT_YEW_LONGBOW",
        "OBJECT_OAK_LONGBOW",
        "OBJECT_EBONY_LONGBOW",
        "OBJECT_YEW_CROSSBOW",
        "OBJECT_OAK_CROSSBOW",
        "OBJECT_EBONY_CROSSBOW",
        "OBJECT_SKORMS_BOW",

        // Armor
        "OBJECT_LEATHER_ARMOR",
        "OBJECT_CHAINMAIL_ARMOR",
        "OBJECT_PLATE_ARMOR",
        "OBJECT_DARK_ARMOR",
        "OBJECT_BRIGHT_ARMOR",

        // Gifts & Items
        "OBJECT_CHOCOLATE_BOX_01",
        "OBJECT_TEDDY_BEAR",
        "OBJECT_WEDDING_RING",
        "OBJECT_ROSE",
        "OBJECT_PERFUME",
        "OBJECT_RED_ROSE",
        "OBJECT_BUNCH_OF_ROSES",
        "OBJECT_DIAMOND",

        // Food
        "OBJECT_APPLE",
        "OBJECT_CARROT",
        "OBJECT_RED_MEAT",
        "OBJECT_TOFU",
        "OBJECT_CRUNCHY_CHICK",

        // Quest Items
        "OBJECT_LADIES_NECKLACE",
        "OBJECT_BRONZE_TROPHY",
        "OBJECT_SILVER_TROPHY",
        "OBJECT_GOLD_TROPHY",

        // Misc
        "OBJECT_GOLD_01",
        "OBJECT_GOLD_02",
        "OBJECT_GOLD_03",
        "OBJECT_GOLD_BAG_01",
        "OBJECT_GUILD_SEAL"
    };

    public static ObservableCollection<string> Abilities { get; } = new()
    {
        "1 - Blade",
        "2 - Battle Charge",
        "3 - Berserk",
        "4 - Lightning",
        "5 - Multi Strike",
        "6 - Multi Arrow",
        "7 - Physical Shield",
        "8 - Slow Time",
        "9 - Enflame",
        "10 - Fireball",
        "11 - Force Push",
        "12 - Drain Life",
        "13 - Summon",
        "14 - Turncoat",
        "15 - Ghost Swords",
        "16 - Assassin Rush",
        "17 - Divine Fury",
        "18 - Multi Shot",
        "19 - Ages of Might",
        "20 - Ages of Skill",
        "21 - Ages of Will"
    };

    // Ability ID to name mapping
    public static Dictionary<int, string> AbilityNames { get; } = new()
    {
        {1, "Blade"},
        {2, "Battle Charge"},
        {3, "Berserk"},
        {4, "Lightning"},
        {5, "Multi Strike"},
        {6, "Multi Arrow"},
        {7, "Physical Shield"},
        {8, "Slow Time"},
        {9, "Enflame"},
        {10, "Fireball"},
        {11, "Force Push"},
        {12, "Drain Life"},
        {13, "Summon"},
        {14, "Turncoat"},
        {15, "Ghost Swords"},
        {16, "Assassin Rush"},
        {17, "Divine Fury"},
        {18, "Multi Shot"},
        {19, "Ages of Might"},
        {20, "Ages of Skill"},
        {21, "Ages of Will"}
    };

    public static string GetAbilityName(int id)
    {
        return AbilityNames.TryGetValue(id, out var name) ? name : $"Ability {id}";
    }

    public static int? ParseAbilityId(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Try to parse "1 - Blade" format
        var parts = text.Split('-');
        if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int id))
        {
            return id;
        }

        // Try to parse direct number
        if (int.TryParse(text.Trim(), out int directId))
        {
            return directId;
        }

        return null;
    }
}
