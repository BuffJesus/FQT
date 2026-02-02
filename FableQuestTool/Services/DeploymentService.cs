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

        // Check and install FSE if needed
        if (!EnsureFseInstalled(out string? fseError))
        {
            message = $"FSE installation check failed: {fseError}";
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

            // Generate container entity script if needed (for manual-opening containers with multiple items)
            if (codeGenerator.NeedsContainerEntityScript(quest))
            {
                var container = quest.Rewards.Container!;
                string containerPath = Path.Combine(entitiesFolder, $"{container.ContainerScriptName}.lua");
                File.WriteAllText(containerPath, codeGenerator.GenerateContainerEntityScript(quest, container));
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

        // Add container entity script if needed (for manual-opening containers)
        bool needsContainerEntity = quest.Rewards.Container != null &&
                                    quest.Rewards.Container.Items.Count > 0 &&
                                    !quest.Rewards.Container.AutoGiveOnComplete;
        if (needsContainerEntity)
        {
            var container = quest.Rewards.Container!;
            sb.AppendLine($"            {{ name = \"{container.ContainerScriptName}\", file = \"{quest.Name}/Entities/{container.ContainerScriptName}\", id = {entityId} }},");
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
        // FIX: Split by any newline type to handle both \n and \r\n properly
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
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
                // Comment/uncomment lines within the quest block FIRST (before counting braces)
                bool isCommented = trimmed.StartsWith("--");
                if (enable && isCommented)
                {
                    lines[i] = Regex.Replace(line, @"^(\s*)--\s*", "$1");
                    trimmed = lines[i].Trim(); // Update trimmed after uncommenting
                }
                else if (!enable && !isCommented && !string.IsNullOrWhiteSpace(trimmed))
                {
                    lines[i] = Regex.Replace(line, @"^(\s*)", "$1-- ");
                }

                // Count braces to track quest block (use the original trimmed for counting)
                string lineForCounting = trimmed.StartsWith("--") ? trimmed.Substring(2).Trim() : trimmed;
                foreach (char c in lineForCounting)
                {
                    if (c == '{') braceDepth++;
                    else if (c == '}') braceDepth--;
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
            // Preserve original line ending style
            string lineEnding = content.Contains("\r\n") ? "\r\n" : "\n";
            File.WriteAllText(filePath, string.Join(lineEnding, lines));
        }

        return found;
    }

   private bool ToggleMasterActivation(string masterPath, string questName, bool enable)
    {
        string content = File.ReadAllText(masterPath);
        
        // FIX: Use case-insensitive pattern to match both 'Quest' and 'quest'
        // The actual FSE_Master.lua uses lowercase 'quest'
        string pattern = $@"^(\s*)(--\s*)?(quest:ActivateQuest\(""{Regex.Escape(questName)}""\))";

        bool found = false;
        // FIX: Split by any newline type to handle both \n and \r\n properly
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            // FIX: Add RegexOptions.IgnoreCase for extra safety
            var match = Regex.Match(lines[i], pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                found = true;
                bool isCommented = match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value);

                if (enable && isCommented)
                {
                    // Uncomment - remove the "-- " prefix
                    lines[i] = Regex.Replace(lines[i], @"^(\s*)--\s*", "$1");
                }
                else if (!enable && !isCommented)
                {
                    // Comment - add "-- " prefix
                    lines[i] = Regex.Replace(lines[i], @"^(\s*)", "$1-- ");
                }
                
                // Also handle the associated Log line on the next line
                if (i + 1 < lines.Length)
                {
                    string logPattern = $@"^(\s*)(--\s*)?(quest:Log\(""FSE_Master: Activated {Regex.Escape(questName)}""\))";
                    var logMatch = Regex.Match(lines[i + 1], logPattern, RegexOptions.IgnoreCase);
                    if (logMatch.Success)
                    {
                        bool logIsCommented = logMatch.Groups[2].Success && !string.IsNullOrWhiteSpace(logMatch.Groups[2].Value);
                        
                        if (enable && logIsCommented)
                        {
                            lines[i + 1] = Regex.Replace(lines[i + 1], @"^(\s*)--\s*", "$1");
                        }
                        else if (!enable && !logIsCommented)
                        {
                            lines[i + 1] = Regex.Replace(lines[i + 1], @"^(\s*)", "$1-- ");
                        }
                    }
                }
                
                break;
            }
        }

        if (found)
        {
            // Preserve original line ending style
            string lineEnding = content.Contains("\r\n") ? "\r\n" : "\n";
            File.WriteAllText(masterPath, string.Join(lineEnding, lines));
        }

        return found;
    }

    private bool EnsureFseInstalled(out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(config.FablePath))
        {
            error = "Fable path not configured.";
            return false;
        }

        // Check if FSE is already installed
        string launcherPath = Path.Combine(config.FablePath, "FSE_Launcher.exe");
        string dllPath = Path.Combine(config.FablePath, "FableScriptExtender.dll");

        if (File.Exists(launcherPath) && File.Exists(dllPath))
        {
            // FSE already installed
            return true;
        }

        // Install FSE from packaged binaries (included with the application)
        string appDirectory = AppContext.BaseDirectory;
        string fseBinariesFolder = Path.Combine(appDirectory, "FSE_Binaries");

        string sourceLauncher = Path.Combine(fseBinariesFolder, "FSE_Launcher.exe");
        string sourceDll = Path.Combine(fseBinariesFolder, "FableScriptExtender.dll");

        if (!File.Exists(sourceLauncher) || !File.Exists(sourceDll))
        {
            error = $"FSE binaries not found in application folder.\n\nExpected location: {fseBinariesFolder}\n\nPlease reinstall the application or contact support.";
            return false;
        }

        try
        {
            // Copy FSE files to Fable directory
            File.Copy(sourceLauncher, launcherPath, overwrite: true);
            File.Copy(sourceDll, dllPath, overwrite: true);

            // Create Mods.ini to enable FSE
            string modsIniPath = Path.Combine(config.FablePath, "Mods.ini");
            if (!File.Exists(modsIniPath))
            {
                File.WriteAllText(modsIniPath, "[Mods]\nFableScriptExtender.dll=1\n");
            }

            // Create FSE folder if it doesn't exist
            string fseFolder = Path.Combine(config.FablePath, "FSE");
            Directory.CreateDirectory(fseFolder);

            // Create Master folder for FSE_Master.lua
            string masterFolder = Path.Combine(fseFolder, "Master");
            Directory.CreateDirectory(masterFolder);

            // Create FSE_Master.lua if it doesn't exist
            string masterLuaPath = Path.Combine(masterFolder, "FSE_Master.lua");
            if (!File.Exists(masterLuaPath))
            {
                string masterLuaContent = @"-- FSE_Master.lua - Master quest for FSE
Quest = nil

function Init(quest)
    Quest = quest
    Quest:Log(""FSE_Master Init()"")
end

function Main(quest)
    Quest = quest
    Quest:Log(""FSE_Master Main() started"")

    -- Custom quests will be auto-activated here by the deployment tool

    Quest:Log(""FSE_Master Main() completed"")
end

function OnPersist(quest, context)
    Quest = quest
end
";
                File.WriteAllText(masterLuaPath, masterLuaContent);
            }

            // Ensure FSE_Master is registered in quests.lua
            string questsLuaPath = Path.Combine(fseFolder, "quests.lua");
            if (!File.Exists(questsLuaPath))
            {
                string questsLuaContent = @"Quests = {
    FSE_Master = {
        name = ""FSE_Master"",
        file = ""Master/FSE_Master"",
        id = 1,
        master = true,
        entity_scripts = {}
    },
}
";
                File.WriteAllText(questsLuaPath, questsLuaContent);
            }
            else
            {
                // Check if FSE_Master is already registered
                string questsContent = File.ReadAllText(questsLuaPath);
                if (!questsContent.Contains("FSE_Master"))
                {
                    // Add FSE_Master to existing quests.lua
                    int insertPos = questsContent.IndexOf("Quests = {") + "Quests = {".Length;
                    string fseMasterEntry = @"
    FSE_Master = {
        name = ""FSE_Master"",
        file = ""Master/FSE_Master"",
        id = 1,
        master = true,
        entity_scripts = {}
    },";
                    questsContent = questsContent.Insert(insertPos, fseMasterEntry);
                    File.WriteAllText(questsLuaPath, questsContent);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            error = $"Failed to install FSE: {ex.Message}";
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

        // Ensure FSE is installed before launching
        if (!EnsureFseInstalled(out string? fseError))
        {
            message = $"Cannot launch FSE: {fseError}";
            return false;
        }

        string? launcherPath = config.GetFseLauncherPath();
        if (string.IsNullOrWhiteSpace(launcherPath))
        {
            message = "FSE_Launcher.exe not found in Fable installation folder.";
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
