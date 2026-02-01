using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

public sealed class ProjectFileService
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        IncludeFields = false, // Don't serialize private backing fields
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true // Allow case-insensitive deserialization
    };

    public void Save(string path, QuestProject project)
    {
        ProjectFileData data = new()
        {
            Project = project
        };

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(path, json);
    }

    public QuestProject Load(string path)
    {
        string json = File.ReadAllText(path);
        ProjectFileData? data = JsonSerializer.Deserialize<ProjectFileData>(json, options);
        if (data == null)
        {
            throw new InvalidDataException("Project data is invalid.");
        }

        return data.Project ?? new QuestProject();
    }
}
