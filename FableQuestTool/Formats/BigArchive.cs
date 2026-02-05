using FableQuestTool.Core;
using System.Collections.Generic;
using System.IO;

namespace FableQuestTool.Formats;

/// <summary>
/// Represents a BIG archive made up of one or more banks.
/// </summary>
public sealed class BigArchive
{
    /// <summary>
    /// Gets Version.
    /// </summary>
    public uint Version { get; internal set; } = 100;
    /// <summary>
    /// Gets or sets ContentType.
    /// </summary>
    public uint ContentType { get; set; }
    /// <summary>
    /// Member Banks.
    /// </summary>
    public IReadOnlyList<BigBank> Banks => _banks;

    private readonly List<BigBank> _banks = new();

    /// <summary>
    /// Loads a BIG archive from a file path.
    /// </summary>
    public static BigArchive Load(string filePath)
    {
        Guard.NotNullOrEmpty(filePath, nameof(filePath));
        using FileStream stream = File.OpenRead(filePath);
        return Load(stream);
    }

    /// <summary>
    /// Loads a BIG archive from an input stream.
    /// </summary>
    public static BigArchive Load(Stream stream)
    {
        Guard.NotNull(stream, nameof(stream));
        return BigReader.Read(stream);
    }

    /// <summary>
    /// Adds a bank to the archive.
    /// </summary>
    public void AddBank(BigBank bank)
    {
        Guard.NotNull(bank, nameof(bank));
        _banks.Add(bank);
    }

    /// <summary>
    /// Finds an entry by symbol name across all banks.
    /// </summary>
    public BigEntry? FindEntry(string symbolName)
    {
        foreach (var bank in _banks)
        {
            foreach (var entry in bank.Entries)
            {
                if (entry.SymbolName.Equals(symbolName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all entries from all banks.
    /// </summary>
    public List<BigEntry> GetAllEntries()
    {
        var allEntries = new List<BigEntry>();
        foreach (var bank in _banks)
        {
            allEntries.AddRange(bank.Entries);
        }
        return allEntries;
    }
}
