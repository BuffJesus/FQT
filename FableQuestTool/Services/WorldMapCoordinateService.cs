using System;
using System.Collections.Generic;

namespace FableQuestTool.Services;

/// <summary>
/// Service for converting world coordinates to world map pixel offsets.
/// The world map uses pixel coordinates, not world coordinates.
/// This service provides mapping between regions/world positions and map pixels.
/// </summary>
public static class WorldMapCoordinateService
{
    /// <summary>
    /// Region coordinate data containing world bounds and map pixel mapping.
    /// </summary>
    public sealed class RegionMapData
    {
        /// <summary>Map pixel X for the region center.</summary>
        public int MapCenterX { get; init; }
        
        /// <summary>Map pixel Y for the region center.</summary>
        public int MapCenterY { get; init; }
        
        /// <summary>Approximate world X coordinate at region center.</summary>
        public float WorldCenterX { get; init; }
        
        /// <summary>Approximate world Z coordinate at region center (Y on map).</summary>
        public float WorldCenterZ { get; init; }
        
        /// <summary>Scale factor: map pixels per world unit (X axis).</summary>
        public float ScaleX { get; init; }
        
        /// <summary>Scale factor: map pixels per world unit (Z axis / Y on map).</summary>
        public float ScaleZ { get; init; }
        
        /// <summary>Approximate radius of the region in world units.</summary>
        public float WorldRadius { get; init; }
    }

    /// <summary>
    /// Mapping of region names to their coordinate data.
    /// Values are approximations based on Fable's world map layout.
    /// World map is approximately 512x512 pixels.
    /// </summary>
    private static readonly Dictionary<string, RegionMapData> RegionData = new(StringComparer.OrdinalIgnoreCase)
    {
        // Northern regions
        { "Snowspire", new RegionMapData { MapCenterX = 420, MapCenterY = 60, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 100 } },
        { "NorthernWastes", new RegionMapData { MapCenterX = 350, MapCenterY = 90, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 150 } },
        { "ArchonShrine", new RegionMapData { MapCenterX = 280, MapCenterY = 110, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        
        // Hook Coast area
        { "Hook", new RegionMapData { MapCenterX = 450, MapCenterY = 150, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "Hook_Coast", new RegionMapData { MapCenterX = 450, MapCenterY = 150, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "Lighthouse", new RegionMapData { MapCenterX = 480, MapCenterY = 170, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        
        // Central-North regions
        { "LychField", new RegionMapData { MapCenterX = 200, MapCenterY = 150, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "LychfieldGraveyard", new RegionMapData { MapCenterX = 200, MapCenterY = 150, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "Headsman", new RegionMapData { MapCenterX = 150, MapCenterY = 170, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        { "Headsmans_Hill", new RegionMapData { MapCenterX = 150, MapCenterY = 170, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        { "Gibbet", new RegionMapData { MapCenterX = 100, MapCenterY = 190, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "Prison", new RegionMapData { MapCenterX = 70, MapCenterY = 210, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        
        // Witchwood area
        { "Witchwood", new RegionMapData { MapCenterX = 320, MapCenterY = 180, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 100 } },
        { "WitchwoodStones", new RegionMapData { MapCenterX = 340, MapCenterY = 190, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "Knothole", new RegionMapData { MapCenterX = 380, MapCenterY = 200, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 70 } },
        { "KnotholeGlade", new RegionMapData { MapCenterX = 380, MapCenterY = 200, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 70 } },
        
        // Central regions - Bowerstone area
        { "Bowerstone", new RegionMapData { MapCenterX = 250, MapCenterY = 250, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 100 } },
        { "Bowerstone_North", new RegionMapData { MapCenterX = 250, MapCenterY = 230, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "Bowerstone_South", new RegionMapData { MapCenterX = 250, MapCenterY = 270, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "Bowerstone_Quay", new RegionMapData { MapCenterX = 280, MapCenterY = 260, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "Bowerstone_Jail", new RegionMapData { MapCenterX = 240, MapCenterY = 240, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 30 } },
        
        // Lookout Point (near Bowerstone)
        { "Lookout", new RegionMapData { MapCenterX = 300, MapCenterY = 240, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "LookoutPoint", new RegionMapData { MapCenterX = 300, MapCenterY = 240, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        
        // Heroes' Guild
        { "HeroesGuild", new RegionMapData { MapCenterX = 200, MapCenterY = 290, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 70 } },
        { "Guild", new RegionMapData { MapCenterX = 200, MapCenterY = 290, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 70 } },
        { "GuildWoods", new RegionMapData { MapCenterX = 180, MapCenterY = 300, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        
        // Darkwood area
        { "Darkwood", new RegionMapData { MapCenterX = 150, MapCenterY = 320, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 120 } },
        { "DarkwoodCamp", new RegionMapData { MapCenterX = 130, MapCenterY = 340, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "DarkwoodMarshes", new RegionMapData { MapCenterX = 170, MapCenterY = 330, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        { "Ancient", new RegionMapData { MapCenterX = 120, MapCenterY = 280, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "AncientCullisGate", new RegionMapData { MapCenterX = 120, MapCenterY = 280, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        
        // BarrowFields - central hub
        { "BarrowFields", new RegionMapData { MapCenterX = 300, MapCenterY = 320, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 100 } },
        
        // Oakvale area
        { "Oakvale", new RegionMapData { MapCenterX = 380, MapCenterY = 340, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "StartOakVale", new RegionMapData { MapCenterX = 380, MapCenterY = 340, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "CliffCottage", new RegionMapData { MapCenterX = 420, MapCenterY = 360, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "FishingChallenge", new RegionMapData { MapCenterX = 350, MapCenterY = 370, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 30 } },
        { "MemorialGarden", new RegionMapData { MapCenterX = 400, MapCenterY = 350, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 30 } },
        
        // TwinBlade camp
        { "TwinBlade", new RegionMapData { MapCenterX = 180, MapCenterY = 370, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        { "TwinBladeCamp", new RegionMapData { MapCenterX = 180, MapCenterY = 370, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 80 } },
        
        // Greatwood area
        { "Greatwood", new RegionMapData { MapCenterX = 250, MapCenterY = 380, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 100 } },
        { "GreatWoodEntrance", new RegionMapData { MapCenterX = 270, MapCenterY = 360, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "GreatWoodGorge", new RegionMapData { MapCenterX = 220, MapCenterY = 400, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        { "GreatWoodLake", new RegionMapData { MapCenterX = 230, MapCenterY = 390, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "GreatWoodCaves", new RegionMapData { MapCenterX = 240, MapCenterY = 410, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "OrchardFarm", new RegionMapData { MapCenterX = 280, MapCenterY = 420, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "RoseGarden", new RegionMapData { MapCenterX = 320, MapCenterY = 410, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "Roses_Garden", new RegionMapData { MapCenterX = 320, MapCenterY = 410, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        
        // Southern regions
        { "PicnicArea", new RegionMapData { MapCenterX = 350, MapCenterY = 430, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "PickAxe", new RegionMapData { MapCenterX = 180, MapCenterY = 430, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "SouthBeach", new RegionMapData { MapCenterX = 400, MapCenterY = 460, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        { "LostBay", new RegionMapData { MapCenterX = 450, MapCenterY = 450, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        
        // Special quest areas
        { "RoseQuest", new RegionMapData { MapCenterX = 150, MapCenterY = 350, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "OracleQuest", new RegionMapData { MapCenterX = 300, MapCenterY = 150, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "Arena", new RegionMapData { MapCenterX = 380, MapCenterY = 280, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 60 } },
        { "ChapelOfSkorm", new RegionMapData { MapCenterX = 180, MapCenterY = 160, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        { "TempleOfAvo", new RegionMapData { MapCenterX = 340, MapCenterY = 200, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 40 } },
        
        // First areas (tutorial regions)
        { "First2", new RegionMapData { MapCenterX = 200, MapCenterY = 290, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
        { "First3", new RegionMapData { MapCenterX = 200, MapCenterY = 290, WorldCenterX = 0, WorldCenterZ = 0, ScaleX = 0.01f, ScaleZ = 0.01f, WorldRadius = 50 } },
    };

    /// <summary>
    /// Gets the map pixel offset for a region's center point.
    /// </summary>
    /// <param name="regionName">Name of the region</param>
    /// <returns>Tuple of (X, Y) pixel offsets, or null if region not found</returns>
    public static (int X, int Y)? GetRegionCenterOffset(string regionName)
    {
        if (string.IsNullOrWhiteSpace(regionName))
            return null;

        if (RegionData.TryGetValue(regionName, out var data))
        {
            return (data.MapCenterX, data.MapCenterY);
        }

        return null;
    }

    /// <summary>
    /// Gets the map pixel offset for a specific world position within a region.
    /// </summary>
    /// <param name="regionName">Name of the region</param>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldZ">World Z coordinate (maps to Y on the 2D map)</param>
    /// <returns>Tuple of (X, Y) pixel offsets, or region center if position can't be mapped</returns>
    public static (int X, int Y)? GetMapOffsetForPosition(string regionName, float worldX, float worldZ)
    {
        if (string.IsNullOrWhiteSpace(regionName))
            return null;

        if (!RegionData.TryGetValue(regionName, out var data))
            return null;

        // For now, return the region center since we don't have accurate world coordinate data
        // In the future, this could use actual world bounds from LEV files to calculate precise offsets
        // The formula would be:
        // int offsetX = data.MapCenterX + (int)((worldX - data.WorldCenterX) * data.ScaleX);
        // int offsetY = data.MapCenterY + (int)((worldZ - data.WorldCenterZ) * data.ScaleZ);
        
        return (data.MapCenterX, data.MapCenterY);
    }

    /// <summary>
    /// Gets the map pixel offset for the first entity's spawn location in the quest.
    /// Falls back to region center if entity position is not available.
    /// </summary>
    /// <param name="primaryRegion">Primary quest region</param>
    /// <param name="entities">List of quest entities</param>
    /// <returns>Tuple of (X, Y) pixel offsets</returns>
    public static (int X, int Y) GetMapOffsetForQuest(string primaryRegion, IEnumerable<Models.QuestEntity>? entities)
    {
        // Default to region center
        var defaultOffset = GetRegionCenterOffset(primaryRegion) ?? (256, 256); // Map center as fallback

        if (entities == null)
            return defaultOffset;

        // Find the first spawned entity (not bound existing) with a valid region
        foreach (var entity in entities)
        {
            if (entity == null)
            {
                continue;
            }

            if (entity.SpawnMethod == Models.SpawnMethod.BindExisting)
                continue;

            string region = !string.IsNullOrWhiteSpace(entity.SpawnRegion) 
                ? entity.SpawnRegion 
                : primaryRegion;

            // If entity has position data, try to use it
            if (entity.SpawnMethod == Models.SpawnMethod.AtPosition)
            {
                var posOffset = GetMapOffsetForPosition(region, entity.SpawnX, entity.SpawnZ);
                if (posOffset.HasValue)
                    return posOffset.Value;
            }

            // Otherwise use region center
            var regionOffset = GetRegionCenterOffset(region);
            if (regionOffset.HasValue)
                return regionOffset.Value;
        }

        return defaultOffset;
    }

    /// <summary>
    /// Gets all available region names.
    /// </summary>
    public static IEnumerable<string> GetAllRegions()
    {
        return RegionData.Keys;
    }

    /// <summary>
    /// Checks if a region has coordinate mapping data.
    /// </summary>
    public static bool HasRegionData(string regionName)
    {
        return !string.IsNullOrWhiteSpace(regionName) && RegionData.ContainsKey(regionName);
    }
}
