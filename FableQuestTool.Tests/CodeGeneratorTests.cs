using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class CodeGeneratorTests
{
    [Fact]
    public void GenerateOnPersist_IncludesStateRoundTrip()
    {
        QuestProject project = new QuestProject();
        project.States.Add(new QuestState { Name = "TestBool", Type = "bool", Persist = true, DefaultValue = true });
        project.States.Add(new QuestState { Name = "TestInt", Type = "int", Persist = true, DefaultValue = 3 });
        project.States.Add(new QuestState { Name = "TestFloat", Type = "float", Persist = true, DefaultValue = 1.5 });
        project.States.Add(new QuestState { Name = "TestString", Type = "string", Persist = true, DefaultValue = "hello" });

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(project);

        Assert.Contains("Quest:PersistTransferBool(context, \"TestBool\")", script);
        Assert.Contains("Quest:PersistTransferInt(context, \"TestInt\")", script);
        Assert.Contains("Quest:PersistTransferFloat(context, \"TestFloat\")", script);
        Assert.Contains("Quest:PersistTransferString(context, \"TestString\")", script);
    }

    [Fact]
    public void GenerateQuestScript_IncludesUserThreadLoop()
    {
        QuestProject project = new QuestProject { Name = "ThreadQuest" };
        project.Threads.Add(new QuestThread
        {
            FunctionName = "MonitorStuff",
            Region = "OAKVALE",
            Description = "Background monitor thread",
            IntervalSeconds = 1.25f,
            ExitStateName = "StopThread",
            ExitStateValue = true
        });

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(project);

        Assert.Contains("function MonitorStuff(questObject)", script);
        Assert.Contains("-- Background monitor thread", script);
        Assert.Contains("-- TODO: Implement thread logic", script);
    }

    [Fact]
    public void GenerateQuestScript_IncludesQuestCardAndRewards()
    {
        QuestProject project = new QuestProject
        {
            Name = "RewardQuest",
            DisplayName = "Reward Quest",
            QuestCardObject = "OBJECT_QUEST_CARD_GENERIC",
            ObjectiveText = "Do the thing",
            UseQuestStartScreen = true,
            UseQuestEndScreen = true,
            IsStoryQuest = false,
            IsGoldQuest = true
        };
        project.Regions.Add("OAKVALE");
        project.Rewards.Gold = 250;
        project.Rewards.Renown = 500;
        project.Rewards.Items.Add("OBJECT_APPLE");
        project.Rewards.Items.Add("OBJECT_CARROT");

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(project);

        Assert.Contains("Quest:AddQuestCard(\"OBJECT_QUEST_CARD_GENERIC\", \"RewardQuest\"", script);
        Assert.Contains("Quest:SetQuestCardObjective(\"RewardQuest\"", script);
        Assert.Contains("Quest:SetQuestGoldReward(\"RewardQuest\", 250)", script);
        Assert.Contains("Quest:SetQuestRenownReward(\"RewardQuest\", 500)", script);
        Assert.Contains("Quest:KickOffQuestStartScreen(questName, true, true)", script);
        Assert.Contains("Quest:SetQuestAsCompleted(\"RewardQuest\", true, false, false)", script);
        Assert.Contains("Quest:GiveHeroObject(\"OBJECT_APPLE\", 1)", script);
        Assert.Contains("Quest:GiveHeroObject(\"OBJECT_CARROT\", 1)", script);
    }

    [Fact]
    public void GenerateQuestScript_BindsEntitiesAndContainer()
    {
        QuestProject project = new QuestProject { Name = "BindQuest" };
        project.Entities.Add(new QuestEntity { ScriptName = "NPC_One" });
        project.Rewards.Container = new ContainerReward
        {
            ContainerScriptName = "RewardChest",
            AutoGiveOnComplete = false
        };
        project.Rewards.Container.Items.Add("OBJECT_APPLE");

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(project);

        Assert.Contains("Quest:AddEntityBinding(\"NPC_One\", \"BindQuest/Entities/NPC_One\")", script);
        Assert.Contains("Quest:AddEntityBinding(\"RewardChest\", \"BindQuest/Entities/RewardChest\")", script);
        Assert.Contains("Quest:FinalizeEntityBindings()", script);
    }
}
