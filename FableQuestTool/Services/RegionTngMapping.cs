using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FableQuestTool.Services;

/// <summary>
/// Maps region names to their corresponding TNG files.
/// </summary>
public static class RegionTngMapping
{
    private static readonly Dictionary<string, string[]> RegionToTngFiles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        // Core regions
        { "BarrowFields", new[] { "Barrowfields_3.tng" } },
        { "Greatwood", new[] { "Greatwood_3.tng" } },
        { "Oakvale", new[] { "StartOakValeWest.tng", "OakValeEast_v2.tng" } },
        { "Lookout", new[] { "LookoutPoint_3.tng" } },
        { "HeroesGuild", new[] { "GuildInt.tng", "Guild_Exterior.tng" } },
        { "Bowerstone", new[] { "Bowerstone_North.tng", "Bowerstone_South.tng", "Bowerstone_Quay.tng" } },
        { "Darkwood", new[] { "Darkwood_1.tng", "DarkwoodCamp.tng" } },
        { "Witchwood", new[] { "WOOD.tng", "WitchwoodStones.tng" } },
        { "RoseQuest", new[] { "Darkwood_TheatreInterior.tng", "Darkwood_CaveInterior.tng" } },
        { "Knothole", new[] { "KnotholeGlade_3.tng" } },
        { "Headsman", new[] { "Headsmans_Hill.tng" } },
        { "NorthernWastes", new[] { "Northern_Wastes.tng" } },
        { "Snowspire", new[] { "SnowSpire_ext.tng", "SnowSpire_Int.tng" } },
        { "OracleQuest", new[] { "OracleQuest.tng" } },
        { "Hook", new[] { "Hook_Coast.tng" } },
        { "TwinBlade", new[] { "TwinBlade_Camp.tng", "TwinBladeCampLower.tng" } },
        { "OrchardFarm", new[] { "OrchardFarm.tng" } },
        { "RoseGarden", new[] { "Rose's_Garden.tng" } },
        { "GreatWoodGorge", new[] { "GreatWoodGorge.tng" } },
        { "CliffCottage", new[] { "CliffCottage.tng" } },
        { "FishingChallenge", new[] { "Fishing_Challenge.tng" } },
        { "LychField", new[] { "LychfieldGroveyard.tng", "ChapelOfSkorm_Int.tng" } },
        { "ArchonShrine", new[] { "ArchonsShrine_Int.tng" } },
        { "Prison", new[] { "Prison_Cellar.tng", "PrisonPath_ext.tng" } },
        { "Bowerstone_South", new[] { "Bowerstone_South.tng" } },
        { "Bowerstone_North", new[] { "Bowerstone_North.tng" } },
        { "Bowerstone_Quay", new[] { "Bowerstone_Quay.tng" } },
        { "PickAxe", new[] { "PickAxe.tng" } },
        { "Gibbet", new[] { "Gibbet.tng" } },
        { "PrisonEscape", new[] { "Prison_Cellar.tng" } },
        { "SouthBeach", new[] { "SouthBeach.tng" } },
        { "LostBay", new[] { "LostBay.tng" } },
        { "Lighthouse", new[] { "Lighthouse.tng" } },
        { "Ancient", new[] { "Ancient_Cullis_Gate.tng" } }
    };

    /// <summary>
    /// Gets all TNG file paths for a given region name.
    /// </summary>
    public static List<string> GetTngFilesForRegion(string regionName, string fablePath)
    {
        List<string> paths = new List<string>();

        if (!RegionToTngFiles.TryGetValue(regionName, out string[]? tngFiles))
        {
            return paths;
        }

        string levelsPath = Path.Combine(fablePath, "data", "Levels", "FinalAlbion");

        foreach (string tngFile in tngFiles)
        {
            string fullPath = Path.Combine(levelsPath, tngFile);
            if (File.Exists(fullPath))
            {
                paths.Add(fullPath);
            }
        }

        return paths;
    }

    /// <summary>
    /// Gets all region names that have TNG mappings.
    /// </summary>
    public static List<string> GetAllRegionNames()
    {
        return RegionToTngFiles.Keys.OrderBy(k => k).ToList();
    }

    /// <summary>
    /// Checks if a region has TNG mappings.
    /// </summary>
    public static bool HasRegionMapping(string regionName)
    {
        return RegionToTngFiles.ContainsKey(regionName);
    }

    /// <summary>
    /// Scans the FinalAlbion directory for all TNG files and returns them.
    /// Used as fallback when region mapping doesn't exist.
    /// </summary>
    public static List<string> GetAllTngFiles(string fablePath)
    {
        string levelsPath = Path.Combine(fablePath, "data", "Levels", "FinalAlbion");

        if (!Directory.Exists(levelsPath))
        {
            return new List<string>();
        }

        try
        {
            return Directory.GetFiles(levelsPath, "*.tng", SearchOption.TopDirectoryOnly).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Attempts to infer region name from TNG file name.
    /// </summary>
    public static string InferRegionFromTngFileName(string tngFileName)
    {
        string name = Path.GetFileNameWithoutExtension(tngFileName);

        // Remove common suffixes
        name = name.Replace("_3", "").Replace("_v2", "").Replace("_ext", "").Replace("_Int", "");

        // Remove "Start" prefix
        if (name.StartsWith("Start", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(5);
        }

        // Handle special cases
        if (name.Contains("Bowerstone", StringComparison.OrdinalIgnoreCase))
        {
            if (name.Contains("North", StringComparison.OrdinalIgnoreCase))
                return "Bowerstone_North";
            if (name.Contains("South", StringComparison.OrdinalIgnoreCase))
                return "Bowerstone_South";
            if (name.Contains("Quay", StringComparison.OrdinalIgnoreCase))
                return "Bowerstone_Quay";
            return "Bowerstone";
        }

        if (name.Contains("OakVale", StringComparison.OrdinalIgnoreCase))
            return "Oakvale";

        if (name.Contains("Guild", StringComparison.OrdinalIgnoreCase))
            return "HeroesGuild";

        return name;
    }
}
