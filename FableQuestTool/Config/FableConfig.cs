using FableQuestTool.Core;
using Microsoft.Win32;
using System;
using System.IO;

namespace FableQuestTool.Config;

public sealed class FableConfig
{
    private readonly IniFile? ini;
    private readonly string? iniPath;

    private FableConfig(string? iniPath, IniFile? ini)
    {
        this.iniPath = iniPath;
        this.ini = ini;
    }

    public string? FablePath { get; private set; }

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

    public void Save()
    {
        if (ini == null || string.IsNullOrWhiteSpace(iniPath))
        {
            return;
        }

        ini.Save(iniPath);
    }

    public bool EnsureFablePath()
    {
        if (IsValidFablePath(FablePath))
        {
            return true;
        }

        return PromptForFablePath();
    }

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

    public void SetFablePath(string path)
    {
        FablePath = path;
        ini?.Set("Settings", "FablePath", path);
    }

    public string[] GetFavoriteNodeTypes()
    {
        string? raw = ini?.Get("UI", "FavoriteNodes");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public void SetFavoriteNodeTypes(IEnumerable<string> types)
    {
        if (ini == null)
        {
            return;
        }

        string value = string.Join('|', types);
        ini.Set("UI", "FavoriteNodes", value);
    }

    public string? GetFseFolder()
    {
        if (string.IsNullOrWhiteSpace(FablePath))
        {
            return null;
        }

        return Path.Combine(FablePath, "FSE");
    }

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
