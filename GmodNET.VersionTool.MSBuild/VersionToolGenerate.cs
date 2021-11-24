using System;
using Microsoft.Build.Framework;
using GmodNET.VersionTool.Core;

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
                    Log.LogError("(GmodNET.VersionTool.MSBuild) Version file is not specified.");
                    return false;
                }
                else if (VersionFiles.Length > 1)
                {
                    Log.LogError("(GmodNET.VersionTool.MSBuild) There is more than one version file for the project.");
                    return false;
                }

                VersionGenerator gen = new VersionGenerator(VersionFiles[0]);

                FullVersion = gen.FullVersion;
                ShortVersion = gen.VersionWithoutBuildData;

                return true;    
            }
            catch(Exception e)
            {
                Log.LogError("(GmodNET.VersionTool.MSBuild) Exception was thrown while executing VersionToolGenerate task: " + e.ToString());
                return false;
            }
        }
    }
}
