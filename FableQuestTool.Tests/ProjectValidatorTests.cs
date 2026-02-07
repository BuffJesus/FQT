using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ProjectValidatorTests
{
    [Fact]
    public void Validate_FindsWarningsAndErrors()
    {
        QuestProject project = new QuestProject
        {
            Name = "Bad Name",
            Id = 100
        };

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "Entity1"
        };
        entity.Nodes.Add(new BehaviorNode
        {
            Id = "node1",
            Type = "parallel",
            Category = "flow"
        });
        entity.Nodes.Add(new BehaviorNode
        {
            Id = "node2",
            Type = "unknownNode",
            Category = "action"
        });
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = "node1",
            FromPort = "Output",
            ToNodeId = "missing",
            ToPort = "Input"
        });
        project.Entities.Add(entity);

        ProjectValidator validator = new ProjectValidator();
        var issues = validator.Validate(project);

        Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error);
        Assert.Contains(issues, i => i.Message.Contains("valid Lua identifier", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, i => i.Message.Contains("unknown node type", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, i => i.Message.Contains("connection to a missing node", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, i => i.Message.Contains("no regions", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, i => i.Message.Contains("no trigger", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, i => i.Message.Contains("Parallel", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ReportsNoBehaviorNodes()
    {
        QuestProject project = new QuestProject { Name = "QuestOk" };
        project.Entities.Add(new QuestEntity { ScriptName = "EmptyEntity" });

        ProjectValidator validator = new ProjectValidator();
        var issues = validator.Validate(project);

        Assert.Contains(issues, i => i.Message.Contains("no behavior nodes", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_SkipsVariableAndRerouteNodes()
    {
        QuestProject project = new QuestProject { Name = "QuestOk" };
        QuestEntity entity = new QuestEntity { ScriptName = "EntityVar" };
        entity.Nodes.Add(new BehaviorNode
        {
            Id = "var1",
            Type = "var_get_Test",
            Category = "variable"
        });
        entity.Nodes.Add(new BehaviorNode
        {
            Id = "reroute1",
            Type = "reroute",
            Category = "flow"
        });
        entity.Nodes.Add(new BehaviorNode
        {
            Id = "trigger1",
            Type = "onHeroTalks",
            Category = "trigger"
        });
        project.Entities.Add(entity);

        ProjectValidator validator = new ProjectValidator();
        var issues = validator.Validate(project);

        Assert.DoesNotContain(issues, i => i.Message.Contains("unknown node type", System.StringComparison.OrdinalIgnoreCase));
    }
}
