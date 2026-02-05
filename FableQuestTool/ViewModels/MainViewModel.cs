using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

/// <summary>
/// Main application ViewModel that coordinates all primary operations.
///
/// MainViewModel is the central hub of the FableQuestTool application, managing:
/// - Project lifecycle (new, open, save, save-as)
/// - Deployment to Fable installation
/// - FSE launcher integration
/// - Quest management (delete, toggle enable/disable)
/// - Entity browser and catalog export
/// - Fable path configuration
///
/// Uses CommunityToolkit.Mvvm for observable properties and relay commands.
/// Commands are automatically generated for methods decorated with [RelayCommand].
/// </summary>
/// <remarks>
/// The ViewModel follows MVVM pattern:
/// - Properties are bound to UI elements in MainWindow.xaml
/// - Commands are invoked by menu items and buttons
/// - Services (fileService, codeGenerator, deploymentService) handle business logic
///
/// Project state is tracked via:
/// - Project: The current QuestProject being edited
/// - ProjectPath: File path if saved, null for new projects
/// - IsModified: Whether unsaved changes exist
/// - StatusText: Current status message shown in UI
/// </remarks>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly ProjectFileService fileService = new();
    private readonly CodeGenerator codeGenerator = new();
    private readonly ExportService exportService;
    private readonly DeploymentService deploymentService;
    private readonly FableConfig fableConfig;
    private readonly ProjectValidator projectValidator = new();
    private QuestProject? trackedProject;
    private DispatcherTimer? statusToastTimer;

    [ObservableProperty]
    private QuestProject project = new();

    [ObservableProperty]
    private bool isModified;

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private bool isStatusToastVisible;

    [ObservableProperty]
    private string? projectPath;

    [ObservableProperty]
    private bool isAdvancedMode;

    [ObservableProperty]
    private bool isFablePathConfigured;

    [ObservableProperty]
    private bool isFseInstalled;

    [ObservableProperty]
    private string fablePathDisplay = "(not configured)";

    [ObservableProperty]
    private bool isSampleQuestsAvailable;

    [ObservableProperty]
    private bool isStartupImageEnabled;

    [ObservableProperty]
    private string sampleQuestsPathDisplay = "(not found)";

    public EntityEditorViewModel? EntityEditorViewModel { get; set; }

    public string Title => "Fable Quest Tool (FQT)";

    public MainViewModel()
    {
        fableConfig = FableConfig.Load();
        exportService = new ExportService(codeGenerator);
        deploymentService = new DeploymentService(fableConfig, codeGenerator);
        UpdateSetupStatus();
        IsStartupImageEnabled = fableConfig.GetShowStartupImage();
    }

    public IReadOnlyList<string> GetFavoriteNodeTypes()
    {
        return fableConfig.GetFavoriteNodeTypes();
    }

    public void SaveFavoriteNodeTypes(IEnumerable<string> types)
    {
        fableConfig.SetFavoriteNodeTypes(types);
        fableConfig.Save();
    }

    partial void OnIsStartupImageEnabledChanged(bool value)
    {
        fableConfig.SetShowStartupImage(value);
        fableConfig.Save();
    }

    [RelayCommand]
    private void NewProject()
    {
        if (!ConfirmDiscardChanges())
        {
            return;
        }

        Project = new QuestProject();
        ProjectPath = null;
        IsModified = false;
        StatusText = "New project created.";
    }

    [RelayCommand]
    private void OpenProject()
    {
        if (!ConfirmDiscardChanges())
        {
            return;
        }

        Microsoft.Win32.OpenFileDialog dialog = new()
        {
            Filter = "FQT Project (*.fqtproj;*.fsequest)|*.fqtproj;*.fsequest|JSON Files (*.json)|*.json|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            Project = fileService.Load(dialog.FileName);
            ProjectPath = dialog.FileName;
            IsModified = false;
            StatusText = "Project loaded.";

            // Reload entities in the entity editor
            EntityEditorViewModel?.RefreshFromProject();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load project: {ex.Message}", "Load Project", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SaveProject()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
        {
            SaveProjectAs();
            return;
        }

        try
        {
            // Save node graph data from all entity tabs back to entity models
            EntityEditorViewModel?.SaveAllTabs();

            // Debug: Check if entities have data before saving
            int totalNodes = Project.Entities.Sum(e => e.Nodes.Count);
            int totalConnections = Project.Entities.Sum(e => e.Connections.Count);

            fileService.Save(ProjectPath, Project);
            if (!File.Exists(ProjectPath))
            {
                StatusText = "Save failed: file not found after write.";
                System.Windows.MessageBox.Show(
                    $"Save completed without errors, but the file was not found at:\n{ProjectPath}",
                    "Save Project",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }
            IsModified = false;
            StatusText = $"Project saved to: {ProjectPath} ({Project.Entities.Count} entities, {totalNodes} nodes, {totalConnections} connections)";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to save project: {ex.Message}", "Save Project", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SaveProjectAs()
    {
        Microsoft.Win32.SaveFileDialog dialog = new()
        {
            Filter = "FQT Project (*.fqtproj;*.fsequest)|*.fqtproj;*.fsequest|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            FileName = Project.Name
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        ProjectPath = dialog.FileName;
        SaveProject();
    }

    [RelayCommand]
    private void DeleteQuest()
    {
        if (string.IsNullOrWhiteSpace(Project.Name))
        {
            System.Windows.MessageBox.Show("No quest loaded to delete.", "Delete Quest", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!ValidateQuestNameForFileOperations(Project.Name))
        {
            System.Windows.MessageBox.Show(
                "Quest name is invalid for file operations. Please rename the quest before deleting.",
                "Delete Quest",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete the quest '{Project.Name}'?\n\n" +
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
            if (deploymentService.DeleteQuest(Project.Name, out string message))
            {
                StatusText = "Quest deleted successfully";
                System.Windows.MessageBox.Show(message, "Deletion Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Delete project file if it exists
                if (!string.IsNullOrWhiteSpace(ProjectPath) && File.Exists(ProjectPath))
                {
                    File.Delete(ProjectPath);
                }

                // Create new project
                Project = new QuestProject();
                ProjectPath = null;
                IsModified = false;
            }
            else
            {
                StatusText = "Deletion failed";
                System.Windows.MessageBox.Show(message, "Deletion Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText = "Deletion error";
            System.Windows.MessageBox.Show($"Failed to delete quest: {ex.Message}", "Deletion Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportProject()
    {
        try
        {
            UpdateSetupStatus();
            // Save node graph data from all entity tabs back to entity models
            EntityEditorViewModel?.SaveAllTabs();

            if (!ValidateProjectForDeployment())
            {
                return;
            }

            QuestProject questToDeploy = Project;
            if (!string.IsNullOrWhiteSpace(ProjectPath))
            {
                fileService.Save(ProjectPath, Project);
                questToDeploy = fileService.Load(ProjectPath);
            }

            if (deploymentService.DeployQuest(questToDeploy, out string message))
            {
                StatusText = "Quest deployed successfully";
                System.Windows.MessageBox.Show(message, "Deployment Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            else
            {
                StatusText = "Deployment failed";
                System.Windows.MessageBox.Show(message, "Deployment Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText = "Deployment error";
            System.Windows.MessageBox.Show($"Failed to deploy quest: {ex.Message}", "Deployment Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void LaunchFse()
    {
        try
        {
            UpdateSetupStatus();
            if (deploymentService.LaunchFse(out string message))
            {
                StatusText = "FSE launched";
            }
            else
            {
                System.Windows.MessageBox.Show(message, "Launch Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to launch FSE: {ex.Message}", "Launch Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ConfigureFablePath()
    {
        if (fableConfig.PromptForFablePath())
        {
            StatusText = $"Fable path configured: {fableConfig.FablePath}";
            System.Windows.MessageBox.Show($"Fable path set to:\n{fableConfig.FablePath}", "Configuration", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            UpdateSetupStatus();
        }
    }

    [RelayCommand]
    private void ManageQuests()
    {
        var dialog = new Views.QuestManagerView
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        dialog.ShowDialog();
        StatusText = "Quest Manager closed";
    }

    [RelayCommand]
    private void BrowseEntities()
    {
        var dialog = new Views.EntityBrowserView
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true && dialog.SelectedEntity != null)
        {
            var entity = dialog.SelectedEntity;
            StatusText = $"Selected entity: {entity.ScriptName} ({entity.DefinitionType})";
            System.Windows.MessageBox.Show(
                $"Entity Selected:\n\n" +
                $"Script Name: {entity.ScriptName}\n" +
                $"Definition: {entity.DefinitionType}\n" +
                $"Category: {entity.Category}\n" +
                $"Region: {entity.RegionName}\n" +
                $"Position: ({entity.PositionX:F2}, {entity.PositionY:F2}, {entity.PositionZ:F2})",
                "Entity Selected",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ExportCatalog()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            FileName = "FableEntityCatalog.txt",
            Title = "Export Entity Catalog"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                StatusText = "Scanning game data...";
                var entityBrowser = new Services.EntityBrowserService(fableConfig);
                entityBrowser.ExportCatalog(dialog.FileName);

                var stats = entityBrowser.GetStatistics();
                StatusText = $"Catalog exported: {stats.TotalEntities} entities";

                System.Windows.MessageBox.Show(
                    $"Entity catalog exported successfully!\n\n" +
                    $"File: {dialog.FileName}\n\n" +
                    $"Statistics:\n" +
                    $"Total Entities: {stats.TotalEntities}\n" +
                    $"Scriptable: {stats.EntitiesWithScriptName}\n" +
                    $"Markers: {stats.MarkerCount}\n" +
                    $"NPCs: {stats.NpcCount}\n" +
                    $"Creatures: {stats.CreatureCount}\n" +
                    $"Objects: {stats.ObjectCount}",
                    "Export Complete",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText = "Export failed";
                System.Windows.MessageBox.Show(
                    $"Failed to export catalog: {ex.Message}",
                    "Export Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void Exit()
    {
        if (!ConfirmDiscardChanges())
        {
            return;
        }

        System.Windows.Application.Current.Shutdown();
    }

    private bool ConfirmDiscardChanges()
    {
        if (!IsModified)
        {
            return true;
        }

        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
            "You have unsaved changes. Continue?",
            "Unsaved Changes",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        return result == System.Windows.MessageBoxResult.Yes;
    }

    partial void OnProjectChanged(QuestProject value)
    {
        if (trackedProject != null)
        {
            trackedProject.PropertyChanged -= Project_PropertyChanged;
        }

        trackedProject = value;
        if (trackedProject != null)
        {
            trackedProject.PropertyChanged += Project_PropertyChanged;
        }
    }

    private void Project_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        IsModified = true;
    }

    partial void OnStatusTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        EnsureStatusToastTimer();
        IsStatusToastVisible = true;
        statusToastTimer!.Stop();
        statusToastTimer.Start();
    }

    private void EnsureStatusToastTimer()
    {
        if (statusToastTimer != null)
        {
            return;
        }

        statusToastTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };

        statusToastTimer.Tick += (_, _) =>
        {
            IsStatusToastVisible = false;
            statusToastTimer!.Stop();
        };
    }

    private bool ValidateProjectForDeployment()
    {
        var issues = projectValidator.Validate(Project);
        var errors = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
        var warnings = issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();

        if (errors.Count == 0)
        {
            if (warnings.Count > 0)
            {
                string warningText = "Warnings:\n\n" + string.Join("\n", warnings.Select(w => w.Message));
                System.Windows.MessageBox.Show(warningText, "Validation Warnings", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            return true;
        }

        string message = "Please fix the following before deploying:\n\n" + string.Join("\n", errors.Select(e => e.Message));
        System.Windows.MessageBox.Show(message, "Invalid Quest Names", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        return false;
    }

    private static bool ValidateQuestNameForFileOperations(string questName)
    {
        var temp = new QuestProject { Name = questName };
        var errors = NameValidation.ValidateProject(temp);
        return errors.Count == 0;
    }

    [RelayCommand]
    private void ValidateProject()
    {
        EntityEditorViewModel?.SaveAllTabs();

        var issues = projectValidator.Validate(Project);
        if (issues.Count == 0)
        {
            System.Windows.MessageBox.Show("No issues found.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        var errorLines = issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => "ERROR: " + i.Message).ToList();
        var warningLines = issues.Where(i => i.Severity == ValidationSeverity.Warning).Select(i => "WARN: " + i.Message).ToList();
        var message = string.Join("\n", errorLines.Concat(warningLines));

        System.Windows.MessageBox.Show(message, "Validation Results", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
    }

    [RelayCommand]
    private void PreviewLua()
    {
        EntityEditorViewModel?.SaveAllTabs();

        var items = new System.Collections.ObjectModel.ObservableCollection<LuaPreviewItem>();
        items.Add(new LuaPreviewItem("Quest", codeGenerator.GenerateQuestScript(Project)));

        foreach (var entity in Project.Entities)
        {
            items.Add(new LuaPreviewItem($"Entity: {entity.ScriptName}", codeGenerator.GenerateEntityScript(Project, entity)));
        }

        if (codeGenerator.NeedsContainerEntityScript(Project))
        {
            var container = Project.Rewards.Container!;
            items.Add(new LuaPreviewItem($"Entity: {container.ContainerScriptName}", codeGenerator.GenerateContainerEntityScript(Project, container)));
        }

        var view = new Views.LuaPreviewView
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = new LuaPreviewViewModel(items)
        };

        view.ShowDialog();
    }

    [RelayCommand]
    private void OpenSampleQuests()
    {
        if (!IsSampleQuestsAvailable)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SampleQuestsPathDisplay,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to open sample quests folder: {ex.Message}",
                "Open Samples",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void UpdateSetupStatus()
    {
        string? fablePath = fableConfig.FablePath;
        bool hasData = !string.IsNullOrWhiteSpace(fablePath) &&
                       Directory.Exists(fablePath) &&
                       Directory.Exists(Path.Combine(fablePath, "data"));

        IsFablePathConfigured = hasData;
        FablePathDisplay = hasData ? fablePath! : "(not configured)";

        if (!hasData)
        {
            IsFseInstalled = false;
            return;
        }

        string launcher = Path.Combine(fablePath!, "FSE_Launcher.exe");
        string dll = Path.Combine(fablePath!, "FableScriptExtender.dll");
        IsFseInstalled = File.Exists(launcher) && File.Exists(dll);

        string samples = Path.Combine(AppContext.BaseDirectory ?? string.Empty, "FSE_Source", "SampleQuests");
        IsSampleQuestsAvailable = Directory.Exists(samples);
        SampleQuestsPathDisplay = IsSampleQuestsAvailable ? samples : "(not found)";
    }
}
