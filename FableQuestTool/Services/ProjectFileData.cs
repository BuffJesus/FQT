using System.Text.Json.Serialization;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

public sealed class ProjectFileData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.1";

    [JsonPropertyName("project")]
    public QuestProject Project { get; set; } = new();
}
