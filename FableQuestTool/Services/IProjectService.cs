using FableQuestTool.Models;

namespace FableQuestTool.Services;

public interface IProjectService
{
    QuestProject CurrentProject { get; }
    QuestProject CreateNew();
}
