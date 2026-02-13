using System;
using System.Collections.Generic;
using System.Linq;
using FableQuestTool.Data;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ConsistencyAuditTests
{
    [Fact]
    public void NodeDefinitions_HaveUniqueTypeIdsAndSupportedPropertyTypes()
    {
        List<NodeDefinition> definitions = NodeDefinitions.GetAllNodes();
        Assert.NotEmpty(definitions);

        var duplicateTypeIds = definitions
            .GroupBy(d => d.Type, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.True(duplicateTypeIds.Count == 0,
            $"Duplicate node type ids: {string.Join(", ", duplicateTypeIds)}");

        HashSet<string> supportedPropertyTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "text",
            "string",
            "bool",
            "int",
            "float"
        };

        var unsupported = definitions
            .SelectMany(d => d.Properties ?? new List<NodeProperty>())
            .Where(p => !string.IsNullOrWhiteSpace(p.Type) && !supportedPropertyTypes.Contains(p.Type))
            .Select(p => p.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Assert.True(unsupported.Count == 0,
            $"Unsupported node property types (not mapped by UI connectors): {string.Join(", ", unsupported)}");
    }

    [Fact]
    public void BuiltInTemplates_UseKnownNodeTypesOrDocumentedDynamicTypes()
    {
        TemplateService templateService = new TemplateService();
        HashSet<string> knownTypes = new(
            NodeDefinitions.GetAllNodes().Select(n => n.Type),
            StringComparer.OrdinalIgnoreCase);

        List<QuestTemplate> templates = templateService.GetAllTemplates()
            .Where(t => t.Template != null)
            .ToList();

        Assert.NotEmpty(templates);

        List<string> violations = new();
        foreach (QuestTemplate template in templates)
        {
            foreach (QuestEntity entity in template.Template!.Entities)
            {
                foreach (BehaviorNode node in entity.Nodes)
                {
                    if (knownTypes.Contains(node.Type) || IsDocumentedDynamicNode(node.Type))
                    {
                        continue;
                    }

                    violations.Add($"{template.Name}/{entity.ScriptName}: {node.Type}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "Templates contain node types outside registry/dynamic contract: " + string.Join(" | ", violations));
    }

    private static bool IsDocumentedDynamicNode(string nodeType)
    {
        return nodeType.StartsWith("var_get_", StringComparison.OrdinalIgnoreCase) ||
               nodeType.StartsWith("var_set_", StringComparison.OrdinalIgnoreCase) ||
               nodeType.StartsWith("var_get_ext", StringComparison.OrdinalIgnoreCase) ||
               nodeType.StartsWith("var_set_ext", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeType, "reroute", StringComparison.OrdinalIgnoreCase);
    }
}
