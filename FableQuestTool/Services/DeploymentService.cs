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
            if (!RegisterInFinalAlbion(quest.Name, quest.IsEnabled, out string? qstError))
            {
                message = $"Quest files deployed but failed to register in FinalAlbion.qst: {qstError}";
                return false;
            }

            // Add quest activation to FSE_Master.lua
            if (!AddQuestActivationToMaster(fseFolder, quest.Name, out string? masterError))
            {
                message = $"Quest deployed but failed to add activation to FSE_Master.lua: {masterError}\n\n" +
                         "You may need to manually add:\n" +
                         $"quest:ActivateQuest(\"{quest.Name}\")\n" +
                         "to FSE_Master.lua Main() function.";
                // Don't return false - this is a non-critical warning
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
            string pattern = $@"^\s*{Regex.Escape(quest.Name)}\s*=\s*\{{";
            var match = Regex.Match(content, pattern, RegexOptions.Multiline);

            if (match.Success)
            {
                // Quest already registered, find and replace the entire entry
                int startPos = match.Index;
                int braceCount = 0;
                int currentPos = content.IndexOf('{', startPos);

                if (currentPos == -1)
                {
                    error = "Failed to parse quest entry in quests.lua";
                    return false;
                }

                // Find matching closing brace by counting braces
                for (int i = currentPos; i < content.Length; i++)
                {
                    if (content[i] == '{') braceCount++;
                    else if (content[i] == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            // Found the end of this quest entry
                            int endPos = i + 1;

                            // Include trailing comma if present
                            if (endPos < content.Length && content[endPos] == ',')
                                endPos++;

                            // Replace the entire quest entry
                            string questEntry = GenerateQuestLuaEntry(quest);
                            content = content.Remove(startPos, endPos - startPos);
                            content = content.Insert(startPos, questEntry);
                            break;
                        }
                    }
                }
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
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"    {quest.Name} = {{");
        sb.AppendLine($"        name = \"{quest.Name}\",");
        sb.AppendLine($"        file = \"{quest.Name}/{quest.Name}\",");
        sb.AppendLine($"        id = {quest.Id},");
        sb.AppendLine();
        sb.AppendLine("        entity_scripts = {");

        int entityId = quest.Id + 1;
        foreach (var entity in quest.Entities)
        {
            sb.AppendLine($"            {{ name = \"{entity.ScriptName}\", file = \"{quest.Name}/Entities/{entity.ScriptName}\", id = {entityId} }},");
            entityId++;
        }

        sb.AppendLine("        }");
        sb.Append("    },");

        return sb.ToString();
    }

    private bool AddQuestActivationToMaster(string fseFolder, string questName, out string? error)
    {
        error = null;
        string masterPath = Path.Combine(fseFolder, "Master", "FSE_Master.lua");

        if (!File.Exists(masterPath))
        {
            error = "FSE_Master.lua not found. Quest will not auto-activate.";
            return false;
        }

        try
        {
            string content = File.ReadAllText(masterPath);

            // Check if quest is already activated
            string activationPattern = $@"quest:ActivateQuest\(""{questName}""\)";
            if (Regex.IsMatch(content, Regex.Escape(activationPattern)))
            {
                // Already activated
                return true;
            }

            // Find the Main function and add activation before the end
            int mainFuncStart = content.IndexOf("function Main(quest)");
            if (mainFuncStart == -1)
            {
                error = "Could not find Main function in FSE_Master.lua";
                return false;
            }

            // Find the end of Main function
            int mainFuncEnd = content.IndexOf("end", mainFuncStart);
            if (mainFuncEnd == -1)
            {
                error = "Could not find end of Main function in FSE_Master.lua";
                return false;
            }

            // Insert activation code before the "end"
            string activation = $"\n    -- Auto-activate {questName}\n" +
                               $"    quest:ActivateQuest(\"{questName}\")\n" +
                               $"    quest:Log(\"FSE_Master: Activated {questName}\")\n";

            content = content.Insert(mainFuncEnd, activation);
            File.WriteAllText(masterPath, content);

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool RegisterInFinalAlbion(string questName, bool isEnabled, out string? error)
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

            // Ensure FSE_Master is always registered and enabled (TRUE)
            if (!qstFile.HasQuest("FSE_Master"))
            {
                qstFile.AddQuestIfMissing("FSE_Master", true);
            }

            if (qstFile.HasQuest(questName))
            {
                // Update existing quest with new enabled status
                qstFile.UpdateQuestStatus(questName, isEnabled);
                qstFile.Save();
                return true;
            }

            qstFile.AddQuestIfMissing(questName, isEnabled);
            qstFile.Save();

            return true;
        }
        catch (Exception ex)
        {
            error = $"Failed to register in FinalAlbion.qst: {ex.Message}";
            return false;
        }
    }

    public bool DeleteQuest(string questName, out string message)
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
            bool filesDeleted = false;
            bool questsLuaUpdated = false;
            bool qstFileUpdated = false;

            // Delete quest folder from FSE
            string questFolder = Path.Combine(fseFolder, questName);
            if (Directory.Exists(questFolder))
            {
                Directory.Delete(questFolder, true);
                filesDeleted = true;
            }

            // Remove from quests.lua
            string questsLuaPath = Path.Combine(fseFolder, "quests.lua");
            if (File.Exists(questsLuaPath))
            {
                string content = File.ReadAllText(questsLuaPath);
                string pattern = $@"^\s*{Regex.Escape(questName)}\s*=\s*\{{";
                var match = Regex.Match(content, pattern, RegexOptions.Multiline);

                if (match.Success)
                {
                    int startPos = match.Index;
                    int braceCount = 0;
                    int currentPos = content.IndexOf('{', startPos);

                    if (currentPos != -1)
                    {
                        // Find matching closing brace by counting braces
                        for (int i = currentPos; i < content.Length; i++)
                        {
                            if (content[i] == '{') braceCount++;
                            else if (content[i] == '}')
                            {
                                braceCount--;
                                if (braceCount == 0)
                                {
                                    // Found the end of this quest entry
                                    int endPos = i + 1;

                                    // Include trailing comma and newline if present
                                    if (endPos < content.Length && content[endPos] == ',')
                                        endPos++;
                                    if (endPos < content.Length && content[endPos] == '\n')
                                        endPos++;

                                    // Remove the entire quest entry
                                    content = content.Remove(startPos, endPos - startPos);
                                    File.WriteAllText(questsLuaPath, content);
                                    questsLuaUpdated = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Remove from FinalAlbion.qst
            string qstPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion.qst");
            if (File.Exists(qstPath))
            {
                try
                {
                    QstFile qstFile = QstFile.Load(qstPath);
                    if (qstFile.RemoveQuest(questName))
                    {
                        qstFile.Save();
                        qstFileUpdated = true;
                    }
                }
                catch
                {
                    // If QST removal fails, continue anyway
                }
            }

            // Remove from FSE_Master.lua
            bool masterUpdated = false;
            string masterPath = Path.Combine(fseFolder, "Master", "FSE_Master.lua");
            if (File.Exists(masterPath))
            {
                try
                {
                    string content = File.ReadAllText(masterPath);
                    // Remove the activation block (including comments and log)
                    string pattern = $@"\s*--\s*Auto-activate {Regex.Escape(questName)}.*?\n\s*quest:ActivateQuest\(""{Regex.Escape(questName)}""\).*?\n\s*quest:Log\(""FSE_Master: Activated {Regex.Escape(questName)}""\).*?\n";
                    string newContent = Regex.Replace(content, pattern, "", RegexOptions.Singleline);

                    if (newContent != content)
                    {
                        File.WriteAllText(masterPath, newContent);
                        masterUpdated = true;
                    }
                }
                catch
                {
                    // If Master removal fails, continue anyway
                }
            }

            if (!filesDeleted && !questsLuaUpdated && !qstFileUpdated && !masterUpdated)
            {
                message = $"Quest '{questName}' was not found in Fable installation.";
                return false;
            }

            message = $"Quest '{questName}' deleted successfully.\n\n" +
                     $"Files deleted: {(filesDeleted ? "Yes" : "No")}\n" +
                     $"quests.lua updated: {(questsLuaUpdated ? "Yes" : "Not found")}\n" +
                     $"FinalAlbion.qst updated: {(qstFileUpdated ? "Yes" : "Not found")}\n" +
                     $"FSE_Master.lua updated: {(masterUpdated ? "Yes" : "Not found")}";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Deletion failed: {ex.Message}";
            return false;
        }
    }

    public bool ToggleQuest(string questName, bool enable, out string message)
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
            bool questsLuaUpdated = false;
            bool qstFileUpdated = false;
            bool masterUpdated = false;

            // Toggle in quests.lua
            string questsLuaPath = Path.Combine(fseFolder, "quests.lua");
            if (File.Exists(questsLuaPath))
            {
                questsLuaUpdated = ToggleQuestInLuaFile(questsLuaPath, questName, enable);
            }

            // Toggle in FinalAlbion.qst
            if (!string.IsNullOrWhiteSpace(config.FablePath))
            {
                string qstPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion.qst");
                if (File.Exists(qstPath))
                {
                    QstFile qstFile = QstFile.Load(qstPath);
                    if (enable)
                    {
                        if (!qstFile.HasQuest(questName))
                        {
                            qstFile.AddQuestIfMissing(questName, true);
                        }
                        else
                        {
                            qstFile.UpdateQuestStatus(questName, true);
                        }
                    }
                    else
                    {
                        qstFile.RemoveQuest(questName);
                    }
                    qstFile.Save();
                    qstFileUpdated = true;
                }
            }

            // Toggle in FSE_Master.lua
            string masterLuaPath = Path.Combine(fseFolder, "Master", "FSE_Master.lua");
            if (File.Exists(masterLuaPath))
            {
                masterUpdated = ToggleMasterActivation(masterLuaPath, questName, enable);
            }

            message = $"Quest '{questName}' {(enable ? "enabled" : "disabled")} successfully.\n\n" +
                     $"quests.lua updated: {(questsLuaUpdated ? "Yes" : "Not found")}\n" +
                     $"FinalAlbion.qst updated: {(qstFileUpdated ? "Yes" : "Not found")}\n" +
                     $"FSE_Master.lua updated: {(masterUpdated ? "Yes" : "Not found")}";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Toggle failed: {ex.Message}";
            return false;
        }
    }

    private bool ToggleQuestInLuaFile(string filePath, string questName, bool enable)
    {
        string content = File.ReadAllText(filePath);
        string pattern = $@"^(\s*)(--\s*)?({Regex.Escape(questName)}\s*=\s*\{{)";

        bool found = false;
        var lines = content.Split('\n');
        int braceDepth = 0;
        bool inTargetQuest = false;
        int questStartLine = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            // Check if this line starts our target quest
            var match = Regex.Match(line, pattern, RegexOptions.Multiline);
            if (match.Success && !inTargetQuest)
            {
                inTargetQuest = true;
                questStartLine = i;
                found = true;

                bool isCommented = match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value);

                if (enable && isCommented)
                {
                    // Uncomment the line
                    lines[i] = Regex.Replace(line, @"^(\s*)--\s*", "$1");
                }
                else if (!enable && !isCommented)
                {
                    // Comment the line
                    lines[i] = Regex.Replace(line, @"^(\s*)", "$1-- ");
                }

                braceDepth = 1;
                continue;
            }

            if (inTargetQuest)
            {
                // Count braces to track quest block
                foreach (char c in trimmed)
                {
                    if (c == '{') braceDepth++;
                    else if (c == '}') braceDepth--;
                }

                // Comment/uncomment lines within the quest block
                if (braceDepth > 0)
                {
                    bool isCommented = trimmed.StartsWith("--");
                    if (enable && isCommented)
                    {
                        lines[i] = Regex.Replace(line, @"^(\s*)--\s*", "$1");
                    }
                    else if (!enable && !isCommented && !string.IsNullOrWhiteSpace(trimmed))
                    {
                        lines[i] = Regex.Replace(line, @"^(\s*)", "$1-- ");
                    }
                }

                // Check if we've closed the quest block
                if (braceDepth == 0)
                {
                    inTargetQuest = false;
                }
            }
        }

        if (found)
        {
            File.WriteAllText(filePath, string.Join("\n", lines));
        }

        return found;
    }

    private bool ToggleMasterActivation(string masterPath, string questName, bool enable)
    {
        string content = File.ReadAllText(masterPath);
        string pattern = $@"^(\s*)(--\s*)?(Quest:ActivateQuest\(""{Regex.Escape(questName)}""\))";

        bool found = false;
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var match = Regex.Match(lines[i], pattern);
            if (match.Success)
            {
                found = true;
                bool isCommented = match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value);

                if (enable && isCommented)
                {
                    // Uncomment
                    lines[i] = Regex.Replace(lines[i], @"^(\s*)--\s*", "$1");
                }
                else if (!enable && !isCommented)
                {
                    // Comment
                    lines[i] = Regex.Replace(lines[i], @"^(\s*)", "$1-- ");
                }
                break;
            }
        }

        if (found)
        {
            File.WriteAllText(masterPath, string.Join("\n", lines));
        }

        return found;
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
