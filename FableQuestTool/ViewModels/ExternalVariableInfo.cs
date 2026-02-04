namespace FableQuestTool.ViewModels;

public sealed class ExternalVariableInfo
{
    public ExternalVariableInfo(string entityScriptName, string variableName, string variableType, string defaultValue)
    {
        EntityScriptName = entityScriptName;
        VariableName = variableName;
        VariableType = variableType;
        DefaultValue = defaultValue;
    }

    public string EntityScriptName { get; }
    public string VariableName { get; }
    public string VariableType { get; }
    public string DefaultValue { get; }
}
