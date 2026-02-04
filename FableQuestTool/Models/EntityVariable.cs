using System.Text.Json.Serialization;

namespace FableQuestTool.Models;

/// <summary>
/// Represents a user-defined, entity-local variable for the node graph.
/// </summary>
public sealed class EntityVariable
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = "String";

    [JsonPropertyName("DefaultValue")]
    public string DefaultValue { get; set; } = string.Empty;

    [JsonPropertyName("IsExposed")]
    public bool IsExposed { get; set; }
}
