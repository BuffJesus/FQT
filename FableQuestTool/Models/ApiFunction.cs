namespace FableQuestTool.Models;

/// <summary>
/// Represents a function from the FSE Lua API.
///
/// ApiFunction models the metadata for functions exposed by the Fable Script
/// Extender to Lua scripts. This information is parsed from the FSE source
/// code and displayed in the API Reference panel to help quest authors.
/// </summary>
/// <remarks>
/// API functions are organized into categories:
/// - Quest: Functions on the Quest object (Quest:SetState, Quest:Complete, etc.)
/// - Entity: Functions on Entity objects (Entity:MoveTo, Entity:Say, etc.)
/// - Hero: Functions for player interaction (Hero:GetGold, Hero:AddItem, etc.)
/// - World: Global functions (GetEntity, CreateCreature, etc.)
/// - UI: User interface functions (ShowMessage, ShowTimer, etc.)
///
/// The ApiParser service extracts this information from C++ source comments.
/// </remarks>
public class ApiFunction
{
    /// <summary>
    /// Name of the function as called in Lua code.
    /// May include the object prefix (e.g., "Quest:SetState" or "Entity:MoveTo").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Return type of the function.
    /// Lua types: "nil", "boolean", "number", "string", "table", "Entity", etc.
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>
    /// Category for organizing functions in the API reference browser.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// List of parameters accepted by this function.
    /// </summary>
    public List<ApiParameter> Parameters { get; set; } = new();

    /// <summary>
    /// Description of what this function does.
    /// Extracted from source code documentation comments.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Example Lua code showing how to use this function.
    /// </summary>
    public string Example { get; set; } = string.Empty;
}

/// <summary>
/// Represents a parameter for an FSE API function.
/// </summary>
public class ApiParameter
{
    /// <summary>
    /// Lua type of the parameter.
    /// Common types: "string", "number", "boolean", "Entity", "table"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Name of the parameter as shown in documentation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this parameter is optional.
    /// Optional parameters have default values if not provided.
    /// </summary>
    public bool IsOptional { get; set; }
}
