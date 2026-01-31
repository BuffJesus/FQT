namespace FableQuestTool.Models;

public sealed class QuestState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "bool";
    public bool Persist { get; set; } = true;
    public object? DefaultValue { get; set; }
}
