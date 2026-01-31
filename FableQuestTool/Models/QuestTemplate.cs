namespace FableQuestTool.Models;

public class QuestTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public QuestProject Template { get; set; } = new();
}
