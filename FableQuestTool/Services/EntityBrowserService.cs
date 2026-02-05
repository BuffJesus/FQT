using FableQuestTool.Config;
using FableQuestTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FableQuestTool.Services;

/// <summary>
/// Service for browsing and searching entities from Fable's TNG files.
/// Now enhanced to read from both loose files and BIG archives.
/// </summary>
public sealed class EntityBrowserService
{
    private readonly FableConfig config;
    private readonly GameDataCatalogService catalogService;

    /// <summary>
    /// Creates a new instance of EntityBrowserService.
    /// </summary>
    public EntityBrowserService(FableConfig config)
    {
        this.config = config;
        catalogService = new GameDataCatalogService(config);
    }

    /// <summary>
    /// Gets all entities from ALL sources (loose files + BIG archives).
    /// Results are cached for performance.
    /// </summary>
    public List<TngEntity> GetAllEntities(bool forceRefresh = false)
    {
        return catalogService.BuildCompleteCatalog(forceRefresh);
    }

    /// <summary>
    /// Gets comprehensive statistics about all game entities.
    /// </summary>
    public GameDataStatistics GetStatistics()
    {
        return catalogService.GetStatistics();
    }

    /// <summary>
    /// Gets entities for a specific region.
    /// </summary>
    public List<TngEntity> GetEntitiesForRegion(string regionName, bool scriptableOnly = false)
    {
        return catalogService.GetFilteredEntities(null, regionName, scriptableOnly);
    }

    /// <summary>
    /// Gets markers for a specific region (useful for spawn locations).
    /// </summary>
    public List<TngEntity> GetMarkersForRegion(string regionName)
    {
        return catalogService.GetFilteredEntities(EntityCategory.Marker, regionName, true);
    }

    /// <summary>
    /// Gets NPCs for a specific region (useful for quest interactions).
    /// </summary>
    public List<TngEntity> GetNpcsForRegion(string regionName)
    {
        return catalogService.GetFilteredEntities(EntityCategory.NPC, regionName, true);
    }

    /// <summary>
    /// Gets objects for a specific region (chests, doors, quest items, etc.).
    /// </summary>
    public List<TngEntity> GetObjectsForRegion(string regionName)
    {
        var objects = catalogService.GetFilteredEntities(EntityCategory.Object, regionName, true);
        var chests = catalogService.GetFilteredEntities(EntityCategory.Chest, regionName, true);
        var doors = catalogService.GetFilteredEntities(EntityCategory.Door, regionName, true);
        var questItems = catalogService.GetFilteredEntities(EntityCategory.QuestItem, regionName, true);

        var combined = new List<TngEntity>();
        combined.AddRange(objects);
        combined.AddRange(chests);
        combined.AddRange(doors);
        combined.AddRange(questItems);

        return combined;
    }

    /// <summary>
    /// Gets creatures for a specific region (useful for combat quests).
    /// </summary>
    public List<TngEntity> GetCreaturesForRegion(string regionName)
    {
        return catalogService.GetFilteredEntities(EntityCategory.Creature, regionName, true);
    }

    /// <summary>
    /// Searches for entities by name or definition type.
    /// </summary>
    public List<TngEntity> SearchEntities(string searchText, EntityCategory? category = null, string? regionName = null)
    {
        List<TngEntity> entities;

        if (!string.IsNullOrWhiteSpace(regionName))
        {
            entities = GetEntitiesForRegion(regionName, scriptableOnly: true);
        }
        else
        {
            entities = GetAllEntities();
        }

        entities = TngParser.FilterBySearch(entities, searchText);

        if (category.HasValue && category.Value != EntityCategory.Unknown)
        {
            entities = TngParser.FilterByCategory(entities, category.Value);
        }

        return entities;
    }

    /// <summary>
    /// Gets a list of all available regions.
    /// </summary>
    public List<string> GetAvailableRegions()
    {
        return RegionTngMapping.GetAllRegionNames();
    }

    /// <summary>
    /// Finds an entity by script name.
    /// </summary>
    public TngEntity? FindEntityByScriptName(string scriptName)
    {
        if (string.IsNullOrWhiteSpace(scriptName))
        {
            return null;
        }

        var entities = GetAllEntities();
        return entities.FirstOrDefault(e =>
            e.ScriptName.Equals(scriptName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Clears the entity cache, forcing a fresh scan on next request.
    /// </summary>
    public void ClearCache()
    {
        catalogService.ClearCache();
    }

    /// <summary>
    /// Exports the complete entity catalog to a text file.
    /// </summary>
    public void ExportCatalog(string outputPath)
    {
        catalogService.ExportCatalogToFile(outputPath);
    }

    /// <summary>
    /// Gets all unique creature definition types from the game files.
    /// Returns sorted list of creature DefinitionType values (e.g. "CREATURE_BANDIT", "CREATURE_VILLAGER_MALE").
    /// </summary>
    public List<string> GetAllCreatureDefinitions()
    {
        var allEntities = GetAllEntities();
        var creatureDefinitions = allEntities
            .Where(e => e.Category == EntityCategory.Creature && !string.IsNullOrWhiteSpace(e.DefinitionType))
            .Select(e => e.DefinitionType!)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        return creatureDefinitions;
    }

    /// <summary>
    /// Gets all unique object definition types from the game files, including chests, doors, and quest items.
    /// Returns sorted list of object DefinitionType values (e.g. "OBJECT_CHEST_SILVER", "OBJECT_BARREL").
    /// </summary>
    public List<string> GetAllObjectDefinitions()
    {
        var allEntities = GetAllEntities();
        var objectDefinitions = allEntities
            .Where(e => (e.Category == EntityCategory.Object ||
                        e.Category == EntityCategory.Chest ||
                        e.Category == EntityCategory.Door ||
                        e.Category == EntityCategory.QuestItem) &&
                        !string.IsNullOrWhiteSpace(e.DefinitionType))
            .Select(e => e.DefinitionType!)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        return objectDefinitions;
    }

    /// <summary>
    /// Gets all unique chest definition types from the game files.
    /// Returns sorted list of chest DefinitionType values (e.g. "OBJECT_CHEST_SILVER", "OBJECT_CHEST_GOLD").
    /// </summary>
    public List<string> GetAllChestDefinitions()
    {
        var allEntities = GetAllEntities();
        var chestDefinitions = allEntities
            .Where(e => e.Category == EntityCategory.Chest && !string.IsNullOrWhiteSpace(e.DefinitionType))
            .Select(e => e.DefinitionType!)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        return chestDefinitions;
    }
}
