using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LibGit2Sharp;
using System.Runtime.CompilerServices;

namespace GmodNET.VersionTool.Core.Tests.Helpers
{
    public class TempVersionFileProvider : IDisposable
    {
        string file_path;
        bool was_disposed;

        public string FilePath => this.file_path;

        public TempVersionFileProvider()
        {
            file_path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            was_disposed = false;
        }

        ~TempVersionFileProvider()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!was_disposed)
            {
                if (File.Exists(file_path))
                {
                    File.Delete(file_path);
                }
                was_disposed = true;
            }
        }
    }

    public class TempRepoProvider : IDisposable
    {
        bool wasDisposed;
        DirectoryInfo repoDirectory;
        string version_file_path;

        public DirectoryInfo RepoDirectory => repoDirectory;

        public string RepoVersionFilePath => version_file_path;

        public TempRepoProvider(string version_file_to_copy_into_repo)
        {
            wasDisposed = false;

            repoDirectory = Directory.CreateDirectory(Path.GetTempPath() + Guid.NewGuid().ToString());

            Repository.Init(repoDirectory.FullName);

            version_file_path = Path.Combine(repoDirectory.FullName, version_file_to_copy_into_repo);

            File.Copy(version_file_to_copy_into_repo, version_file_path);

            using Repository repository = new Repository(repoDirectory.FullName);

            Commands.Stage(repository, "*");

            Signature repo_commiter_signature = new Signature("Test runner", "support@gmodnet.xyz", DateTimeOffset.Now);

            repository.Commit("Initial commit", repo_commiter_signature, repo_commiter_signature);
        }

        public void Dispose()
        {
            if(!wasDisposed)
            {
                if(repoDirectory.Exists)
                {
                    repoDirectory.SetFilesAttributesToNormal();
                    repoDirectory.Delete(true);
                }
                wasDisposed = true;
            }
        }

        ~TempRepoProvider()
        {
            Dispose();
        }
    }

    public static class HelperExtensions
    {
        public static void SetFilesAttributesToNormal(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
            }

            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                subdirectory.SetFilesAttributesToNormal();
            }
        }
    }
}
