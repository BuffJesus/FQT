using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ProjectFileService fileService = new();
    private readonly ExportService exportService = new(new CodeGenerator());

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

    public string Title => "FSE Quest Creator Pro";

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
            fileService.Save(ProjectPath, Project);
            IsModified = false;
            StatusText = "Project saved.";
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
    private void ExportProject()
    {
        using System.Windows.Forms.FolderBrowserDialog dialog = new();
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }

        try
        {
            string exportPath = exportService.Export(Project, dialog.SelectedPath);
            StatusText = $"Exported to {exportPath}";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to export quest: {ex.Message}", "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
