using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Parses TNG (Thing) files to extract entity information.
/// </summary>
public static class TngParser
{
    /// <summary>
    /// Parses a single TNG file and extracts all entities.
    /// </summary>
    public static List<TngEntity> ParseTngFile(string tngPath, string regionName = "")
    {
        List<TngEntity> entities = new List<TngEntity>();

        if (!File.Exists(tngPath))
        {
            return entities;
        }

        try
        {
            string[] lines = File.ReadAllLines(tngPath);
            TngEntity? currentEntity = null;
            string fileName = Path.GetFileName(tngPath);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Start of new entity: "NewThing <Type>;"
                if (line.StartsWith("NewThing ", StringComparison.OrdinalIgnoreCase))
                {
                    // Save previous entity if it exists
                    if (currentEntity != null)
                    {
                        entities.Add(currentEntity);
                    }

                    // Create new entity
                    currentEntity = new TngEntity
                    {
                        SourceFile = fileName,
                        RegionName = regionName
                    };

                    // Extract thing type (e.g., "AICreature", "Object", "Marker")
                    string thingType = line.Substring("NewThing ".Length).TrimEnd(';').Trim();
                    currentEntity.ThingType = thingType;
                }
                else if (currentEntity != null)
                {
                    // Parse properties
                    if (line.StartsWith("ScriptName ", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEntity.ScriptName = ExtractValue(line, "ScriptName");
                    }
                    else if (line.StartsWith("DefinitionType ", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEntity.DefinitionType = ExtractValue(line, "DefinitionType").Trim('"');
                    }
                    else if (line.StartsWith("UID ", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEntity.Uid = ExtractValue(line, "UID");
                    }
                    else if (line.StartsWith("PositionX ", StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(ExtractValue(line, "PositionX"), NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                        {
                            currentEntity.PositionX = x;
                        }
                    }
                    else if (line.StartsWith("PositionY ", StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(ExtractValue(line, "PositionY"), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                        {
                            currentEntity.PositionY = y;
                        }
                    }
                    else if (line.StartsWith("PositionZ ", StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(ExtractValue(line, "PositionZ"), NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            currentEntity.PositionZ = z;
                        }
                    }
                    else if (line.StartsWith("ScriptData ", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEntity.ScriptData = ExtractValue(line, "ScriptData").Trim('"');
                    }
                    else if (line.StartsWith("ThingGamePersistent ", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(line, "ThingGamePersistent");
                        currentEntity.IsGamePersistent = value.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (line.StartsWith("ThingLevelPersistent ", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(line, "ThingLevelPersistent");
                        currentEntity.IsLevelPersistent = value.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                    }
                    // End of entity (semicolon on its own line)
                    else if (line == "EndThing" || (line == ";" && !string.IsNullOrEmpty(currentEntity.ThingType)))
                    {
                        entities.Add(currentEntity);
                        currentEntity = null;
                    }
                }
            }

            // Add last entity if file doesn't end with semicolon
            if (currentEntity != null)
            {
                entities.Add(currentEntity);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing TNG file {tngPath}: {ex.Message}");
        }

        return entities;
    }

    /// <summary>
    /// Parses multiple TNG files for a region.
    /// </summary>
    public static List<TngEntity> ParseRegionTngs(IEnumerable<string> tngPaths, string regionName)
    {
        List<TngEntity> allEntities = new List<TngEntity>();

        foreach (string tngPath in tngPaths)
        {
            var entities = ParseTngFile(tngPath, regionName);
            allEntities.AddRange(entities);
        }

        return allEntities;
    }

    /// <summary>
    /// Filters entities by category.
    /// </summary>
    public static List<TngEntity> FilterByCategory(List<TngEntity> entities, EntityCategory category)
    {
        if (category == EntityCategory.Unknown)
        {
            return entities; // Return all
        }

        return entities.Where(e => e.Category == category).ToList();
    }

    /// <summary>
    /// Filters entities by search text (searches ScriptName and DefinitionType).
    /// </summary>
    public static List<TngEntity> FilterBySearch(List<TngEntity> entities, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return entities;
        }

        string search = searchText.Trim().ToLowerInvariant();
        return entities.Where(e =>
            (e.ScriptName?.ToLowerInvariant().Contains(search) ?? false) ||
            (e.DefinitionType?.ToLowerInvariant().Contains(search) ?? false)
        ).ToList();
    }

    /// <summary>
    /// Filters to only entities with ScriptNames (can be referenced by FSE).
    /// </summary>
    public static List<TngEntity> FilterScriptableOnly(List<TngEntity> entities)
    {
        return entities.Where(e => e.HasScriptName).ToList();
    }

    /// <summary>
    /// Extracts value after property name in TNG line.
    /// Example: "ScriptName MK_HERO_SPAWN;" -> "MK_HERO_SPAWN"
    /// </summary>
    private static string ExtractValue(string line, string propertyName)
    {
        string pattern = $@"{propertyName}\s+(.+?);";
        Match match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Fallback: simple split
        int index = line.IndexOf(propertyName, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            string rest = line.Substring(index + propertyName.Length).Trim();
            return rest.TrimEnd(';').Trim();
        }

        return string.Empty;
    }

    /// <summary>
    /// Generates Lua code snippet for accessing an entity.
    /// </summary>
    public static string GenerateLuaSnippet(TngEntity entity, bool includeComments = true)
    {
        if (!entity.HasScriptName)
        {
            return "-- Error: Entity has no ScriptName";
        }

        StringBuilder sb = new StringBuilder();

        if (includeComments)
        {
            sb.AppendLine($"-- Entity: {entity.ScriptName}");
            sb.AppendLine($"-- Type: {entity.DefinitionType}");
            sb.AppendLine($"-- Position: ({entity.PositionX:F2}, {entity.PositionY:F2}, {entity.PositionZ:F2})");
            if (!string.IsNullOrEmpty(entity.RegionName))
            {
                sb.AppendLine($"-- Region: {entity.RegionName}");
            }
        }

        sb.AppendLine($"local entity = Quest:GetThingWithScriptName(\"{entity.ScriptName}\")");
        sb.AppendLine("if entity and not entity:IsNull() then");
        sb.AppendLine("    Quest:Log(\"Found entity: " + entity.ScriptName + "\")");
        sb.AppendLine("    -- Add your quest logic here");
        sb.AppendLine("else");
        sb.AppendLine("    Quest:Log(\"ERROR: Could not find entity: " + entity.ScriptName + "\")");
        sb.AppendLine("end");

        return sb.ToString();
    }
}
