using FableQuestTool.Config;
using FableQuestTool.Formats;
using FableQuestTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FableQuestTool.Services;

/// <summary>
/// Comprehensive catalog service that scans all game data (loose files + BIG archives)
/// to build complete lists of entities, markers, creatures, objects, etc.
/// </summary>
public sealed class GameDataCatalogService
{
    private readonly FableConfig config;
    private readonly LevelDataService levelDataService;
    private List<TngEntity>? cachedAllEntities;
    private Dictionary<string, List<TngEntity>>? cachedEntitiesByRegion;
    private DateTime? cacheTime;
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(10);

    public GameDataCatalogService(FableConfig config)
    {
        this.config = config;
        levelDataService = new LevelDataService(config);
    }

    /// <summary>
    /// Scans ALL TNGs (loose files + BIG archives) and builds complete entity catalog.
    /// This is the main method to get a comprehensive list of everything in the game.
    /// </summary>
    public List<TngEntity> BuildCompleteCatalog(bool forceRefresh = false)
    {
        if (!forceRefresh && cachedAllEntities != null && cacheTime.HasValue &&
            DateTime.Now - cacheTime.Value < cacheExpiration)
        {
            return cachedAllEntities;
        }

        var allEntities = new List<TngEntity>();

        // 1. Scan loose TNG files (from data/Levels/FinalAlbion/*.tng)
        var looseTngs = ScanLooseTngFiles();
        allEntities.AddRange(looseTngs);

        // 2. Scan TNG files inside BIG archives
        var archivedTngs = ScanBigArchiveTngs();
        allEntities.AddRange(archivedTngs);

        cachedAllEntities = allEntities;
        cacheTime = DateTime.Now;

        return allEntities;
    }

    /// <summary>
    /// Gets entities organized by region.
    /// </summary>
    public Dictionary<string, List<TngEntity>> GetEntitiesByRegion(bool forceRefresh = false)
    {
        if (!forceRefresh && cachedEntitiesByRegion != null)
        {
            return cachedEntitiesByRegion;
        }

        var allEntities = BuildCompleteCatalog(forceRefresh);
        var byRegion = new Dictionary<string, List<TngEntity>>(StringComparer.OrdinalIgnoreCase);

        foreach (var entity in allEntities)
        {
            string region = string.IsNullOrWhiteSpace(entity.RegionName) ? "Unknown" : entity.RegionName;

            if (!byRegion.ContainsKey(region))
            {
                byRegion[region] = new List<TngEntity>();
            }

            byRegion[region].Add(entity);
        }

        cachedEntitiesByRegion = byRegion;
        return byRegion;
    }

    /// <summary>
    /// Gets comprehensive statistics about the game data.
    /// </summary>
    public GameDataStatistics GetStatistics()
    {
        var allEntities = BuildCompleteCatalog();
        var stats = new GameDataStatistics();

        stats.TotalEntities = allEntities.Count;
        stats.EntitiesWithScriptName = allEntities.Count(e => e.HasScriptName);
        stats.MarkerCount = allEntities.Count(e => e.Category == EntityCategory.Marker);
        stats.NpcCount = allEntities.Count(e => e.Category == EntityCategory.NPC);
        stats.CreatureCount = allEntities.Count(e => e.Category == EntityCategory.Creature);
        stats.ObjectCount = allEntities.Count(e => e.Category == EntityCategory.Object);
        stats.ChestCount = allEntities.Count(e => e.Category == EntityCategory.Chest);
        stats.DoorCount = allEntities.Count(e => e.Category == EntityCategory.Door);
        stats.QuestItemCount = allEntities.Count(e => e.Category == EntityCategory.QuestItem);

        var byRegion = GetEntitiesByRegion();
        stats.RegionCount = byRegion.Count;

        return stats;
    }

    /// <summary>
    /// Gets a filtered list of entities by category and optional region.
    /// </summary>
    public List<TngEntity> GetFilteredEntities(EntityCategory? category = null, string? regionName = null, bool scriptableOnly = false)
    {
        List<TngEntity> entities;

        if (!string.IsNullOrWhiteSpace(regionName))
        {
            var byRegion = GetEntitiesByRegion();
            entities = byRegion.ContainsKey(regionName) ? byRegion[regionName] : new List<TngEntity>();
        }
        else
        {
            entities = BuildCompleteCatalog();
        }

        // Filter by scriptable
        if (scriptableOnly)
        {
            entities = entities.Where(e => e.HasScriptName).ToList();
        }

        // Filter by category
        if (category.HasValue && category.Value != EntityCategory.Unknown)
        {
            entities = entities.Where(e => e.Category == category.Value).ToList();
        }

        return entities;
    }

    /// <summary>
    /// Exports the complete catalog to a text file for reference.
    /// </summary>
    public void ExportCatalogToFile(string outputPath)
    {
        var allEntities = BuildCompleteCatalog();
        var byRegion = GetEntitiesByRegion();
        var stats = GetStatistics();

        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);

        writer.WriteLine("=== FABLE: THE LOST CHAPTERS - COMPLETE ENTITY CATALOG ===");
        writer.WriteLine($"Generated: {DateTime.Now}");
        writer.WriteLine();

        writer.WriteLine("=== STATISTICS ===");
        writer.WriteLine($"Total Entities: {stats.TotalEntities}");
        writer.WriteLine($"Scriptable Entities: {stats.EntitiesWithScriptName}");
        writer.WriteLine($"Regions: {stats.RegionCount}");
        writer.WriteLine($"Markers: {stats.MarkerCount}");
        writer.WriteLine($"NPCs: {stats.NpcCount}");
        writer.WriteLine($"Creatures: {stats.CreatureCount}");
        writer.WriteLine($"Objects: {stats.ObjectCount}");
        writer.WriteLine($"Chests: {stats.ChestCount}");
        writer.WriteLine($"Doors: {stats.DoorCount}");
        writer.WriteLine($"Quest Items: {stats.QuestItemCount}");
        writer.WriteLine();

        writer.WriteLine("=== ENTITIES BY REGION ===");
        foreach (var region in byRegion.Keys.OrderBy(k => k))
        {
            writer.WriteLine();
            writer.WriteLine($"--- {region} ({byRegion[region].Count} entities) ---");

            var scriptable = byRegion[region].Where(e => e.HasScriptName).OrderBy(e => e.ScriptName);
            foreach (var entity in scriptable)
            {
                writer.WriteLine($"  {entity.ScriptName,-40} | {entity.DefinitionType,-50} | {entity.Category}");
            }
        }
    }

    /// <summary>
    /// Scans loose TNG files from data/Levels/FinalAlbion/*.tng
    /// </summary>
    private List<TngEntity> ScanLooseTngFiles()
    {
        var entities = new List<TngEntity>();

        if (string.IsNullOrWhiteSpace(config.FablePath))
        {
            return entities;
        }

        string levelsPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion");
        if (!Directory.Exists(levelsPath))
        {
            return entities;
        }

        var tngFiles = Directory.GetFiles(levelsPath, "*.tng", SearchOption.TopDirectoryOnly);

        foreach (var tngFile in tngFiles)
        {
            string regionName = RegionTngMapping.InferRegionFromTngFileName(tngFile);
            var parsed = TngParser.ParseTngFile(tngFile, regionName);

            // Mark as loose file
            foreach (var entity in parsed)
            {
                entity.SourceFile = $"[LOOSE] {entity.SourceFile}";
            }

            entities.AddRange(parsed);
        }

        return entities;
    }

    /// <summary>
    /// Scans TNG files inside BIG archives (Graphics.big, CompiledLevel.big, etc.)
    /// </summary>
    private List<TngEntity> ScanBigArchiveTngs()
    {
        var entities = new List<TngEntity>();

        try
        {
            var tngEntries = levelDataService.FindTngEntries();

            foreach (var tngEntry in tngEntries)
            {
                // Extract TNG data from BIG archive
                var tngData = levelDataService.ExtractEntry(tngEntry);
                if (tngData == null || tngData.Length == 0)
                {
                    continue;
                }

                // Parse TNG from memory
                string tngText = Encoding.ASCII.GetString(tngData);
                var parsed = ParseTngFromText(tngText, tngEntry.SymbolName);

                // Mark as archived
                foreach (var entity in parsed)
                {
                    entity.SourceFile = $"[BIG] {tngEntry.SymbolName}";
                }

                entities.AddRange(parsed);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scanning BIG archives: {ex.Message}");
        }

        return entities;
    }

    /// <summary>
    /// Parses TNG data from text string (for in-memory parsing).
    /// </summary>
    private List<TngEntity> ParseTngFromText(string tngText, string fileName)
    {
        var entities = new List<TngEntity>();
        string regionName = RegionTngMapping.InferRegionFromTngFileName(fileName);

        string[] lines = tngText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        TngEntity? currentEntity = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("NewThing ", StringComparison.OrdinalIgnoreCase))
            {
                if (currentEntity != null)
                {
                    entities.Add(currentEntity);
                }

                currentEntity = new TngEntity
                {
                    SourceFile = fileName,
                    RegionName = regionName,
                    ThingType = line.Substring("NewThing ".Length).TrimEnd(';').Trim()
                };
            }
            else if (currentEntity != null && line == ";" && !string.IsNullOrEmpty(currentEntity.ThingType))
            {
                entities.Add(currentEntity);
                currentEntity = null;
            }
            else if (currentEntity != null)
            {
                // Parse properties (simplified)
                if (line.StartsWith("ScriptName ", StringComparison.OrdinalIgnoreCase))
                {
                    currentEntity.ScriptName = ExtractValue(line, "ScriptName");
                }
                else if (line.StartsWith("DefinitionType ", StringComparison.OrdinalIgnoreCase))
                {
                    currentEntity.DefinitionType = ExtractValue(line, "DefinitionType").Trim('"');
                }
            }
        }

        if (currentEntity != null)
        {
            entities.Add(currentEntity);
        }

        return entities;
    }

    private static string ExtractValue(string line, string propertyName)
    {
        int index = line.IndexOf(propertyName, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            string rest = line.Substring(index + propertyName.Length).Trim();
            return rest.TrimEnd(';').Trim();
        }
        return string.Empty;
    }

    public void ClearCache()
    {
        cachedAllEntities = null;
        cachedEntitiesByRegion = null;
        cacheTime = null;
        levelDataService.ClearCache();
    }
}

/// <summary>
/// Statistics about the game data catalog.
/// </summary>
public sealed class GameDataStatistics
{
    public int TotalEntities { get; set; }
    public int EntitiesWithScriptName { get; set; }
    public int RegionCount { get; set; }
    public int MarkerCount { get; set; }
    public int NpcCount { get; set; }
    public int CreatureCount { get; set; }
    public int ObjectCount { get; set; }
    public int ChestCount { get; set; }
    public int DoorCount { get; set; }
    public int QuestItemCount { get; set; }
}
