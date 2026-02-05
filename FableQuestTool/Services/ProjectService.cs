using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Holds the active quest project for editor sessions.
/// </summary>
public sealed class ProjectService : IProjectService
{
    /// <summary>
    /// Gets the currently loaded project.
    /// </summary>
    public QuestProject CurrentProject { get; private set; } = new();

    /// <summary>
    /// Creates and sets a new empty project.
    /// </summary>
    public QuestProject CreateNew()
    {
        CurrentProject = new QuestProject();
        return CurrentProject;
    }
}
