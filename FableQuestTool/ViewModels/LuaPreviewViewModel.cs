using System.Collections.ObjectModel;

namespace FableQuestTool.ViewModels;

public sealed class LuaPreviewViewModel
{
    public LuaPreviewViewModel(ObservableCollection<LuaPreviewItem> items)
    {
        Items = items;
    }

    public ObservableCollection<LuaPreviewItem> Items { get; }
}

public sealed class LuaPreviewItem
{
    public LuaPreviewItem(string title, string content)
    {
        Title = title;
        Content = content;
    }

    public string Title { get; }
    public string Content { get; }
}
