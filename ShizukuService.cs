using System;
using System.Diagnostics;

namespace KuronamiGfx;

public static class ShizukuService
{
    public static bool CheckShizukuActive()
    {
        try
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/system/bin/sh",
                    Arguments = "-c \"/data/local/tmp/rish -c 'id'\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return output.Contains("uid=2000") || output.Contains("root") || output.Contains("shell");
        }
        catch
        {
            return false;
        }
    }

    public static bool ApplyConfigToPaks(string localPath, string packageName, string targetSubpath, string fileName)
    {
        try
        {
            string destDir = $"/sdcard/Android/data/{packageName}/{targetSubpath}";
            string destFile = $"{destDir}/{fileName}";

            ExecuteShizuku($"mkdir -p '{destDir}'");
            ExecuteShizuku($"cp '{localPath}' '{destFile}'");
            ExecuteShizuku($"chmod 660 '{destFile}'");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ExecuteShizuku(string command)
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/system/bin/sh",
                Arguments = $"-c \"/data/local/tmp/rish -c '{command}'\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        proc.Start();
        proc.WaitForExit();
    }
}
