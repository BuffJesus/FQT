using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class QuestManagerViewModel : ObservableObject
{
    private readonly FableConfig config;
    private readonly DeploymentService deploymentService;

    [ObservableProperty]
    private ObservableCollection<DeployedQuestInfo> quests = new();

    [ObservableProperty]
    private DeployedQuestInfo? selectedQuest;

    [ObservableProperty]
    private string statusText = "Ready";

    public QuestManagerViewModel()
    {
        config = FableConfig.Load();
        var codeGenerator = new CodeGenerator();
        deploymentService = new DeploymentService(config, codeGenerator);
        LoadQuests();
    }

    [RelayCommand]
    private void LoadQuests()
    {
        Quests.Clear();
        StatusText = "Loading...";

        if (!config.EnsureFablePath())
        {
            StatusText = "Fable path not configured";
            return;
        }

        string? fseFolder = config.GetFseFolder();
        if (string.IsNullOrWhiteSpace(fseFolder))
        {
            StatusText = "FSE folder not found";
            return;
        }

        string questsLuaPath = Path.Combine(fseFolder, "quests.lua");
        if (!File.Exists(questsLuaPath))
        {
            StatusText = "quests.lua not found";
            return;
        }

        try
        {
            string content = File.ReadAllText(questsLuaPath);
            var questInfos = ParseQuestsLua(content, fseFolder);

            foreach (var info in questInfos)
            {
                Quests.Add(info);
            }

            StatusText = $"Loaded {Quests.Count} quests";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleQuest()
    {
        if (SelectedQuest == null)
        {
            StatusText = "No quest selected";
            return;
        }

        bool newState = !SelectedQuest.IsEnabled;
        string action = newState ? "enable" : "disable";

        try
        {
            if (deploymentService.ToggleQuest(SelectedQuest.Name, newState, out string message))
            {
                StatusText = $"{(newState ? "Enabled" : "Disabled")} {SelectedQuest.Name}";
                System.Windows.MessageBox.Show(message, $"Quest {(newState ? "Enabled" : "Disabled")}",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                LoadQuests();
            }
            else
            {
                StatusText = $"Failed to {action} quest";
                System.Windows.MessageBox.Show(message, "Toggle Failed",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            System.Windows.MessageBox.Show($"Failed to {action} quest: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteSelectedQuest()
    {
        if (SelectedQuest == null)
        {
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete the quest '{SelectedQuest.Name}'?\n\n" +
            "This will:\n" +
            "• Delete quest files from Fable installation\n" +
            "• Remove quest registration from quests.lua\n" +
            "• Remove quest registration from FinalAlbion.qst\n\n" +
            "This action cannot be undone!",
            "Delete Quest",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            if (deploymentService.DeleteQuest(SelectedQuest.Name, out string message))
            {
                StatusText = $"Deleted {SelectedQuest.Name}";
                System.Windows.MessageBox.Show(message, "Deletion Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                LoadQuests();
            }
            else
            {
                StatusText = "Deletion failed";
                System.Windows.MessageBox.Show(message, "Deletion Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            System.Windows.MessageBox.Show($"Failed to delete quest: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private static ObservableCollection<DeployedQuestInfo> ParseQuestsLua(string content, string fseFolder)
    {
        var quests = new ObservableCollection<DeployedQuestInfo>();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        DeployedQuestInfo? currentQuest = null;
        bool isCurrentQuestCommented = false;
        int braceDepth = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            // Check for quest definition start
            if (trimmed.Contains("=") && trimmed.Contains("{") && currentQuest == null)
            {
                bool isCommented = trimmed.StartsWith("--");
                string workingLine = isCommented ? trimmed.TrimStart('-').Trim() : trimmed;

                int equalsIndex = workingLine.IndexOf("=");
                if (equalsIndex > 0)
                {
                    string questName = workingLine.Substring(0, equalsIndex).Trim();

                    // Ignore root table only
                    if (!string.Equals(questName, "Quests", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(questName))
                    {
                        currentQuest = new DeployedQuestInfo
                        {
                            Name = questName,
                            IsEnabled = !isCommented
                        };
                        isCurrentQuestCommented = isCommented;
                        braceDepth = 1;
                    }
                }
            }
            else if (currentQuest != null)
            {
                // Track brace depth
                if (trimmed.Contains("{"))
                {
                    braceDepth++;
                }

                // Parse properties
                string propertyLine = trimmed;
                if (isCurrentQuestCommented && propertyLine.StartsWith("--"))
                {
                    propertyLine = propertyLine.TrimStart('-').Trim();
                }

                if (propertyLine.Contains("id =") || propertyLine.Contains("QuestID ="))
                {
                    var match = Regex.Match(propertyLine, @"=\s*(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                    {
                        currentQuest.Id = id;
                    }
                }
                else if (propertyLine.Contains("file =") || propertyLine.Contains("Folder ="))
                {
                    var match = Regex.Match(propertyLine, @"=\s*""([^""]*)""");
                    if (match.Success)
                    {
                        currentQuest.FilePath = match.Groups[1].Value;
                    }
                }

                // Check for closing braces
                if (trimmed.Contains("}"))
                {
                    braceDepth--;

                    if (braceDepth == 0)
                    {
                        // Check if quest folder exists
                        string questFolder = Path.Combine(fseFolder, currentQuest.Name);
                        currentQuest.FolderExists = Directory.Exists(questFolder);

                        if (currentQuest.Id > 0 && !string.IsNullOrWhiteSpace(currentQuest.Name))
                        {
                            quests.Add(currentQuest);
                        }
                        currentQuest = null;
                        isCurrentQuestCommented = false;
                    }
                }
            }
        }

        return quests;
    }
}

public sealed class DeployedQuestInfo : ObservableObject
{
    private string name = string.Empty;
    private int id;
    private string filePath = string.Empty;
    private bool isEnabled = true;
    private bool folderExists = true;

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public int Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    public string FilePath
    {
        get => filePath;
        set => SetProperty(ref filePath, value);
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }

    public bool FolderExists
    {
        get => folderExists;
        set => SetProperty(ref folderExists, value);
    }

    public string Status => IsEnabled ? "Enabled" : "Disabled";
}
