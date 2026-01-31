using System.Collections.ObjectModel;
using System.Linq;
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
        "Combat",
        "Collection",
        "Escort",
        "Travel"
    };

    public ObservableCollection<QuestTemplate> FilteredTemplates { get; } = new();
    private List<QuestTemplate> allTemplates = new();

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
        if (SelectedTemplate == null) return;

        // This will be implemented to create a new project from the template
        // For now, we just notify that it would create a new project
        System.Windows.MessageBox.Show(
            $"This would create a new quest project based on:\n\n{SelectedTemplate.Name}\n\n{SelectedTemplate.Description}",
            "Use Template",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }
}

