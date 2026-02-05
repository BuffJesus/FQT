namespace FableQuestTool.ViewModels;

/// <summary>
/// Describes an exposed variable from another entity.
/// </summary>
public sealed class ExternalVariableInfo
{
    /// <summary>
    /// Creates a new instance of ExternalVariableInfo.
    /// </summary>
    public ExternalVariableInfo(string entityScriptName, string variableName, string variableType, string defaultValue)
    {
        EntityScriptName = entityScriptName;
        VariableName = variableName;
        VariableType = variableType;
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Gets EntityScriptName.
    /// </summary>
    public string EntityScriptName { get; }
    /// <summary>
    /// Gets VariableName.
    /// </summary>
    public string VariableName { get; }
    /// <summary>
    /// Gets VariableType.
    /// </summary>
    public string VariableType { get; }
    /// <summary>
    /// Gets DefaultValue.
    /// </summary>
    public string DefaultValue { get; }
}
