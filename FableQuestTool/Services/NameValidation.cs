using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

public static class NameValidation
{
    private static readonly Regex LuaIdentifier = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    /// <summary>
    /// Executes ValidateProject.
    /// </summary>
    public static List<string> ValidateProject(QuestProject project)
    {
        var errors = new List<string>();

        if (project == null)
        {
            errors.Add("Project is null.");
            return errors;
        }

        ValidateLuaIdentifier("Quest Name", project.Name, errors);
        ValidateSafeFileName("Quest Name", project.Name, errors);

        var entityNames = new List<string>();
        foreach (var entity in project.Entities)
        {
            if (entity == null)
            {
                continue;
            }

            ValidateLuaIdentifier($"Entity Script Name ({entity.Id})", entity.ScriptName, errors);
            ValidateSafeFileName($"Entity Script Name ({entity.Id})", entity.ScriptName, errors);

            if (!string.IsNullOrWhiteSpace(entity.ScriptName))
            {
                entityNames.Add(entity.ScriptName);
            }
        }

        if (project.Rewards?.Container != null &&
            !string.IsNullOrWhiteSpace(project.Rewards.Container.ContainerScriptName))
        {
            ValidateLuaIdentifier("Reward Container Script Name", project.Rewards.Container.ContainerScriptName, errors);
            ValidateSafeFileName("Reward Container Script Name", project.Rewards.Container.ContainerScriptName, errors);
            entityNames.Add(project.Rewards.Container.ContainerScriptName);
        }

        AddDuplicateNameErrors(entityNames, errors);

        return errors;
    }

    private static void ValidateLuaIdentifier(string label, string? value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{label} is required.");
            return;
        }

        if (!LuaIdentifier.IsMatch(value))
        {
            errors.Add($"{label} must be a valid Lua identifier (letters, digits, underscore; cannot start with a digit).");
        }
    }

    private static void ValidateSafeFileName(string label, string? value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Contains("..", StringComparison.Ordinal))
        {
            errors.Add($"{label} cannot contain '..'.");
        }

        if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            value.Contains(Path.DirectorySeparatorChar) ||
            value.Contains(Path.AltDirectorySeparatorChar))
        {
            errors.Add($"{label} contains invalid file name characters.");
        }
    }

    private static void AddDuplicateNameErrors(IEnumerable<string> names, List<string> errors)
    {
        var duplicates = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicate in duplicates)
        {
            errors.Add($"Duplicate script name detected: {duplicate}");
        }
    }
}
