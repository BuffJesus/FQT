using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FableQuestTool.ViewModels;

/// <summary>
/// ViewModel for the in-app user guide.
/// </summary>
public sealed partial class HelpViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "FQT User Guide";

    [ObservableProperty]
    private string content = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private HelpSection? selectedSection;

    public ObservableCollection<HelpSection> Sections { get; } = new();
    public ObservableCollection<HelpSection> FilteredSections { get; } = new();

    public void Load(string rawContent)
    {
        Sections.Clear();
        FilteredSections.Clear();

        foreach (var section in ParseSections(rawContent))
        {
            Sections.Add(section);
        }

        ApplyFilter();
        SelectedSection = FilteredSections.FirstOrDefault();
        Content = SelectedSection?.Content ?? rawContent;
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedSectionChanged(HelpSection? value)
    {
        if (value == null)
        {
            return;
        }

        Content = value.Content;
    }

    private void ApplyFilter()
    {
        FilteredSections.Clear();

        IEnumerable<HelpSection> filtered = Sections;
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string needle = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(s =>
                s.Title.ToLowerInvariant().Contains(needle) ||
                s.Content.ToLowerInvariant().Contains(needle));
        }

        foreach (var section in filtered)
        {
            FilteredSections.Add(section);
        }
    }

    private static List<HelpSection> ParseSections(string content)
    {
        var sections = new List<HelpSection>();
        string? currentTitle = null;
        var currentLines = new List<string>();

        foreach (string line in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                AddSectionIfAny(sections, currentTitle, currentLines);
                currentTitle = line.Substring(3).Trim();
                currentLines.Clear();
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                continue;
            }

            currentLines.Add(line);
        }

        AddSectionIfAny(sections, currentTitle, currentLines);

        if (sections.Count == 0)
        {
            sections.Add(new HelpSection("Guide", content.Trim()));
        }

        return sections;
    }

    private static void AddSectionIfAny(List<HelpSection> sections, string? title, List<string> lines)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        string text = string.Join(Environment.NewLine, lines).Trim();
        sections.Add(new HelpSection(title, text));
    }

    [RelayCommand]
    private void CopySection()
    {
        if (SelectedSection == null)
        {
            return;
        }

        System.Windows.Clipboard.SetText(SelectedSection.Content);
    }

    [RelayCommand]
    private void CopyAll()
    {
        System.Windows.Clipboard.SetText(Content);
    }
}

public sealed class HelpSection
{
    public HelpSection(string title, string content)
    {
        Title = title;
        Content = content;
    }

    public string Title { get; }
    public string Content { get; }
}
