using System.IO;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Exports quests and entity scripts to a folder structure.
/// </summary>
public sealed class ExportService
{
    private readonly CodeGenerator codeGenerator;

    /// <summary>
    /// Creates a new instance of ExportService.
    /// </summary>
    public ExportService(CodeGenerator codeGenerator)
    {
        this.codeGenerator = codeGenerator;
    }

    /// <summary>
    /// Writes quest Lua files and returns the output folder path.
    /// </summary>
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

        // Generate container entity script if needed (for manual-opening containers with multiple items)
        if (codeGenerator.NeedsContainerEntityScript(quest))
        {
            var container = quest.Rewards.Container!;
            string containerPath = Path.Combine(entitiesFolder, $"{container.ContainerScriptName}.lua");
            File.WriteAllText(containerPath, codeGenerator.GenerateContainerEntityScript(quest, container));
        }

        string registrationPath = Path.Combine(questFolder, "_quests_registration.lua");
        File.WriteAllText(registrationPath, codeGenerator.GenerateRegistrationSnippet(quest));

        return questFolder;
    }
}
