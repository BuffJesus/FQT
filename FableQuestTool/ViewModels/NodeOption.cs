using FableQuestTool.Data;

namespace FableQuestTool.ViewModels;

public sealed class NodeOption
{
    public NodeOption(string label, string category, string icon)
    {
        Label = label;
        Category = category;
        Icon = icon;
    }

    public string Label { get; }
    public string Category { get; }
    public string Icon { get; }
    public string Type { get; init; } = string.Empty;
    public NodeDefinition? Definition { get; init; }
    public int MenuIndex { get; set; } = -1;
}
