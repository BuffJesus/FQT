using System.Collections.ObjectModel;

namespace FableQuestTool.ViewModels;

/// <summary>
/// View model for the Lua preview window.
/// </summary>
public sealed class LuaPreviewViewModel
{
    /// <summary>
    /// Creates a new instance of LuaPreviewViewModel.
    /// </summary>
    public LuaPreviewViewModel(ObservableCollection<LuaPreviewItem> items)
    {
        Items = items;
    }

    /// <summary>
    /// Gets Items.
    /// </summary>
    public ObservableCollection<LuaPreviewItem> Items { get; }
}

/// <summary>
/// Represents a single Lua preview tab.
/// </summary>
public sealed class LuaPreviewItem
{
    /// <summary>
    /// Creates a new instance of LuaPreviewItem.
    /// </summary>
    public LuaPreviewItem(string title, string content)
    {
        Title = title;
        Content = content;
    }

    /// <summary>
    /// Gets Title.
    /// </summary>
    public string Title { get; }
    /// <summary>
    /// Gets Content.
    /// </summary>
    public string Content { get; }
}
