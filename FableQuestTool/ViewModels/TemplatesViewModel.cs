using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class TemplatesViewModel : ObservableObject
{
    private readonly TemplateService templateService = new();
    private readonly QuestIdManager questIdManager;

    [ObservableProperty]
    private QuestTemplate? selectedTemplate;

    [ObservableProperty]
    private string selectedCategory = "All";

    /// <summary>
    /// Executes This member.
    /// </summary>
    public ObservableCollection<string> Categories { get; } = new();

    /// <summary>
    /// Executes This member.
    /// </summary>
    public ObservableCollection<QuestTemplate> FilteredTemplates { get; } = new();
    private List<QuestTemplate> allTemplates = new();

    /// <summary>
    /// Event raised when a template is selected for use.
    /// The MainViewModel subscribes to this to load the template.
    /// </summary>
    public event Action<QuestProject>? TemplateSelected;

    /// <summary>
    /// Creates a new instance of TemplatesViewModel.
    /// </summary>
    public TemplatesViewModel()
    {
        questIdManager = new QuestIdManager(FableConfig.Load());
        allTemplates = templateService.GetAllTemplates();
        RefreshCategories();
        SelectedCategory = "All";
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

        // Assign a new quest ID if possible
        clonedProject.Id = questIdManager.SuggestNextQuestId();

        // Ensure quest name doesn't collide with existing quests
        var existingNames = questIdManager.GetExistingQuestNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
        clonedProject.Name = EnsureUniqueQuestName(clonedProject.Name, existingNames);

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

    private void RefreshCategories()
    {
        Categories.Clear();
        Categories.Add("All");

        foreach (string category in allTemplates
                     .Select(t => t.Category)
                     .Where(c => !string.IsNullOrWhiteSpace(c))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(c => c))
        {
            Categories.Add(category);
        }
    }

    private static string EnsureUniqueQuestName(string name, HashSet<string> existingNames)
    {
        if (string.IsNullOrWhiteSpace(name) || existingNames.Count == 0)
        {
            return name;
        }

        if (!existingNames.Contains(name))
        {
            return name;
        }

        int suffix = 2;
        string candidate = $"{name}_{suffix}";
        while (existingNames.Contains(candidate))
        {
            suffix++;
            candidate = $"{name}_{suffix}";
        }

        return candidate;
    }
}

