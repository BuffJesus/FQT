using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class TemplatesViewModel : ObservableObject
{
    private readonly TemplateService templateService = new();

    [ObservableProperty]
    private QuestTemplate? selectedTemplate;

    [ObservableProperty]
    private string selectedCategory = "All";

    public ObservableCollection<string> Categories { get; } = new()
    {
        "All",
        "Dialogue",
        "Cinematic",
        "Combat",
        "Collection",
        "Escort",
        "Travel",
        "Mystery"
    };

    public ObservableCollection<QuestTemplate> FilteredTemplates { get; } = new();
    private List<QuestTemplate> allTemplates = new();

    /// <summary>
    /// Event raised when a template is selected for use.
    /// The MainViewModel subscribes to this to load the template.
    /// </summary>
    public event Action<QuestProject>? TemplateSelected;

    public TemplatesViewModel()
    {
        allTemplates = templateService.GetAllTemplates();
        UpdateFilteredTemplates();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        UpdateFilteredTemplates();
    }

    private void UpdateFilteredTemplates()
    {
        FilteredTemplates.Clear();

        var filtered = allTemplates.AsEnumerable();

        if (SelectedCategory != "All")
        {
            filtered = filtered.Where(t => t.Category == SelectedCategory);
        }

        foreach (var template in filtered)
        {
            FilteredTemplates.Add(template);
        }
    }

    [RelayCommand]
    private void UseTemplate()
    {
        if (SelectedTemplate?.Template == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Create a new quest project based on:\n\n" +
            $"{SelectedTemplate.Name}\n\n" +
            $"{SelectedTemplate.Description}\n\n" +
            "This will replace your current project. Continue?",
            "Use Template",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        // Deep clone the template project so edits don't affect the original
        var clonedProject = CloneProject(SelectedTemplate.Template);

        // Raise event for MainViewModel to handle
        TemplateSelected?.Invoke(clonedProject);
    }

    /// <summary>
    /// Deep clones a QuestProject by serializing and deserializing it.
    /// </summary>
    private static QuestProject CloneProject(QuestProject original)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string json = JsonSerializer.Serialize(original, options);
        return JsonSerializer.Deserialize<QuestProject>(json, options) ?? new QuestProject();
    }
}

