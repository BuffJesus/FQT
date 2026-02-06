using System.Linq;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ViewModelEntityTabTests
{
    [Fact]
    public void EntityTab_AddNode_AddsNodeAndRecent()
    {
        QuestEntity entity = new QuestEntity { ScriptName = "TestEntity" };
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        NodeOption option = viewModel.SimpleNodes.First(n => n.Definition != null);
        viewModel.AddNodeCommand.Execute(option);

        Assert.Single(viewModel.Nodes);
        Assert.Equal(option.Type, viewModel.Nodes[0].Type);
        Assert.Contains(viewModel.RecentNodes, recent => recent.Type == option.Type);
    }

    [Fact]
    public void EntityTab_ApplyTalkTemplate_BuildsNodes()
    {
        QuestEntity entity = new QuestEntity { ScriptName = "TemplateEntity" };
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.ApplyTemplateCommand.Execute("Talk");

        Assert.Equal(3, viewModel.Nodes.Count);
        var types = viewModel.Nodes.Select(n => n.Type).ToList();
        Assert.Contains("onHeroTalks", types);
        Assert.Contains("showDialogue", types);
        Assert.Contains("completeQuest", types);
    }

    [Fact]
    public void EntityTab_ApplyRewardTemplate_ConfiguresObjectReward()
    {
        QuestEntity entity = new QuestEntity();
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        viewModel.ApplyTemplateCommand.Execute("Reward");

        Assert.Equal(EntityType.Object, entity.EntityType);
        Assert.Equal(SpawnMethod.AtMarker, entity.SpawnMethod);
        Assert.Equal("OBJECT_CHEST_SILVER", entity.DefName);
        Assert.Equal("RewardObject", entity.ScriptName);
        Assert.NotNull(entity.ObjectReward);
        Assert.True(viewModel.HasObjectReward);
        Assert.Equal(100, viewModel.ObjectRewardGold);
        Assert.Contains("OBJECT_HEALTH_POTION", viewModel.ObjectRewardItems);
    }

    [Fact]
    public void EntityTab_ObjectReward_AddsAndRemovesItems()
    {
        QuestEntity entity = new QuestEntity
        {
            EntityType = EntityType.Object,
            DefName = "OBJECT_CHEST"
        };
        EntityTabViewModel viewModel = new EntityTabViewModel(entity);

        Assert.True(viewModel.IsObjectRewardSupported);

        viewModel.HasObjectReward = true;
        viewModel.NewRewardItem = "OBJECT_APPLE";
        viewModel.AddRewardItemCommand.Execute(null);

        Assert.Single(viewModel.ObjectRewardItems);
        Assert.Contains("OBJECT_APPLE", entity.ObjectReward!.Items);

        viewModel.RemoveRewardItemCommand.Execute("OBJECT_APPLE");

        Assert.Empty(viewModel.ObjectRewardItems);
        Assert.Empty(entity.ObjectReward!.Items);
    }
}
