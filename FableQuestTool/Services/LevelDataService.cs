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

    /// <summary>
    /// Creates a new instance of LevelDataService.
    /// </summary>
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
            System.Diagnostics.Debug.WriteLine($"LevelDataService: Using cached archives ({cachedArchives.Count} archives)");
            return cachedArchives;
        }

        cachedArchives = new Dictionary<string, BigArchive>(StringComparer.OrdinalIgnoreCase);
        var bigFiles = GetBigFiles();
        System.Diagnostics.Debug.WriteLine($"LevelDataService: Found {bigFiles.Count} BIG files in data directory");

        foreach (var bigFile in bigFiles)
        {
            System.Diagnostics.Debug.WriteLine($"LevelDataService: Loading {Path.GetFileName(bigFile)}...");
            var archive = LoadBigArchive(bigFile);
            if (archive != null)
            {
                string fileName = Path.GetFileName(bigFile);
                cachedArchives[fileName] = archive;
                int entryCount = archive.GetAllEntries().Count;
                System.Diagnostics.Debug.WriteLine($"LevelDataService: Loaded {fileName} with {entryCount} entries");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"LevelDataService: Failed to load {Path.GetFileName(bigFile)}");
            }
        }

        System.Diagnostics.Debug.WriteLine($"LevelDataService: Successfully loaded {cachedArchives.Count} archives");
        return cachedArchives;
    }

    /// <summary>
    /// Finds TNG files within BIG archives.
    /// </summary>
    public List<BigEntry> FindTngEntries()
    {
        var archives = LoadAllBigArchives();
        var tngEntries = new List<BigEntry>();

        System.Diagnostics.Debug.WriteLine($"LevelDataService.FindTngEntries: Searching {archives.Count} archives for TNG files");

        foreach (var kvp in archives)
        {
            var allEntries = kvp.Value.GetAllEntries();
            System.Diagnostics.Debug.WriteLine($"LevelDataService.FindTngEntries: Archive '{kvp.Key}' has {allEntries.Count} total entries");

            // Sample first 10 entries to see what's in the archive
            var sample = allEntries.Take(10).Select(e => e.SymbolName).ToList();
            System.Diagnostics.Debug.WriteLine($"LevelDataService.FindTngEntries: Sample entries from '{kvp.Key}': {string.Join(", ", sample)}");

            var entries = allEntries
                .Where(e => e.SymbolName.EndsWith(".tng", StringComparison.OrdinalIgnoreCase))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"LevelDataService.FindTngEntries: Found {entries.Count} TNG entries in '{kvp.Key}'");
            tngEntries.AddRange(entries);
        }

        System.Diagnostics.Debug.WriteLine($"LevelDataService.FindTngEntries: Total TNG entries found: {tngEntries.Count}");
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
    /// <summary>
    /// Gets or sets Name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets LevFilePath.
    /// </summary>
    public string LevFilePath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets HasLevFile.
    /// </summary>
    public bool HasLevFile { get; set; }
    /// <summary>
    /// Executes This member.
    /// </summary>
    public List<string> TngFiles { get; set; } = new();
    /// <summary>
    /// Gets or sets EntityCount.
    /// </summary>
    public int EntityCount { get; set; }
}
