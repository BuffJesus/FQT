using System;
using System.Collections.Generic;
using System.Linq;
using FableQuestTool.Data;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Severity level for validation issues.
/// </summary>
public enum ValidationSeverity
{
    Error,
    Warning
}

/// <summary>
/// Represents a validation result with severity and message.
/// </summary>
public sealed record ValidationIssue(ValidationSeverity Severity, string Message, string? Code = null);

/// <summary>
/// Performs structural and naming validation on quest projects.
/// </summary>
public sealed class ProjectValidator
{
    /// <summary>
    /// Validates a quest project and returns a list of issues.
    /// </summary>
    public List<ValidationIssue> Validate(QuestProject project)
    {
        var issues = new List<ValidationIssue>();

        if (project == null)
        {
            issues.Add(CreateIssue(ValidationSeverity.Error, "FQT-VAL-001", "Project is null."));
            return issues;
        }

        foreach (var error in NameValidation.ValidateProject(project))
        {
            issues.Add(CreateIssue(ValidationSeverity.Error, "FQT-VAL-002", error));
        }

        if (project.Id < 50000)
        {
            issues.Add(CreateIssue(ValidationSeverity.Warning, "FQT-VAL-003", $"Quest ID {project.Id} is below 50000 and may conflict with base game quests."));
        }

        if (project.Regions.Count == 0)
        {
            issues.Add(CreateIssue(ValidationSeverity.Warning, "FQT-VAL-004", "Quest has no regions configured."));
        }

        var knownNodeTypes = new HashSet<string>(NodeDefinitions.GetAllNodes().Select(n => n.Type), StringComparer.OrdinalIgnoreCase);

        foreach (var entity in project.Entities)
        {
            if (entity == null)
            {
                continue;
            }

            if (entity.Nodes.Count == 0)
            {
                issues.Add(CreateIssue(ValidationSeverity.Warning, "FQT-VAL-005", $"Entity '{entity.ScriptName}' has no behavior nodes."));
                continue;
            }

            bool hasTrigger = entity.Nodes.Any(n => string.Equals(n.Category, "trigger", StringComparison.OrdinalIgnoreCase));
            if (!hasTrigger)
            {
                issues.Add(CreateIssue(ValidationSeverity.Warning, "FQT-VAL-006", $"Entity '{entity.ScriptName}' has no trigger nodes."));
            }

            var nodeIds = new HashSet<string>(entity.Nodes.Select(n => n.Id));

            foreach (var node in entity.Nodes)
            {
                if (IsVariableNode(node.Type) || string.Equals(node.Type, "reroute", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!knownNodeTypes.Contains(node.Type))
                {
                    issues.Add(CreateIssue(ValidationSeverity.Error, "FQT-VAL-007", $"Entity '{entity.ScriptName}' uses unknown node type '{node.Type}'."));
                }

                if (string.Equals(node.Type, "parallel", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(CreateIssue(ValidationSeverity.Warning, "FQT-VAL-008", $"Entity '{entity.ScriptName}' uses Parallel; entity scripts run it sequentially."));
                }
            }

            foreach (var connection in entity.Connections)
            {
                if (!nodeIds.Contains(connection.FromNodeId) || !nodeIds.Contains(connection.ToNodeId))
                {
                    issues.Add(CreateIssue(ValidationSeverity.Error, "FQT-VAL-009", $"Entity '{entity.ScriptName}' has a connection to a missing node."));
                }
            }
        }

        return issues;
    }

    private static bool IsVariableNode(string type)
    {
        return type.StartsWith("var_get_", StringComparison.OrdinalIgnoreCase) ||
               type.StartsWith("var_set_", StringComparison.OrdinalIgnoreCase) ||
               type.StartsWith("var_get_ext", StringComparison.OrdinalIgnoreCase) ||
               type.StartsWith("var_set_ext", StringComparison.OrdinalIgnoreCase);
    }

    private static ValidationIssue CreateIssue(ValidationSeverity severity, string code, string message)
    {
        return new ValidationIssue(severity, $"[{code}] {message}", code);
    }
}
