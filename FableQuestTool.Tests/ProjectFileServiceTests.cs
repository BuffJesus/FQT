using System;
using System.IO;
using System.Text.Json;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ProjectFileServiceTests
{
    [Fact]
    public void SaveLoad_RoundTrip_PreservesCoreData()
    {
        QuestProject quest = BuildProject();
        ProjectFileService service = new ProjectFileService();

        using TestTempDirectory temp = new TestTempDirectory();
        string path = System.IO.Path.Combine(temp.Path, "RoundTrip.fqtproj");

        service.Save(path, quest);
        QuestProject loaded = service.Load(path);

        Assert.Equal("RoundTripQuest", loaded.Name);
        Assert.Equal(51000, loaded.Id);
        Assert.Equal(2, loaded.States.Count);
        Assert.Single(loaded.Entities);
        Assert.Single(loaded.Threads);
        Assert.Equal(100, loaded.Rewards.Gold);

        QuestEntity entity = loaded.Entities[0];
        Assert.Equal("QuestGiver", entity.ScriptName);
        Assert.Equal(2, entity.Nodes.Count);
        Assert.Single(entity.Connections);

        BehaviorNode checkHasItem = entity.Nodes[1];
        Assert.Equal("checkHasItem", checkHasItem.Type);
        Assert.Equal("OBJECT_CARROT", GetConfigString(checkHasItem, "item"));
    }

    private static QuestProject BuildProject()
    {
        QuestProject quest = new QuestProject
        {
            Name = "RoundTripQuest",
            Id = 51000
        };
        quest.Rewards.Gold = 100;
        quest.States.Add(new QuestState { Name = "Flag", Type = "bool", Persist = true, DefaultValue = true });
        quest.States.Add(new QuestState { Name = "Count", Type = "int", Persist = false, DefaultValue = 2 });
        quest.Threads.Add(new QuestThread { FunctionName = "Watcher", Region = "Oakvale" });

        QuestEntity entity = new QuestEntity
        {
            ScriptName = "QuestGiver",
            EntityType = EntityType.Creature
        };
        BehaviorNode takeItem = new BehaviorNode
        {
            Id = "take",
            Type = "takeItem",
            Category = "action"
        };
        takeItem.Config["item"] = "OBJECT_CARROT";
        BehaviorNode checkHasItem = new BehaviorNode
        {
            Id = "check",
            Type = "checkHasItem",
            Category = "condition"
        };
        entity.Nodes.Add(takeItem);
        entity.Nodes.Add(checkHasItem);
        entity.Connections.Add(new NodeConnection
        {
            FromNodeId = takeItem.Id,
            FromPort = "Output",
            ToNodeId = checkHasItem.Id,
            ToPort = "Input"
        });
        quest.Entities.Add(entity);

        return quest;
    }

    private static string GetConfigString(BehaviorNode node, string key)
    {
        if (!node.Config.TryGetValue(key, out object? value) || value == null)
        {
            return string.Empty;
        }

        if (value is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? string.Empty;
            }

            return element.ToString() ?? string.Empty;
        }

        return Convert.ToString(value) ?? string.Empty;
    }
}
