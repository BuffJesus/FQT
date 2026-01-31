using System.IO;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

public sealed class ExportService
{
    private readonly CodeGenerator codeGenerator;

    public ExportService(CodeGenerator codeGenerator)
    {
        this.codeGenerator = codeGenerator;
    }

    public string Export(QuestProject quest, string baseDirectory)
    {
        string questFolder = Path.Combine(baseDirectory, quest.Name);
        string entitiesFolder = Path.Combine(questFolder, "Entities");
        Directory.CreateDirectory(entitiesFolder);

        string questPath = Path.Combine(questFolder, $"{quest.Name}.lua");
        File.WriteAllText(questPath, codeGenerator.GenerateQuestScript(quest));

        foreach (QuestEntity entity in quest.Entities)
        {
            string entityPath = Path.Combine(entitiesFolder, $"{entity.ScriptName}.lua");
            File.WriteAllText(entityPath, codeGenerator.GenerateEntityScript(quest, entity));
        }

        string registrationPath = Path.Combine(questFolder, "_quests_registration.lua");
        File.WriteAllText(registrationPath, codeGenerator.GenerateRegistrationSnippet(quest));

        return questFolder;
    }
}
