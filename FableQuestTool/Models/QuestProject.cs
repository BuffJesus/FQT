using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FableQuestTool.Models;

public sealed class QuestProject
{
    public string Name { get; set; } = "NewQuest";
    public int Id { get; set; } = 50000;
    public string DisplayName { get; set; } = "New Quest";
    public string Description { get; set; } = string.Empty;
    public ObservableCollection<string> Regions { get; set; } = new();

    public string QuestCardObject { get; set; } = "OBJECT_QUEST_CARD_GENERIC";
    public string ObjectiveText { get; set; } = string.Empty;
    public string ObjectiveRegion1 { get; set; } = string.Empty;
    public string ObjectiveRegion2 { get; set; } = string.Empty;
    public int WorldMapOffsetX { get; set; }
    public int WorldMapOffsetY { get; set; }

    public bool UseQuestStartScreen { get; set; }
    public bool IsStoryQuest { get; set; }
    public bool IsGoldQuest { get; set; }
    public bool GiveCardDirectly { get; set; }

    public QuestRewards Rewards { get; set; } = new();
    public ObservableCollection<QuestBoast> Boasts { get; set; } = new();
    public ObservableCollection<QuestState> States { get; set; } = new();
    public ObservableCollection<QuestEntity> Entities { get; set; } = new();
    public ObservableCollection<QuestThread> Threads { get; set; } = new();
}
