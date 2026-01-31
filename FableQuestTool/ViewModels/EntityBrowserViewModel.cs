using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Models;
using FableQuestTool.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityBrowserViewModel : ObservableObject
{
    private readonly EntityBrowserService entityBrowser;

    [ObservableProperty]
    private ObservableCollection<TngEntity> entities = new();

    [ObservableProperty]
    private ObservableCollection<TngEntity> filteredEntities = new();

    [ObservableProperty]
    private TngEntity? selectedEntity;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedRegion = "All Regions";

    [ObservableProperty]
    private EntityCategory selectedCategory = EntityCategory.Unknown;

    [ObservableProperty]
    private bool scriptableOnly = true;

    [ObservableProperty]
    private string statusText = "Ready";

    public ObservableCollection<string> Regions { get; } = new();
    public ObservableCollection<EntityCategory> Categories { get; } = new();

    public EntityBrowserViewModel()
    {
        var config = FableConfig.Load();
        entityBrowser = new EntityBrowserService(config);

        // Populate regions
        Regions.Add("All Regions");
        foreach (var region in entityBrowser.GetAvailableRegions())
        {
            Regions.Add(region);
        }

        // Populate categories
        Categories.Add(EntityCategory.Unknown); // "All"
        Categories.Add(EntityCategory.Marker);
        Categories.Add(EntityCategory.NPC);
        Categories.Add(EntityCategory.Creature);
        Categories.Add(EntityCategory.Object);
        Categories.Add(EntityCategory.Chest);
        Categories.Add(EntityCategory.Door);
        Categories.Add(EntityCategory.QuestItem);
    }

    [RelayCommand]
    private void LoadEntities()
    {
        StatusText = "Loading entities...";

        try
        {
            var allEntities = SelectedRegion == "All Regions"
                ? entityBrowser.GetAllEntities()
                : entityBrowser.GetEntitiesForRegion(SelectedRegion, ScriptableOnly);

            Entities.Clear();
            foreach (var entity in allEntities)
            {
                Entities.Add(entity);
            }

            ApplyFilters();
            StatusText = $"Loaded {FilteredEntities.Count} entities";
        }
        catch (System.Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var filtered = Entities.AsEnumerable();

        // Filter by scriptable only
        if (ScriptableOnly)
        {
            filtered = filtered.Where(e => e.HasScriptName);
        }

        // Filter by category
        if (SelectedCategory != EntityCategory.Unknown)
        {
            filtered = filtered.Where(e => e.Category == SelectedCategory);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(e =>
                (e.ScriptName?.ToLowerInvariant().Contains(search) ?? false) ||
                (e.DefinitionType?.ToLowerInvariant().Contains(search) ?? false));
        }

        FilteredEntities.Clear();
        foreach (var entity in filtered)
        {
            FilteredEntities.Add(entity);
        }

        StatusText = $"Showing {FilteredEntities.Count} of {Entities.Count} entities";
    }

    [RelayCommand]
    private void RefreshCache()
    {
        entityBrowser.ClearCache();
        LoadEntities();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(EntityCategory value)
    {
        ApplyFilters();
    }

    partial void OnScriptableOnlyChanged(bool value)
    {
        ApplyFilters();
    }

    partial void OnSelectedRegionChanged(string value)
    {
        LoadEntities();
    }
}
