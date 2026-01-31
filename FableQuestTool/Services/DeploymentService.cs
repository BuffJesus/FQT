using FableQuestTool.Config;
using FableQuestTool.Formats;
using FableQuestTool.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FableQuestTool.Services;

public sealed class DeploymentService
{
    private readonly FableConfig config;
    private readonly CodeGenerator codeGenerator;

    public DeploymentService(FableConfig config, CodeGenerator codeGenerator)
    {
        this.config = config;
        this.codeGenerator = codeGenerator;
    }

    public bool DeployQuest(QuestProject quest, out string message)
    {
        message = string.Empty;

        if (!config.EnsureFablePath())
        {
            message = "Fable installation path not configured.";
            return false;
        }

        string? fseFolder = config.GetFseFolder();
        if (string.IsNullOrWhiteSpace(fseFolder))
        {
            message = "Could not determine FSE folder path.";
            return false;
        }

        try
        {
            // Create FSE folder if it doesn't exist
            Directory.CreateDirectory(fseFolder);

            // Create quest folder
            string questFolder = Path.Combine(fseFolder, quest.Name);
            Directory.CreateDirectory(questFolder);

            // Create entities folder
            string entitiesFolder = Path.Combine(questFolder, "Entities");
            Directory.CreateDirectory(entitiesFolder);

            // Generate and save quest script
            string questPath = Path.Combine(questFolder, $"{quest.Name}.lua");
            File.WriteAllText(questPath, codeGenerator.GenerateQuestScript(quest));

            // Generate and save entity scripts
            foreach (QuestEntity entity in quest.Entities)
            {
                string entityPath = Path.Combine(entitiesFolder, $"{entity.ScriptName}.lua");
                File.WriteAllText(entityPath, codeGenerator.GenerateEntityScript(quest, entity));
            }

            // Register quest in quests.lua
            if (!RegisterInQuestsLua(fseFolder, quest, out string? questsError))
            {
                message = $"Quest files deployed but failed to register in quests.lua: {questsError}";
                return false;
            }

            // Register quest in FinalAlbion.qst
            if (!RegisterInFinalAlbion(quest.Name, out string? qstError))
            {
                message = $"Quest files deployed but failed to register in FinalAlbion.qst: {qstError}";
                return false;
            }

            message = $"Quest '{quest.Name}' deployed successfully to:\n{questFolder}";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Deployment failed: {ex.Message}";
            return false;
        }
    }

    private bool RegisterInQuestsLua(string fseFolder, QuestProject quest, out string? error)
    {
        error = null;
        string questsLuaPath = Path.Combine(fseFolder, "quests.lua");

        try
        {
            // Create quests.lua if it doesn't exist
            if (!File.Exists(questsLuaPath))
            {
                File.WriteAllText(questsLuaPath, "Quests = {}\n");
            }

            string content = File.ReadAllText(questsLuaPath);

            // Check if quest is already registered
            string pattern = $@"{quest.Name}\s*=\s*{{";
            if (Regex.IsMatch(content, pattern, RegexOptions.Multiline))
            {
                // Quest already registered, update it
                string questEntry = GenerateQuestLuaEntry(quest);
                content = Regex.Replace(content, $@"{quest.Name}\s*=\s*{{[^}}]*}},?", questEntry, RegexOptions.Singleline);
            }
            else
            {
                // Add new quest entry
                string questEntry = GenerateQuestLuaEntry(quest);

                // Find the Quests table and add the entry
                if (content.Contains("Quests = {"))
                {
                    int insertPos = content.IndexOf("Quests = {") + "Quests = {".Length;
                    content = content.Insert(insertPos, "\n" + questEntry);
                }
                else
                {
                    content += "\nQuests = {\n" + questEntry + "\n}\n";
                }
            }

            File.WriteAllText(questsLuaPath, content);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private string GenerateQuestLuaEntry(QuestProject quest)
    {
        return $@"    {quest.Name} = {{
        QuestName = ""{quest.Name}"",
        QuestID = {quest.Id},
        Folder = ""{quest.Name}""
    }},";
    }

    private bool RegisterInFinalAlbion(string questName, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(config.FablePath))
        {
            error = "Fable path not configured.";
            return false;
        }

        string qstPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion.qst");
        if (!File.Exists(qstPath))
        {
            error = $"FinalAlbion.qst not found at: {qstPath}";
            return false;
        }

        try
        {
            QstFile qstFile = QstFile.Load(qstPath);

            if (qstFile.HasQuest(questName))
            {
                // Already registered
                return true;
            }

            qstFile.AddQuestIfMissing(questName, false);
            qstFile.Save();

            return true;
        }
        catch (Exception ex)
        {
            error = $"Failed to register in FinalAlbion.qst: {ex.Message}";
            return false;
        }
    }

    public bool LaunchFse(out string message)
    {
        message = string.Empty;

        if (!config.EnsureFablePath())
        {
            message = "Fable installation path not configured.";
            return false;
        }

        string? launcherPath = config.GetFseLauncherPath();
        if (string.IsNullOrWhiteSpace(launcherPath))
        {
            message = "FSE_Launcher.exe not found in Fable installation folder.\nPlease install FSE first.";
            return false;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = launcherPath,
                WorkingDirectory = Path.GetDirectoryName(launcherPath),
                UseShellExecute = true
            };

            Process.Start(startInfo);
            message = "FSE launched successfully.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Failed to launch FSE: {ex.Message}";
            return false;
        }
    }
}
