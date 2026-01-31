using FableQuestTool.Core;
using System.Collections.Generic;

namespace FableQuestTool.Formats;

public sealed class BigBank
{
    public string Name { get; }
    public uint Id { get; internal set; }
    public uint BlockSize { get; internal set; }
    public uint EntryStartOffset { get; internal set; }
    public uint Length { get; internal set; }
    public IReadOnlyList<BigEntry> Entries => _entries;

    private readonly List<BigEntry> _entries = new();

    public BigBank(string name)
    {
        Guard.NotNullOrEmpty(name, nameof(name));
        Name = name;
    }

    public BigBank(string name, uint id, uint blockSize) : this(name)
    {
        Id = id;
        BlockSize = blockSize;
    }

    internal void AddEntry(BigEntry entry)
    {
        Guard.NotNull(entry, nameof(entry));
        _entries.Add(entry);
    }

    public void AddNewEntry(BigEntry entry)
    {
        Guard.NotNull(entry, nameof(entry));
        _entries.Add(entry);
    }
}
