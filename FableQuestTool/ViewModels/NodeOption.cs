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
}
