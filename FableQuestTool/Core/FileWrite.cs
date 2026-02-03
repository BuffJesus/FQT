using System;
using System.IO;

namespace FableQuestTool.Core;

public static class FileWrite
{
    public static void WriteAllTextAtomic(string path, string content, bool createBackup = true)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, content);

        if (File.Exists(path))
        {
            if (createBackup)
            {
                string backupPath = path + ".bak";
                File.Replace(tempPath, path, backupPath, true);
            }
            else
            {
                File.Replace(tempPath, path, null, true);
            }
        }
        else
        {
            File.Move(tempPath, path);
        }
    }
}
