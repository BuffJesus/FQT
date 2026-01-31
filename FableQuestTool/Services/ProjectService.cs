using FableQuestTool.Models;

namespace FableQuestTool.Services;

public sealed class ProjectService : IProjectService
{
    public QuestProject CurrentProject { get; private set; } = new();

    public QuestProject CreateNew()
    {
        CurrentProject = new QuestProject();
        return CurrentProject;
    }
}
