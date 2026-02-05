using FableQuestTool.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FableQuestTool.Services;

/// <summary>
/// Manages quest ID discovery and availability checks.
/// </summary>
public sealed class QuestIdManager
{
    private readonly FableConfig config;
    private const int MinCustomQuestId = 50000;
    private const int MaxCustomQuestId = 99999;

    /// <summary>
    /// Creates a new instance of QuestIdManager.
    /// </summary>
    public QuestIdManager(FableConfig config)
    {
        this.config = config;
    }

    /// <summary>
    /// Scans the FSE folder and quests.lua to find all used quest IDs.
    /// Returns the next available ID starting from 50000.
    /// </summary>
    /// <summary>
    /// Suggests the next available quest ID in the custom range.
    /// </summary>
    public int SuggestNextQuestId()
    {
        var usedIds = GetUsedQuestIds();

        // Find the first available ID starting from MinCustomQuestId
        for (int id = MinCustomQuestId; id <= MaxCustomQuestId; id++)
        {
            if (!usedIds.Contains(id))
            {
                return id;
            }
        }

        // If all IDs are taken, return the max + 1 (fallback)
        return MaxCustomQuestId + 1;
    }

    /// <summary>
    /// Gets all quest IDs currently in use by parsing quests.lua.
    /// </summary>
    /// <summary>
    /// Gets all quest IDs currently registered in quests.lua.
    /// </summary>
    public HashSet<int> GetUsedQuestIds()
    {
        var usedIds = new HashSet<int>();

        string? fseFolder = config.GetFseFolder();
        if (string.IsNullOrWhiteSpace(fseFolder) || !Directory.Exists(fseFolder))
        {
            return usedIds;
        }

        string questsLuaPath = Path.Combine(fseFolder, "quests.lua");
        if (!File.Exists(questsLuaPath))
        {
            return usedIds;
        }

        try
        {
            string content = File.ReadAllText(questsLuaPath);

            // Match patterns like: id = 50001 (new format) or QuestID = 50001 (old format)
            var matches = Regex.Matches(content, @"(?:^|\s)(?:id|QuestID)\s*=\s*(\d+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int questId))
                {
                    usedIds.Add(questId);
                }
            }
        }
        catch
        {
            // If parsing fails, return empty set
        }

        return usedIds;
    }

    /// <summary>
    /// Checks if a quest ID is available (not in use).
    /// </summary>
    /// <summary>
    /// Checks whether a quest ID is unused.
    /// </summary>
    public bool IsQuestIdAvailable(int questId)
    {
        var usedIds = GetUsedQuestIds();
        return !usedIds.Contains(questId);
    }

    /// <summary>
    /// Gets a list of all quest names currently in the FSE folder.
    /// </summary>
    /// <summary>
    /// Gets all quest names currently registered in quests.lua.
    /// </summary>
    public List<string> GetExistingQuestNames()
    {
        var questNames = new List<string>();

        string? fseFolder = config.GetFseFolder();
        if (string.IsNullOrWhiteSpace(fseFolder) || !Directory.Exists(fseFolder))
        {
            return questNames;
        }

        string questsLuaPath = Path.Combine(fseFolder, "quests.lua");
        if (!File.Exists(questsLuaPath))
        {
            return questNames;
        }

        try
        {
            string content = File.ReadAllText(questsLuaPath);

            // Match patterns like: name = "MyQuest" (new format) or QuestName = "MyQuest" (old format)
            var matches = Regex.Matches(content, @"(?:^|\s)(?:name|QuestName)\s*=\s*""([^""]+)""", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    questNames.Add(match.Groups[1].Value);
                }
            }
        }
        catch
        {
            // If parsing fails, return empty list
        }

        return questNames;
    }
}
