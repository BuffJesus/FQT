using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FableQuestTool.Formats;

public sealed class QstFile
{
    private readonly List<QstQuestEntry> quests = new List<QstQuestEntry>();
    private readonly string sourcePath;

    private QstFile(string path)
    {
        sourcePath = path;
    }

    public IReadOnlyList<QstQuestEntry> Quests => quests;

    public static QstFile Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("QST path is required.", nameof(path));
        }

        string text = File.ReadAllText(path);
        return Parse(text, path);
    }

    private static QstFile Parse(string text, string path)
    {
        QstFile file = new QstFile(path);
        int i = 0;
        bool inString = false;
        bool escape = false;

        while (i < text.Length)
        {
            char c = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    i++;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    inString = false;
                }

                i++;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                i++;
                continue;
            }

            if (TryMatchWord(text, i, "AddQuest"))
            {
                int endIndex = ParseCall(text, file, i, "AddQuest");
                i = endIndex >= 0 ? endIndex + 1 : i + "AddQuest".Length;
                continue;
            }

            i++;
        }

        return file;
    }

    private static int ParseCall(string text, QstFile file, int startIndex, string name)
    {
        int cursor = SkipWhitespace(text, startIndex + name.Length);
        if (cursor >= text.Length || text[cursor] != '(')
        {
            return -1;
        }

        if (!TryParseArguments(text, cursor + 1, out List<string> args, out int endIndex))
        {
            return -1;
        }

        if (args.Count >= 2)
        {
            string questName = args[0];
            string rawEnabled = args[1];
            bool? enabled = ParseBool(rawEnabled);
            file.quests.Add(new QstQuestEntry(questName, enabled, rawEnabled));
        }

        return endIndex;
    }

    private static int SkipWhitespace(string text, int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }
        return index;
    }

    private static bool TryMatchWord(string text, int index, string value)
    {
        if (index < 0 || index + value.Length > text.Length)
        {
            return false;
        }

        if (!text.AsSpan(index, value.Length).Equals(value.AsSpan(), StringComparison.Ordinal))
        {
            return false;
        }

        if (index > 0 && IsIdentifierChar(text[index - 1]))
        {
            return false;
        }

        int end = index + value.Length;
        if (end < text.Length && IsIdentifierChar(text[end]))
        {
            return false;
        }

        return true;
    }

    private static bool IsIdentifierChar(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_';
    }

    private static bool TryParseArguments(string text, int startIndex, out List<string> args, out int endIndex)
    {
        args = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inString = false;
        bool escape = false;
        int depth = 1;

        for (int i = startIndex; i < text.Length; i++)
        {
            char c = text[i];

            if (inString)
            {
                if (escape)
                {
                    current.Append(c);
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = false;
                    continue;
                }

                current.Append(c);
                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '(')
            {
                depth++;
                current.Append(c);
                continue;
            }

            if (c == ')')
            {
                depth--;
                if (depth == 0)
                {
                    args.Add(current.ToString().Trim());
                    endIndex = i;
                    return true;
                }
                current.Append(c);
                continue;
            }

            if (c == ',' && depth == 1)
            {
                args.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        endIndex = text.Length - 1;
        return false;
    }

    private static bool? ParseBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Equals("true", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalized.Equals("false", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("FALSE", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("0", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return null;
    }

    public bool HasQuest(string questName)
    {
        if (string.IsNullOrWhiteSpace(questName))
        {
            return false;
        }

        foreach (var quest in quests)
        {
            if (quest.Name.Equals(questName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool AddQuestIfMissing(string questName, bool enabled = false)
    {
        if (HasQuest(questName))
        {
            return false;
        }

        quests.Add(new QstQuestEntry(questName, enabled, enabled ? "TRUE" : "FALSE"));
        return true;
    }

    public bool RemoveQuest(string questName)
    {
        if (string.IsNullOrWhiteSpace(questName))
        {
            return false;
        }

        // Don't remove vanilla/protected quests
        if (ShouldSkipRemoval(questName))
        {
            return false;
        }

        int removed = quests.RemoveAll(q => q.Name.Equals(questName, StringComparison.OrdinalIgnoreCase));
        return removed > 0;
    }

    /// <summary>
    /// Synchronizes the QST file with a list of active quest names.
    /// Adds missing quests and removes quests not in the list.
    /// </summary>
    public void SyncQuests(IEnumerable<string> activeQuestNames, out int added, out int removed)
    {
        added = 0;
        removed = 0;

        var activeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in activeQuestNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                activeSet.Add(name);
            }
        }

        // Add missing quests
        foreach (var questName in activeSet)
        {
            if (AddQuestIfMissing(questName, false))
            {
                added++;
            }
        }

        // Remove quests not in active list (but protect vanilla quests)
        for (int i = quests.Count - 1; i >= 0; i--)
        {
            var questName = quests[i].Name;

            // Skip vanilla/system quests
            if (ShouldSkipRemoval(questName))
            {
                continue;
            }

            if (!activeSet.Contains(questName))
            {
                quests.RemoveAt(i);
                removed++;
            }
        }
    }

    /// <summary>
    /// Determines if a quest should be protected from removal (vanilla quests).
    /// </summary>
    private static bool ShouldSkipRemoval(string questName)
    {
        // Protect vanilla quests - they start with specific prefixes
        if (questName.StartsWith("Q_", StringComparison.OrdinalIgnoreCase) ||
            questName.StartsWith("V_", StringComparison.OrdinalIgnoreCase) ||
            questName.StartsWith("QS_", StringComparison.OrdinalIgnoreCase) ||
            questName.StartsWith("QR_", StringComparison.OrdinalIgnoreCase) ||
            questName.StartsWith("CS_", StringComparison.OrdinalIgnoreCase) ||
            questName.StartsWith("Hook_", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Protect known system quests
        var protectedQuests = new[]
        {
            "Gameflow", "GameflowAssistance", "CreatureGenerators",
            "ChapterAndSceneManager", "PersonalScriptMain", "PersonalScript_GlobalThings",
            "NPCDeath", "HeroBoasts", "DummyQuestForHeroLevels", "DummyQuestForScarletRoseStatue"
        };

        foreach (var protected_ in protectedQuests)
        {
            if (questName.Equals(protected_, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void Save(string? outputPath = null)
    {
        string path = outputPath ?? sourcePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("No output path specified.");
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine();

        foreach (var quest in quests)
        {
            string enabledStr = quest.Enabled.HasValue
                ? (quest.Enabled.Value ? "TRUE" : "FALSE")
                : quest.RawEnabled;

            sb.AppendLine($"AddQuest(\"{quest.Name}\", \t\t\t{enabledStr});");
        }

        File.WriteAllText(path, sb.ToString());
    }
}

public sealed class QstQuestEntry
{
    public QstQuestEntry(string name, bool? enabled, string rawEnabled)
    {
        Name = name ?? string.Empty;
        Enabled = enabled;
        RawEnabled = rawEnabled ?? string.Empty;
    }

    public string Name { get; }
    public bool? Enabled { get; }
    public string RawEnabled { get; }
}
