using System;
using System.Collections.Generic;
using System.IO;

namespace FableQuestTool.Core;

public sealed class IniFile
{
    private readonly Dictionary<string, Dictionary<string, string>> sections;

    private IniFile(Dictionary<string, Dictionary<string, string>> sections)
    {
        this.sections = sections;
    }

    public static IniFile CreateEmpty()
    {
        return new IniFile(new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase));
    }

    public static IniFile Load(string path)
    {
        Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = string.Empty;

        foreach (string rawLine in File.ReadAllLines(path))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
            {
                currentSection = line.Substring(1, line.Length - 2).Trim();
                if (!sections.ContainsKey(currentSection))
                {
                    sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                continue;
            }

            int splitIndex = line.IndexOf('=');
            if (splitIndex <= 0)
            {
                continue;
            }

            string key = line.Substring(0, splitIndex).Trim();
            string value = line.Substring(splitIndex + 1).Trim();

            if (!sections.TryGetValue(currentSection, out Dictionary<string, string>? section))
            {
                section = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                sections[currentSection] = section;
            }

            section[key] = value;
        }

        return new IniFile(sections);
    }

    public string? Get(string section, string key)
    {
        if (sections.TryGetValue(section, out Dictionary<string, string>? values)
            && values.TryGetValue(key, out string? value))
        {
            return value;
        }

        return null;
    }

    public IReadOnlyDictionary<string, string>? GetSection(string section)
    {
        if (sections.TryGetValue(section, out Dictionary<string, string>? values))
        {
            return values;
        }

        return null;
    }

    public void Set(string section, string key, string value)
    {
        if (!sections.TryGetValue(section, out Dictionary<string, string>? values))
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            sections[section] = values;
        }

        values[key] = value;
    }

    public void Save(string path)
    {
        using StreamWriter writer = new StreamWriter(path);
        foreach (KeyValuePair<string, Dictionary<string, string>> section in sections)
        {
            if (!string.IsNullOrEmpty(section.Key))
            {
                writer.WriteLine($"[{section.Key}]");
            }

            foreach (KeyValuePair<string, string> kvp in section.Value)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }

            writer.WriteLine();
        }
    }
}
