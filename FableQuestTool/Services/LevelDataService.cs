using FableQuestTool.Config;
using FableQuestTool.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FableQuestTool.Services;

/// <summary>
/// Service for reading level data from Fable's BIG and LEV files.
/// </summary>
public sealed class LevelDataService
{
    private readonly FableConfig config;
    private Dictionary<string, BigArchive>? cachedArchives;

    public LevelDataService(FableConfig config)
    {
        this.config = config;
    }

    /// <summary>
    /// Gets a list of all BIG files in the Fable data directory.
    /// </summary>
    public List<string> GetBigFiles()
    {
        if (string.IsNullOrWhiteSpace(config.FablePath))
        {
            return new List<string>();
        }

        string dataPath = Path.Combine(config.FablePath, "data");
        if (!Directory.Exists(dataPath))
        {
            return new List<string>();
        }

        try
        {
            return Directory.GetFiles(dataPath, "*.big", SearchOption.AllDirectories).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Loads a BIG archive from the specified path.
    /// </summary>
    public BigArchive? LoadBigArchive(string bigFilePath)
    {
        try
        {
            if (!File.Exists(bigFilePath))
            {
                return null;
            }

            return BigArchive.Load(bigFilePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading BIG file {bigFilePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads and caches all BIG archives from the Fable data directory.
    /// </summary>
    public Dictionary<string, BigArchive> LoadAllBigArchives()
    {
        if (cachedArchives != null)
        {
            return cachedArchives;
        }

        cachedArchives = new Dictionary<string, BigArchive>(StringComparer.OrdinalIgnoreCase);
        var bigFiles = GetBigFiles();

        foreach (var bigFile in bigFiles)
        {
            var archive = LoadBigArchive(bigFile);
            if (archive != null)
            {
                string fileName = Path.GetFileName(bigFile);
                cachedArchives[fileName] = archive;
            }
        }

        return cachedArchives;
    }

    /// <summary>
    /// Finds TNG files within BIG archives.
    /// </summary>
    public List<BigEntry> FindTngEntries()
    {
        var archives = LoadAllBigArchives();
        var tngEntries = new List<BigEntry>();

        foreach (var archive in archives.Values)
        {
            var entries = archive.GetAllEntries()
                .Where(e => e.SymbolName.EndsWith(".tng", StringComparison.OrdinalIgnoreCase))
                .ToList();

            tngEntries.AddRange(entries);
        }

        return tngEntries;
    }

    /// <summary>
    /// Extracts a specific entry from a BIG archive.
    /// </summary>
    public byte[]? ExtractEntry(BigEntry entry)
    {
        try
        {
            return entry.GetData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error extracting entry {entry.SymbolName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets level metadata from Graphics.big and CompiledLevel.big.
    /// </summary>
    public Dictionary<string, LevelInfo> GetLevelMetadata()
    {
        var levelInfo = new Dictionary<string, LevelInfo>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(config.FablePath))
        {
            return levelInfo;
        }

        string levelsPath = Path.Combine(config.FablePath, "data", "Levels", "FinalAlbion");
        if (Directory.Exists(levelsPath))
        {
            // Get all LEV files
            var levFiles = Directory.GetFiles(levelsPath, "*.lev", SearchOption.TopDirectoryOnly);

            foreach (var levFile in levFiles)
            {
                string levelName = Path.GetFileNameWithoutExtension(levFile);
                var info = new LevelInfo
                {
                    Name = levelName,
                    LevFilePath = levFile,
                    HasLevFile = true
                };

                // Check for corresponding TNG files
                string regionName = RegionTngMapping.InferRegionFromTngFileName(levelName);
                var tngFiles = RegionTngMapping.GetTngFilesForRegion(regionName, config.FablePath);
                info.TngFiles = tngFiles;

                levelInfo[levelName] = info;
            }
        }

        return levelInfo;
    }

    /// <summary>
    /// Clears cached data, forcing a reload on next request.
    /// </summary>
    public void ClearCache()
    {
        cachedArchives = null;
    }
}

/// <summary>
/// Information about a Fable level.
/// </summary>
public sealed class LevelInfo
{
    public string Name { get; set; } = string.Empty;
    public string LevFilePath { get; set; } = string.Empty;
    public bool HasLevFile { get; set; }
    public List<string> TngFiles { get; set; } = new();
    public int EntityCount { get; set; }
}
