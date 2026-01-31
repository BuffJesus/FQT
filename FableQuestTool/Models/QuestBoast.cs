namespace FableQuestTool.Models;

public sealed class QuestBoast
{
    public int BoastId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int RenownReward { get; set; }
    public int GoldReward { get; set; }
    public bool IsBoast { get; set; }
    public int TextId { get; set; }
}
