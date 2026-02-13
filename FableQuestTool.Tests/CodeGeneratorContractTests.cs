using System;
using System.Collections.Generic;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class CodeGeneratorContractTests
{
    [Fact]
    public void GenerateQuestScript_EscapesObjectiveTextForLuaStringLiteral()
    {
        QuestProject quest = new QuestProject
        {
            Name = "EscapeQuest",
            ObjectiveText = "Line1 \"quoted\"\\path\nLine2\tTabbed",
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC"
        };
        quest.Regions.Add("Oakvale");

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        Assert.Contains("Quest:SetQuestCardObjective(\"EscapeQuest\", \"Line1 \\\"quoted\\\"\\\\path\\nLine2\\tTabbed\", \"\", \"\")", script);
    }

    [Fact]
    public void GenerateEntityScript_EscapesTextPropertyValues()
    {
        QuestProject quest = new QuestProject { Name = "EntityEscapeQuest" };
        QuestEntity entity = new QuestEntity
        {
            ScriptName = "Talker",
            Nodes =
            {
                new BehaviorNode
                {
                    Id = "t",
                    Type = "onHeroTalks",
                    Category = "trigger"
                },
                new BehaviorNode
                {
                    Id = "d",
                    Type = "showDialogue",
                    Category = "action",
                    Config = new Dictionary<string, object>
                    {
                        ["text"] = "Hello \"Hero\"\\Road\nNext"
                    }
                }
            },
            Connections =
            {
                new NodeConnection
                {
                    FromNodeId = "t",
                    FromPort = "Output",
                    ToNodeId = "d",
                    ToPort = "Input"
                }
            }
        };

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        Assert.Contains("Me:SpeakAndWait(\"Hello \\\"Hero\\\"\\\\Road\\nNext\")", script);
    }

    [Fact]
    public void GenerateEntityScript_ReplacesQuotedVariablePlaceholderWithLuaExpression()
    {
        QuestProject quest = new QuestProject { Name = "VarRefQuest" };
        QuestEntity entity = new QuestEntity
        {
            ScriptName = "VarNpc",
            Nodes =
            {
                new BehaviorNode
                {
                    Id = "t",
                    Type = "onHeroTalks",
                    Category = "trigger"
                },
                new BehaviorNode
                {
                    Id = "s",
                    Type = "setStateString",
                    Category = "action",
                    Config = new Dictionary<string, object>
                    {
                        ["name"] = "CurrentStage",
                        ["value"] = "$Stage Name"
                    }
                }
            },
            Connections =
            {
                new NodeConnection
                {
                    FromNodeId = "t",
                    FromPort = "Output",
                    ToNodeId = "s",
                    ToPort = "Input"
                }
            },
            Variables =
            {
                new EntityVariable
                {
                    Name = "Stage Name",
                    Type = "String",
                    DefaultValue = "start",
                    IsExposed = false
                }
            }
        };

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        Assert.Contains("Quest:SetStateString(\"CurrentStage\", var_Stage_Name)", script);
        Assert.DoesNotContain("Quest:SetStateString(\"CurrentStage\", \"$Stage Name\")", script, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateQuestScript_AddsActionQueueContract_WhenQueueNodeIsPresent()
    {
        QuestProject quest = new QuestProject { Name = "QueueQuest" };
        quest.Regions.Add("Oakvale");

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "TargetNpc",
            Nodes =
            {
                new BehaviorNode
                {
                    Id = "trigger",
                    Type = "onHeroTalks",
                    Category = "trigger"
                },
                new BehaviorNode
                {
                    Id = "mark",
                    Type = "showMinimapMarkerByName",
                    Category = "action",
                    Config = new Dictionary<string, object>
                    {
                        ["targetScriptName"] = "TargetNpc",
                        ["markerName"] = "QuestMarker"
                    }
                }
            },
            Connections =
            {
                new NodeConnection
                {
                    FromNodeId = "trigger",
                    FromPort = "Output",
                    ToNodeId = "mark",
                    ToPort = "Input"
                }
            }
        };
        quest.Entities.Add(entity);

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(quest);

        Assert.Contains("Quest:SetStateInt(\"FQT_ActionCounter\", 0)", script);
        Assert.Contains("Quest:SetStateInt(\"FQT_ActionProcessed\", 0)", script);
        Assert.Contains("Quest:SetStateBool(\"FQT_StopActions\", false)", script);
        Assert.Contains("function ProcessQuestActionQueueOnce()", script);
        Assert.Contains("function ProcessQuestActions(questObject)", script);
        Assert.Contains("Quest:CreateThread(\"ProcessQuestActions\", {region=\"Oakvale\"})", script);
        Assert.Contains("Quest:MiniMapAddMarker(target, markerName)", script);
    }

    [Fact]
    public void GenerateEntityScript_EmitsDiagnosticComment_ForUnknownNodeType()
    {
        QuestProject quest = new QuestProject { Name = "UnknownNodeQuest" };
        QuestEntity entity = new QuestEntity
        {
            ScriptName = "UnknownNodeNpc",
            Nodes =
            {
                new BehaviorNode
                {
                    Id = "t",
                    Type = "onHeroTalks",
                    Category = "trigger"
                },
                new BehaviorNode
                {
                    Id = "u",
                    Type = "notARealNodeType",
                    Category = "action"
                }
            },
            Connections =
            {
                new NodeConnection
                {
                    FromNodeId = "t",
                    FromPort = "Output",
                    ToNodeId = "u",
                    ToPort = "Input"
                }
            }
        };

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateEntityScript(quest, entity);

        Assert.Contains("-- [FQT-CG-001] Unknown node type: notARealNodeType", script);
    }
}
