using System.Text.Json.Serialization;

namespace FableQuestTool.Models;

/// <summary>
/// Represents a user-defined, entity-local variable for the node graph.
/// </summary>
public sealed class EntityVariable
{
    /// <summary>
    /// Gets or sets Name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Type.
    /// </summary>
    [JsonPropertyName("Type")]
    public string Type { get; set; } = "String";

    /// <summary>
    /// Gets or sets DefaultValue.
    /// </summary>
    [JsonPropertyName("DefaultValue")]
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets IsExposed.
    /// </summary>
    [JsonPropertyName("IsExposed")]
    public bool IsExposed { get; set; }
}
