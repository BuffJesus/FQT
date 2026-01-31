namespace FableQuestTool.Models;

public sealed class NodeConnection
{
    public string FromNodeId { get; set; } = string.Empty;
    public string FromPort { get; set; } = "output";
    public string ToNodeId { get; set; } = string.Empty;
    public string ToPort { get; set; } = "input";
}
