using FableQuestTool.Core;
using System.Collections.Generic;
using System.IO;

namespace FableQuestTool.Formats;

public sealed class BigArchive
{
    public uint Version { get; internal set; } = 100;
    public uint ContentType { get; set; }
    public IReadOnlyList<BigBank> Banks => _banks;

    private readonly List<BigBank> _banks = new();

    public static BigArchive Load(string filePath)
    {
        Guard.NotNullOrEmpty(filePath, nameof(filePath));
        using FileStream stream = File.OpenRead(filePath);
        return Load(stream);
    }

    public static BigArchive Load(Stream stream)
    {
        Guard.NotNull(stream, nameof(stream));
        return BigReader.Read(stream);
    }

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
