using FableQuestTool.Config;
using FableQuestTool.Core;
using FableQuestTool.Formats;
using FableQuestTool.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FableQuestTool.Services;

/// <summary>
/// Handles deployment of quest projects to the Fable game installation.
///
/// DeploymentService manages the complete deployment pipeline that takes a QuestProject
/// and installs it into the Fable: The Lost Chapters game directory so it can be played.
///
/// The deployment process includes:
/// 1. Ensuring FSE (Fable Script Extender) is installed in the game directory
/// 2. Generating Lua scripts from the quest project via CodeGenerator
/// 3. Creating the quest folder structure in FSE directory
/// 4. Registering the quest in quests.lua (FSE's quest configuration)
/// 5. Registering the quest in FinalAlbion.qst (Fable's quest registry)
/// 6. Adding quest activation to FSE_Master.lua (auto-starts the quest)
/// </summary>
/// <remarks>
/// File structure created during deployment:
/// <code>
/// Fable/
/// ├── FSE_Launcher.exe        (FSE launcher, installed if missing)
/// ├── FableScriptExtender.dll (FSE DLL, installed if missing)
/// └── FSE/
///     ├── quests.lua          (Quest registration, updated)
///     ├── Master/
///     │   └── FSE_Master.lua  (Master quest, updated with activation)
///     └── [QuestName]/
///         ├── [QuestName].lua (Main quest script, generated)
///         └── Entities/
///             └── *.lua       (Entity scripts, generated)
/// </code>
///
/// The service also supports:
/// - Deleting deployed quests (removes all traces)
/// - Toggling quests enabled/disabled (comments/uncomments in config files)
/// - Launching the game via FSE_Launcher.exe
/// </remarks>
public sealed class DeploymentService
{
    private readonly FableConfig config;
    private readonly CodeGenerator codeGenerator;

    /// <summary>
    /// Creates a new DeploymentService instance.
    /// </summary>
    /// <param name="config">Fable configuration containing installation path</param>
    /// <param name="codeGenerator">Code generator for producing Lua scripts</param>
    public DeploymentService(FableConfig config, CodeGenerator codeGenerator)
    {
        this.config = config;
        this.codeGenerator = codeGenerator;
    }

    /// <summary>
    /// Deploys a quest project to the Fable game installation.
    ///
    /// This method performs the complete deployment pipeline:
    /// 1. Validates Fable installation path
    /// 2. Ensures FSE is installed (copies binaries if needed)
    /// 3. Creates quest folder structure
    /// 4. Generates and writes quest script
    /// 5. Generates and writes entity scripts
    /// 6. Registers quest in quests.lua
    /// 7. Registers quest in FinalAlbion.qst
    /// 8. Adds quest activation to FSE_Master.lua
    /// </summary>
    /// <param name="quest">The quest project to deploy</param>
    /// <param name="message">Output message describing result or error</param>
    /// <returns>True if deployment succeeded, false otherwise</returns>
    public bool DeployQuest(QuestProject quest, out string message)
    {
        message = string.Empty;

        var nameErrors = NameValidation.ValidateProject(quest);
        if (nameErrors.Count > 0)
        {
            message = "Quest contains invalid names:\n" + string.Join("\n", nameErrors);
            return false;
        }

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
                FileWrite.WriteAllTextAtomic(questsLuaPath, "Quests = {}\n", createBackup: false);
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

            FileWrite.WriteAllTextAtomic(questsLuaPath, content);
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
            string activationPattern = $@"quest:ActivateQuest\(""{Regex.Escape(questName)}""\)";
            if (Regex.IsMatch(content, activationPattern, RegexOptions.IgnoreCase))
            {
                // Already activated
                return true;
            }

            // Find the Main function and add activation before its matching "end"
            int mainFuncStart = FindLuaFunctionStart(content, "Main");
            if (mainFuncStart == -1)
            {
                error = "Could not find Main function in FSE_Master.lua";
                return false;
            }

            int mainFuncEnd = FindLuaFunctionEnd(content, mainFuncStart);
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
            FileWrite.WriteAllTextAtomic(masterPath, content);

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

    /// <summary>
    /// Deletes a deployed quest from the Fable installation.
    ///
    /// Removes all traces of the quest:
    /// - Quest folder and all scripts from FSE directory
    /// - Quest entry from quests.lua
    /// - Quest entry from FinalAlbion.qst
    /// - Quest activation from FSE_Master.lua
    /// </summary>
    /// <param name="questName">Name of the quest to delete</param>
    /// <param name="message">Output message describing what was deleted</param>
    /// <returns>True if any quest files were found and deleted</returns>
    public bool DeleteQuest(string questName, out string message)
    {
        message = string.Empty;

        var nameErrors = NameValidation.ValidateProject(new QuestProject { Name = questName });
        if (nameErrors.Count > 0)
        {
            message = "Quest name is invalid for file operations.";
            return false;
        }

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
                                    FileWrite.WriteAllTextAtomic(questsLuaPath, content);
                                    questsLuaUpdated = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Remove from FinalAlbion.qst
            string? fablePath = config.FablePath;
            if (!string.IsNullOrWhiteSpace(fablePath))
            {
                string qstPath = Path.Combine(fablePath, "data", "Levels", "FinalAlbion.qst");
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
                        FileWrite.WriteAllTextAtomic(masterPath, newContent);
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

    /// <summary>
    /// Enables or disables a deployed quest without removing it.
    ///
    /// Toggling works by commenting/uncommenting quest entries in:
    /// - quests.lua (Lua comments using --)
    /// - FinalAlbion.qst (adds/removes quest entry)
    /// - FSE_Master.lua (comments/uncomments activation line)
    ///
    /// This allows temporarily disabling quests for testing without losing
    /// the deployment, or enabling previously disabled quests.
    /// </summary>
    /// <param name="questName">Name of the quest to toggle</param>
    /// <param name="enable">True to enable, false to disable</param>
    /// <param name="message">Output message describing what was toggled</param>
    /// <returns>True if toggle succeeded</returns>
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
            FileWrite.WriteAllTextAtomic(filePath, string.Join(lineEnding, lines));
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
            FileWrite.WriteAllTextAtomic(masterPath, string.Join(lineEnding, lines));
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

        // FSE not found - direct user to download from GitHub
        error = @"Fable Script Extender (FSE) is required but not installed.

Please download FSE from the official repository:
https://github.com/eeeeeAeoN/FableScriptExtender

Installation instructions:
1. Download the latest release from the GitHub repository
2. Extract the contents to your Fable installation directory:
   " + config.FablePath + @"
3. You should have the following files:
   - FSE_Launcher.exe (in your Fable root directory)
   - FableScriptExtender.dll (in your Fable root directory)
   - Mods.ini (in your Fable root directory)
   - FSE folder with quests and Master folder

After installing FSE, try deploying your quest again.";
        return false;
    }

    /// <summary>
    /// Launches the game using FSE_Launcher.exe.
    ///
    /// Ensures FSE is installed first, then starts the launcher which
    /// will inject FableScriptExtender.dll into the game process.
    /// This enables all deployed quests to run.
    /// </summary>
    /// <param name="message">Output message describing result or error</param>
    /// <returns>True if launch succeeded</returns>
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

    private static int FindLuaFunctionStart(string content, string functionName)
    {
        var match = Regex.Match(
            content,
            $@"function\s+{Regex.Escape(functionName)}\s*\(",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Index : -1;
    }

    private static int FindLuaFunctionEnd(string content, int functionStart)
    {
        bool inString = false;
        char stringChar = '\0';
        bool inLineComment = false;
        int depth = 0;

        for (int i = functionStart; i < content.Length; i++)
        {
            char c = content[i];

            if (inLineComment)
            {
                if (c == '\n')
                {
                    inLineComment = false;
                }
                continue;
            }

            if (inString)
            {
                if (c == '\\' && i + 1 < content.Length)
                {
                    i++;
                    continue;
                }

                if (c == stringChar)
                {
                    inString = false;
                }
                continue;
            }

            if (c == '-' && i + 1 < content.Length && content[i + 1] == '-')
            {
                inLineComment = true;
                i++;
                continue;
            }

            if (c == '"' || c == '\'')
            {
                inString = true;
                stringChar = c;
                continue;
            }

            if (IsWordAt(content, i, "function"))
            {
                depth++;
                i += "function".Length - 1;
                continue;
            }

            if (IsWordAt(content, i, "end"))
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
                i += "end".Length - 1;
            }
        }

        return -1;
    }

    private static bool IsWordAt(string content, int index, string word)
    {
        if (index < 0 || index + word.Length > content.Length)
        {
            return false;
        }

        if (!content.AsSpan(index, word.Length).Equals(word.AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        bool beforeOk = index == 0 || !char.IsLetterOrDigit(content[index - 1]);
        int end = index + word.Length;
        bool afterOk = end >= content.Length || !char.IsLetterOrDigit(content[end]);

        return beforeOk && afterOk;
    }
}
