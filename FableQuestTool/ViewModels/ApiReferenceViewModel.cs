using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class ApiReferenceViewModel : ObservableObject
{
    private readonly ApiParser apiParser = new();
    private List<ApiFunction> allFunctions = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedCategory = "All";

    [ObservableProperty]
    private ApiFunction? selectedFunction;

    public ObservableCollection<string> Categories { get; } = new()
    {
        "All",
        "Entity API",
        "Quest API",
        "Hero API",
        "Game API"
    };

    public ObservableCollection<ApiFunction> FilteredFunctions { get; } = new();

    public ApiReferenceViewModel()
    {
        LoadApiData();
    }

    private void LoadApiData()
    {
        // Try to find the API header file
        string[] possiblePaths = new[]
        {
            @"D:\Documents\JetBrains\FQT\SourceFilesToReference\OldQuestManager\FseQuestEditor\data\script_extender\ALL-INTERFACE-FUNCTIONS-FOR-FSE.h",
            @"data\script_extender\ALL-INTERFACE-FUNCTIONS-FOR-FSE.h",
            @"..\..\SourceFilesToReference\OldQuestManager\FseQuestEditor\data\script_extender\ALL-INTERFACE-FUNCTIONS-FOR-FSE.h"
        };

        foreach (string path in possiblePaths)
        {
            if (System.IO.File.Exists(path))
            {
                allFunctions = apiParser.ParseHeaderFile(path);
                UpdateFilteredFunctions();
                return;
            }
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredFunctions();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        UpdateFilteredFunctions();
    }

    private void UpdateFilteredFunctions()
    {
        FilteredFunctions.Clear();

        var filtered = allFunctions.AsEnumerable();

        // Filter by category
        if (SelectedCategory != "All")
        {
            filtered = filtered.Where(f => f.Category == SelectedCategory);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string searchLower = SearchText.ToLower();
            filtered = filtered.Where(f =>
                f.Name.ToLower().Contains(searchLower) ||
                f.Description.ToLower().Contains(searchLower));
        }

        foreach (var func in filtered.Take(200)) // Limit to 200 results for performance
        {
            FilteredFunctions.Add(func);
        }
    }

    [RelayCommand]
    private void CopyExample()
    {
        if (SelectedFunction == null) return;

        try
        {
            System.Windows.Clipboard.SetText(SelectedFunction.Example);
        }
        catch
        {
            // Silently handle clipboard errors
        }
    }

    [RelayCommand]
    private void CopySignature()
    {
        if (SelectedFunction == null) return;

        try
        {
            string signature = $"{SelectedFunction.ReturnType} {SelectedFunction.Name}(";
            signature += string.Join(", ", SelectedFunction.Parameters.Select(p => $"{p.Type} {p.Name}"));
            signature += ")";

            System.Windows.Clipboard.SetText(signature);
        }
        catch
        {
            // Silently handle clipboard errors
        }
    }
}
