using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace FableQuestTool.Services;

public static class WadBridgeClient
{
    public static bool TryListEntries(string wadPath, out List<WadEntryInfo> entries, out string? error)
    {
        entries = new List<WadEntryInfo>();
        error = null;

        string? bridgePath = FindBridge("SilverChest.WadBridge.exe");
        if (bridgePath == null)
        {
            error = "SilverChest.WadBridge.exe not found.";
            return false;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = bridgePath,
            Arguments = $"--list \"{wadPath}\"",
            WorkingDirectory = Path.GetDirectoryName(bridgePath) ?? AppContext.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            error = "WAD list failed: unable to start bridge process.";
            return false;
        }

        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(30000))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            error = "WAD list failed: bridge process timed out.";
            return false;
        }

        if (process.ExitCode != 0)
        {
            string detail = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            error = $"WAD list failed: {detail.Trim()}";
            return false;
        }

        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<WadEntryInfo>? parsed = JsonSerializer.Deserialize<List<WadEntryInfo>>(stdout, options);
            if (parsed != null)
            {
                entries = parsed;
            }
        }
        catch (Exception ex)
        {
            error = $"WAD list failed: {ex.Message}";
            return false;
        }

        return true;
    }

    public static bool TryExtractEntry(string wadPath, int index, string outputPath, out string? error)
    {
        error = null;
        string? bridgePath = FindBridge("SilverChest.WadBridge.exe");
        if (bridgePath == null)
        {
            error = "SilverChest.WadBridge.exe not found.";
            return false;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = bridgePath,
            Arguments = $"--extract \"{wadPath}\" {index} \"{outputPath}\"",
            WorkingDirectory = Path.GetDirectoryName(bridgePath) ?? AppContext.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            error = "WAD extract failed: unable to start bridge process.";
            return false;
        }

        string stderr = process.StandardError.ReadToEnd();
        string stdout = process.StandardOutput.ReadToEnd();

        if (!process.WaitForExit(30000))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            error = "WAD extract failed: bridge process timed out.";
            return false;
        }

        if (process.ExitCode != 0)
        {
            string detail = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            error = $"WAD extract failed: {detail.Trim()}";
            return false;
        }

        return true;
    }

    public static bool TryExtractAll(string wadPath, string outputDir, out string? error)
    {
        error = null;
        string? bridgePath = FindBridge("SilverChest.WadBridge.exe");
        if (bridgePath == null)
        {
            error = "SilverChest.WadBridge.exe not found.";
            return false;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = bridgePath,
            Arguments = $"--extract-all \"{wadPath}\" \"{outputDir}\"",
            WorkingDirectory = Path.GetDirectoryName(bridgePath) ?? AppContext.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            error = "WAD extract failed: unable to start bridge process.";
            return false;
        }

        string stderr = process.StandardError.ReadToEnd();
        string stdout = process.StandardOutput.ReadToEnd();

        if (!process.WaitForExit(120000))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            error = "WAD extract failed: bridge process timed out.";
            return false;
        }

        if (process.ExitCode != 0)
        {
            string detail = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            error = $"WAD extract failed: {detail.Trim()}";
            return false;
        }

        return true;
    }

    private static string? FindBridge(string fileName)
    {
        string baseDir = AppContext.BaseDirectory;
        string toolsDir = Path.Combine(baseDir, "tools");
        string toolPath = Path.Combine(toolsDir, fileName);
        if (File.Exists(toolPath))
        {
            return toolPath;
        }

        string direct = Path.Combine(baseDir, fileName);
        return File.Exists(direct) ? direct : null;
    }
}

public sealed class WadEntryInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint Size { get; set; }
    public string Type { get; set; } = string.Empty;
}
