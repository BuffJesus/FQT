using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Handles saving and loading quest projects to/from .fqtproj files.
///
/// ProjectFileService manages the persistence of QuestProject objects as JSON files.
/// The .fqtproj format stores the complete quest definition including all entities,
/// behavior nodes, connections, states, rewards, and configuration.
/// </summary>
/// <remarks>
/// File format: JSON with pretty-printing enabled for readability.
/// The file wraps the QuestProject in a ProjectFileData container for
/// future extensibility (adding metadata, version info, etc.).
///
/// Serialization options:
/// - WriteIndented: True for human-readable JSON
/// - IncludeFields: False to avoid serializing private backing fields from MVVM
/// - PropertyNameCaseInsensitive: True for flexible deserialization
/// </remarks>
public sealed class ProjectFileService
{
    /// <summary>
    /// JSON serialization options configured for .fqtproj format.
    /// </summary>
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        IncludeFields = false, // Don't serialize private backing fields
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true // Allow case-insensitive deserialization
    };

    /// <summary>
    /// Saves a quest project to a .fqtproj file.
    /// Creates the parent directory if it doesn't exist.
    /// </summary>
    /// <param name="path">Full path to the .fqtproj file</param>
    /// <param name="project">The quest project to save</param>
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

    /// <summary>
    /// Loads a quest project from a .fqtproj file.
    /// </summary>
    /// <param name="path">Full path to the .fqtproj file</param>
    /// <returns>The loaded QuestProject, or a new empty project if data is missing</returns>
    /// <exception cref="InvalidDataException">Thrown if the file cannot be parsed</exception>
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
