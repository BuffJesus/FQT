using FableQuestTool.Core;
using System.Collections.Generic;

namespace FableQuestTool.Formats;

/// <summary>
/// Represents a bank inside a BIG archive and its entry list.
/// </summary>
public sealed class BigBank
{
    /// <summary>
    /// Gets Name.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets Id.
    /// </summary>
    public uint Id { get; internal set; }
    /// <summary>
    /// Gets BlockSize.
    /// </summary>
    public uint BlockSize { get; internal set; }
    /// <summary>
    /// Gets EntryStartOffset.
    /// </summary>
    public uint EntryStartOffset { get; internal set; }
    /// <summary>
    /// Gets Length.
    /// </summary>
    public uint Length { get; internal set; }
    /// <summary>
    /// Member Entries.
    /// </summary>
    public IReadOnlyList<BigEntry> Entries => _entries;

    private readonly List<BigEntry> _entries = new();

    /// <summary>
    /// Creates a bank with the specified name.
    /// </summary>
    public BigBank(string name)
    {
        Guard.NotNullOrEmpty(name, nameof(name));
        Name = name;
    }

    /// <summary>
    /// Creates a bank with a name, id, and block size.
    /// </summary>
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

    /// <summary>
    /// Adds a new entry to the bank.
    /// </summary>
    public void AddNewEntry(BigEntry entry)
    {
        Guard.NotNull(entry, nameof(entry));
        _entries.Add(entry);
    }
}
