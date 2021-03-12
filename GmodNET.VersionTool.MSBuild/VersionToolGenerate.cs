using System;
using Microsoft.Build.Framework;
using GmodNET.VersionTool.Core;
using System.Runtime.InteropServices;
using System.IO;

namespace GmodNET.VersionTool.MSBuild
{
    public class VersionToolGenerate : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string[] VersionFiles { get; set; }

        [Output]
        public string FullVersion { private set; get; }

        [Output]
        public string ShortVersion { private set; get; }

        public override bool Execute()
        {
            try
            {
                if(VersionFiles.Length == 0)
                {
                    Log.LogError("Version file is not specified.");
                    return false;
                }
                else if (VersionFiles.Length > 1)
                {
                    Log.LogError("There is more than one version file for the project.");
                    return false;
                }

                string[] platforms = new string[]
                {
                    "win-x86",
                    "win-x64",
                    "osx",
                    "linux-arm",
                    "linux-arm64",
                    "linux-musl-x64",
                    "linux-x64"
                };

                string path_var = Environment.GetEnvironmentVariable("PATH");

                path_var += Path.PathSeparator + Path.Combine(Path.GetDirectoryName(typeof(VersionToolGenerate).Assembly.Location), $"runtimes\\win-x64\\native");

                Environment.SetEnvironmentVariable("PATH", path_var);

                Log.LogWarning($"Resulting 11 PATH: ${Environment.GetEnvironmentVariable("PATH")}");

                VersionGenerator generator = new VersionGenerator(VersionFiles[0]);

                FullVersion = generator.FullVersion;
                ShortVersion = generator.VersionWithoutBuildData;

                return true;    
            }
            catch(Exception e)
            {
                Log.LogError("Exception was thrown while executing VersionToolGenerate task (GmodNET.VersionTool.MSBuild): " + e.ToString());
                return false;
            }
        }

        static OSPlatform GetOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            else
            {
                return OSPlatform.Create("UNKNOWN");
            }
        }
    }
}
