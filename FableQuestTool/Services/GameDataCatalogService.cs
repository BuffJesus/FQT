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
    /// Scans ALL TNGs (WAD extraction + loose files + BIG archives) and builds complete entity catalog.
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

        // 1. Extract TNG files from WAD archives (for vanilla Fable installs)
        ExtractTngFilesFromWad();

        // 2. Scan loose TNG files (from data/Levels/FinalAlbion/*.tng or temp cache)
        var looseTngs = ScanLooseTngFiles();
        allEntities.AddRange(looseTngs);

        // 3. Scan TNG files inside BIG archives (for modded installs)
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
    /// <summary>
    /// Extracts TNG files from FinalAlbion.wad to a temporary cache directory.
    /// This allows reading TNG data from vanilla Fable installations.
    /// </summary>
    private void ExtractTngFilesFromWad()
    {
        if (string.IsNullOrWhiteSpace(config.FablePath))
        {
            return;
        }

        string wadPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion.wad");
        if (!File.Exists(wadPath))
        {
            System.Diagnostics.Debug.WriteLine("GameDataCatalogService: FinalAlbion.wad not found");
            return;
        }

        // Create cache directory in temp
        string cacheDir = Path.Combine(Path.GetTempPath(), "FQT_TngCache");
        Directory.CreateDirectory(cacheDir);

        // Check if already extracted
        if (Directory.Exists(cacheDir) && Directory.GetFiles(cacheDir, "*.tng").Length > 0)
        {
            System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Using cached TNG files from {cacheDir}");

            // Copy cached TNGs to FinalAlbion directory for compatibility
            string finalAlbionPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion");
            Directory.CreateDirectory(finalAlbionPath);

            foreach (string cachedTng in Directory.GetFiles(cacheDir, "*.tng"))
            {
                string targetPath = Path.Combine(finalAlbionPath, Path.GetFileName(cachedTng));
                if (!File.Exists(targetPath))
                {
                    try
                    {
                        File.Copy(cachedTng, targetPath, overwrite: false);
                    }
                    catch
                    {
                        // Ignore copy errors - cache files still work
                    }
                }
            }
            return;
        }

        System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Extracting TNG files from {wadPath}...");

        // Extract all files from WAD
        if (!WadBridgeClient.TryExtractAll(wadPath, cacheDir, out string? error))
        {
            System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: WAD extraction failed: {error}");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: TNG files extracted to {cacheDir}");

        // Copy extracted TNGs to FinalAlbion directory
        string levelsPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion");
        Directory.CreateDirectory(levelsPath);

        foreach (string tngFile in Directory.GetFiles(cacheDir, "*.tng"))
        {
            string targetPath = Path.Combine(levelsPath, Path.GetFileName(tngFile));
            try
            {
                File.Copy(tngFile, targetPath, overwrite: true);
                System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Copied {Path.GetFileName(tngFile)} to FinalAlbion");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Failed to copy {Path.GetFileName(tngFile)}: {ex.Message}");
            }
        }
    }

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
    /// NOTE: Vanilla Fable stores TNG data in LEV/WAD files, NOT as loose .tng files in BIG archives.
    /// This method will only work with modded installations that have extracted TNG files.
    /// For vanilla installs, use the static fallback data in GameData.Creatures.
    /// </summary>
    private List<TngEntity> ScanBigArchiveTngs()
    {
        var entities = new List<TngEntity>();

        try
        {
            System.Diagnostics.Debug.WriteLine("GameDataCatalogService: Scanning BIG archives for TNG files...");
            System.Diagnostics.Debug.WriteLine("GameDataCatalogService: NOTE - Vanilla Fable TNG data is in LEV/WAD files, not BIG archives");
            System.Diagnostics.Debug.WriteLine("GameDataCatalogService: TNG extraction from LEV files requires FableMod libraries (not implemented)");

            var tngEntries = levelDataService.FindTngEntries();
            System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Found {tngEntries.Count} TNG entries in BIG archives");

            if (tngEntries.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("GameDataCatalogService: No TNG files found (expected for vanilla Fable)");
                System.Diagnostics.Debug.WriteLine("GameDataCatalogService: Using static fallback creature definitions from GameData.Creatures");
                return entities;
            }

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

            System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Parsed {entities.Count} total entities from BIG archives");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Error scanning BIG archives: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GameDataCatalogService: Stack trace: {ex.StackTrace}");
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
