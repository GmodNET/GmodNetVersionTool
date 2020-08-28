using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using GmodNET.VersionTool.Core;
using System.IO;
using System.Data.SqlTypes;

namespace GmodNET.VersionTool
{
    class Program
    {
        static int Main(string[] args)
        {
            Argument path_to_version_file_arg = new Argument<FileInfo>("path-to-version-file")
            {
                Description = "An absolute or relative path to a version json file."
            };

            RootCommand commandRoot = new RootCommand
            {
                Description = "GmodNET.VersionTool. SemVer 2.0.0 compatible version generator."
            };

            Command getVersionCommand = new Command("getVersion")
            {
                Description = "Get the full version including build metadata."
            };

            Option skipBuildDataOption = new Option<bool>("--skip-build-data", "Should the version generator skip build metadata in the generated version.");

            getVersionCommand.AddArgument(path_to_version_file_arg);
            getVersionCommand.AddOption(skipBuildDataOption);

            getVersionCommand.Handler = CommandHandler.Create<FileInfo, bool>((FileInfo pathToVersionFile, bool skipBuildData) =>
            {
                VersionGenerator versionGenerator = new VersionGenerator(pathToVersionFile.FullName);

                if (!skipBuildData)
                {
                    Console.WriteLine(versionGenerator.FullVersion);
                }
                else
                {
                    Console.WriteLine(versionGenerator.VersionWithoutBuildData);
                }
            });

            Command getCommitCommand = new Command("getCommit", "Get the git commit hash (first 7 symbols) of the the given version.");

            getCommitCommand.AddArgument(path_to_version_file_arg);

            getCommitCommand.Handler = CommandHandler.Create((FileInfo pathToVersionFile) =>
            {
                VersionGenerator versionGenerator = new VersionGenerator(pathToVersionFile.FullName);

                Console.WriteLine(versionGenerator.CommitHash);
            });

            Command getBranchNameCommand = new Command("getBranchName", "Get the git branch of the given version.");

            getBranchNameCommand.AddArgument(path_to_version_file_arg);

            getBranchNameCommand.Handler = CommandHandler.Create((FileInfo pathToVersionFile) =>
            {
                VersionGenerator versionGenerator = new VersionGenerator(pathToVersionFile.FullName);

                Console.WriteLine(versionGenerator.BranchName);
            });

            commandRoot.Add(getVersionCommand);
            commandRoot.Add(getCommitCommand);
            commandRoot.Add(getBranchNameCommand);

            return commandRoot.Invoke(args);
        }
    }
}
