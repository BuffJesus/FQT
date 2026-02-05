using System.Text.Json.Serialization;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Wrapper used for .fqtproj serialization (version + project payload).
/// </summary>
public sealed class ProjectFileData
{
    /// <summary>
    /// File format version for forward compatibility.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.1";

    /// <summary>
    /// Serialized quest project payload.
    /// </summary>
    [JsonPropertyName("project")]
    public QuestProject Project { get; set; } = new();
}
