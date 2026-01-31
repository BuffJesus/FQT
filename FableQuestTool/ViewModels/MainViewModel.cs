using System;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ProjectFileService fileService = new();
    private readonly CodeGenerator codeGenerator = new();
    private readonly ExportService exportService;
    private readonly DeploymentService deploymentService;
    private readonly FableConfig fableConfig;

    [ObservableProperty]
    private QuestProject project = new();

    [ObservableProperty]
    private bool isModified;

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private string? projectPath;

    [ObservableProperty]
    private bool isAdvancedMode;

    public EntityEditorViewModel? EntityEditorViewModel { get; set; }

    public string Title => "FSE Quest Creator Pro";

    public MainViewModel()
    {
        fableConfig = FableConfig.Load();
        exportService = new ExportService(codeGenerator);
        deploymentService = new DeploymentService(fableConfig, codeGenerator);
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
            Filter = "FSE Quest Project (*.fsequest)|*.fsequest|JSON Files (*.json)|*.json|All Files (*.*)|*.*"
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
            EntityEditorViewModel?.ReloadEntities();
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
            IsModified = false;
            StatusText = $"Project saved. ({Project.Entities.Count} entities, {totalNodes} nodes, {totalConnections} connections)";
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
            Filter = "FSE Quest Project (*.fsequest)|*.fsequest|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
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
            // Save node graph data from all entity tabs back to entity models
            EntityEditorViewModel?.SaveAllTabs();

            if (deploymentService.DeployQuest(Project, out string message))
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
}
