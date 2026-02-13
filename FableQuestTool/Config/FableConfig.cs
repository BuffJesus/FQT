using FableQuestTool.Core;
using Microsoft.Win32;
using System;
using System.IO;

namespace FableQuestTool.Config;

/// <summary>
/// Manages persisted settings such as the Fable install path and UI preferences.
/// </summary>
public sealed class FableConfig
{
    private readonly IniFile? ini;
    private readonly string? iniPath;

    private FableConfig(string? iniPath, IniFile? ini)
    {
        this.iniPath = iniPath;
        this.ini = ini;
    }

    /// <summary>
    /// Gets the resolved Fable installation path, if available.
    /// </summary>
    public string? FablePath { get; private set; }

    /// <summary>
    /// Loads configuration from the local INI file and resolves defaults.
    /// </summary>
    public static FableConfig Load()
    {
        string? iniPath = FindIniPath();
        IniFile? ini = null;
        if (!string.IsNullOrWhiteSpace(iniPath) && File.Exists(iniPath))
        {
            ini = IniFile.Load(iniPath);
        }
        else if (!string.IsNullOrWhiteSpace(iniPath))
        {
            ini = IniFile.CreateEmpty();
        }

        FableConfig config = new FableConfig(iniPath, ini);
        config.ResolveFablePath();
        return config;
    }

    /// <summary>
    /// Writes the current configuration back to disk.
    /// </summary>
    public void Save()
    {
        if (ini == null || string.IsNullOrWhiteSpace(iniPath))
        {
            return;
        }

        ini.Save(iniPath);
    }

    /// <summary>
    /// Ensures the Fable path is valid, prompting the user if needed.
    /// </summary>
    public bool EnsureFablePath()
    {
        if (IsValidFablePath(FablePath))
        {
            return true;
        }

        return PromptForFablePath();
    }

    /// <summary>
    /// Opens a folder picker to set the Fable install path.
    /// </summary>
    public bool PromptForFablePath()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.Description = "Select your Fable: The Lost Chapters installation folder";

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return false;
        }

        string selected = dialog.SelectedPath;
        if (!IsValidFablePath(selected))
        {
            System.Windows.MessageBox.Show(
                "Selected folder does not look like a Fable installation (missing data folder).",
                "Invalid Folder",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }

        SetFablePath(selected);
        Save();
        return true;
    }

    /// <summary>
    /// Sets the Fable install path and persists it to the INI.
    /// </summary>
    public void SetFablePath(string path)
    {
        FablePath = path;
        ini?.Set("Settings", "FablePath", path);
    }

    /// <summary>
    /// Gets the list of favorite node types for the node menu.
    /// </summary>
    public string[] GetFavoriteNodeTypes()
    {
        string? raw = ini?.Get("UI", "FavoriteNodes");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Stores favorite node types for quick access in the UI.
    /// </summary>
    public void SetFavoriteNodeTypes(IEnumerable<string> types)
    {
        if (ini == null)
        {
            return;
        }

        string value = string.Join('|', types);
        ini.Set("UI", "FavoriteNodes", value);
    }

    /// <summary>
    /// Gets whether the startup splash image is enabled.
    /// </summary>
    public bool GetShowStartupImage()
    {
        string? raw = ini?.Get("UI", "ShowStartupImage");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        return bool.TryParse(raw, out bool value) ? value : true;
    }

    /// <summary>
    /// Stores the startup splash preference.
    /// </summary>
    public void SetShowStartupImage(bool value)
    {
        ini?.Set("UI", "ShowStartupImage", value.ToString());
    }

    /// <summary>
    /// Gets whether quest start screen debug logging is enabled.
    /// </summary>
    public bool GetStartScreenDebug()
    {
        string? raw = ini?.Get("Debug", "StartScreen");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        return bool.TryParse(raw, out bool value) && value;
    }

    /// <summary>
    /// Stores the quest start screen debug logging preference.
    /// </summary>
    public void SetStartScreenDebug(bool value)
    {
        ini?.Set("Debug", "StartScreen", value.ToString());
    }

    /// <summary>
    /// Gets whether a start screen debug banner should be shown in-game.
    /// </summary>
    public bool GetStartScreenDebugBanner()
    {
        string? raw = ini?.Get("Debug", "StartScreenBanner");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        return bool.TryParse(raw, out bool value) && value;
    }

    /// <summary>
    /// Stores the start screen debug banner preference.
    /// </summary>
    public void SetStartScreenDebugBanner(bool value)
    {
        ini?.Set("Debug", "StartScreenBanner", value.ToString());
    }

    /// <summary>
    /// Gets the FSE folder path based on the configured install path.
    /// </summary>
    public string? GetFseFolder()
    {
        if (string.IsNullOrWhiteSpace(FablePath))
        {
            return null;
        }

        return Path.Combine(FablePath, "FSE");
    }

    /// <summary>
    /// Gets the full path to the FSE launcher executable if available.
    /// </summary>
    public string? GetFseLauncherPath()
    {
        if (string.IsNullOrWhiteSpace(FablePath))
        {
            return null;
        }

        string launcherPath = Path.Combine(FablePath, "FSE_Launcher.exe");
        return File.Exists(launcherPath) ? launcherPath : null;
    }

    private void ResolveFablePath()
    {
        string? configured = NormalizePath(ini?.Get("Settings", "FablePath"));
        if (IsValidFablePath(configured))
        {
            FablePath = configured;
            return;
        }

        string? registryPath = NormalizePath(TryReadRegistryPath());
        if (IsValidFablePath(registryPath))
        {
            FablePath = registryPath;
            ini?.Set("Settings", "FablePath", registryPath!);
        }
    }

    private static string? TryReadRegistryPath()
    {
        string keyPath = @"Software\Microsoft\Microsoft Games\Fable\1.0\";
        string[] valueNames = { "AppPath", "InstallPath", "Path" };
        RegistryView[] views = { RegistryView.Registry64, RegistryView.Registry32 };
        RegistryHive[] hives = { RegistryHive.LocalMachine, RegistryHive.CurrentUser };

        foreach (RegistryHive hive in hives)
        {
            foreach (RegistryView view in views)
            {
                try
                {
                    using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using RegistryKey? subKey = baseKey.OpenSubKey(keyPath);
                    if (subKey == null)
                    {
                        continue;
                    }

                    foreach (string name in valueNames)
                    {
                        string? value = Convert.ToString(subKey.GetValue(name));
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        return null;
    }

    private static bool IsValidFablePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        string candidate = NormalizePath(path) ?? string.Empty;
        if (candidate.Length == 0)
        {
            return false;
        }

        return Directory.Exists(candidate) && Directory.Exists(Path.Combine(candidate, "data"));
    }

    private static string? NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim().Trim('"');
        if (trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetDirectoryName(trimmed);
        }

        return trimmed;
    }

    private static string? FindIniPath()
    {
        string[] names = { "FableQuestTool.ini", "FableQuestTool.INI" };

        if (!string.IsNullOrWhiteSpace(AppContext.BaseDirectory))
        {
            foreach (string name in names)
            {
                string candidate = Path.Combine(AppContext.BaseDirectory, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return Path.Combine(AppContext.BaseDirectory, names[0]);
    }
}
