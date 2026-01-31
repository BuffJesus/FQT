namespace FableQuestTool.Models;

public class ApiFunction
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<ApiParameter> Parameters { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
}

public class ApiParameter
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}
