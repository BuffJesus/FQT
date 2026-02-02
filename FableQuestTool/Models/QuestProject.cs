using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.Models;

public sealed partial class QuestProject : ObservableObject
{
    [ObservableProperty]
    [property: JsonPropertyName("Name")]
    private string name = "NewQuest";

    [ObservableProperty]
    [property: JsonPropertyName("Id")]
    private int id = 50000;
    public string DisplayName { get; set; } = "New Quest";
    public string Description { get; set; } = string.Empty;
    public ObservableCollection<string> Regions { get; set; } = new();

    public string QuestCardObject { get; set; } = "OBJECT_QUEST_CARD_GENERIC";
    public string ObjectiveText { get; set; } = string.Empty;
    public string ObjectiveRegion1 { get; set; } = string.Empty;
    public string ObjectiveRegion2 { get; set; } = string.Empty;
    
    [ObservableProperty]
    [property: JsonPropertyName("WorldMapOffsetX")]
    private int worldMapOffsetX;
    
    [ObservableProperty]
    [property: JsonPropertyName("WorldMapOffsetY")]
    private int worldMapOffsetY;

    public bool UseQuestStartScreen { get; set; }
    public bool UseQuestEndScreen { get; set; }
    public bool IsStoryQuest { get; set; }
    public bool IsGoldQuest { get; set; }
    public bool GiveCardDirectly { get; set; }
    public bool IsGuildQuest { get; set; } = false;
    public bool IsEnabled { get; set; } = true;

    public QuestRewards Rewards { get; set; } = new();
    public ObservableCollection<QuestBoast> Boasts { get; set; } = new();
    public ObservableCollection<QuestState> States { get; set; } = new();
    public ObservableCollection<QuestEntity> Entities { get; set; } = new();
    public ObservableCollection<QuestThread> Threads { get; set; } = new();
}
