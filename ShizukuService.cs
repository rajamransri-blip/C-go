using System;
using System.Diagnostics;

namespace KuronamiGfx;

public static class ShizukuService
{
    public static void ExecuteShizuku(string command)
    {
        try
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/system/bin/sh",
                    Arguments = $"-c \"/data/local/tmp/rish -c '{command}'\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
        catch { }
    }

    public static void ApplyConfig(string localPath, string packageName, string targetSubpath, string fileName)
    {
        string destDir = $"/sdcard/Android/data/{packageName}/{targetSubpath}";
        string destFile = $"{destDir}/{fileName}";
        ExecuteShizuku($"mkdir -p '{destDir}'");
        ExecuteShizuku($"cp '{localPath}' '{destFile}'");
        ExecuteShizuku($"chmod 660 '{destFile}'");
    }
}
