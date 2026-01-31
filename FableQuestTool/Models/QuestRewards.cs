using System.Collections.ObjectModel;

namespace FableQuestTool.Models;

public sealed class QuestRewards
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public int Renown { get; set; }
    public float Morality { get; set; }
    public ObservableCollection<string> Items { get; set; } = new();
    public ObservableCollection<string> Abilities { get; set; } = new();
}
