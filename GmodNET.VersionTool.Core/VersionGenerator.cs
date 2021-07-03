﻿using Newtonsoft.Json;
using System;
using System.IO;
using Semver;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace GmodNET.VersionTool.Core
{
    /// <summary>
    /// Represents a version generator.
    /// </summary>
    public class VersionGenerator
    {
        readonly string full_version;
        readonly string version_without_build_data;
        readonly string branch_name;
        readonly string commit_hash;

        /// <summary>
        /// Returns a full generated version with build metadata.
        /// </summary>
        public string FullVersion
        {
            get {return full_version;}
        }

        /// <summary>
        /// Returnes a generated version.
        /// <remarks>
        /// Unlike <see cref="FullVersion" /> returned string does not contain build metadata.
        /// </remarks>
        /// </summary>
        public string VersionWithoutBuildData
        {
            get { return version_without_build_data; }
        }

        /// <summary>
        /// Returnes the current git repositiry HEAD branch name in human-readable format.
        /// </summary>
        public string BranchName
        {
            get {return branch_name;}
        }

        /// <summary>
        /// Returnes first 7 symbols of the current git commit hash.
        /// </summary>
        public string CommitHash
        {
            get {return commit_hash;}
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionGenerator"/> class.
        /// </summary>
        /// <param name="path_to_version_file">A full or relative path to a JSON version file.</param>
        public VersionGenerator(string path_to_version_file)
        {
            string full_version_file_path = Path.GetFullPath(path_to_version_file);
            FileInfo version_file_info = new FileInfo(full_version_file_path);

            VersionStruct versionStruct;

            using(StreamReader fileStream = File.OpenText(full_version_file_path))
            {
                versionStruct = JsonSerializer.CreateDefault().Deserialize<VersionStruct>(new JsonTextReader(fileStream));
            }

            SemVersion version_from_file;

            if(versionStruct.Version == null || versionStruct.Version == String.Empty || !SemVersion.TryParse(versionStruct.Version, out version_from_file, true))
            {
                throw new ArgumentException("Version JSON file does not contain proper Version value", "path_to_version_file");
            }

            DirectoryInfo repository_directory = version_file_info.Directory;

            while(!repository_directory.GetDirectories().Where(info => info.Name == ".git").Any())
            {
                repository_directory = repository_directory.Parent;
                if (repository_directory == null)
                {
                    throw new ArgumentException("Version file is not contained in any git repository");
                }
            }

            using(Repository repo = new Repository(repository_directory.FullName))
            {
                if(repo.Head.Tip == null)
                {
                    throw new ArgumentException("Version file is contained in the repository without commits or git HEAD is corrupted");
                }

                branch_name = repo.Head.FriendlyName;

                StringBuilder version_string_builder = new StringBuilder();

                Regex non_semver_characters_regex = new Regex(@"[^0-9A-Za-z-]+", RegexOptions.ECMAScript | RegexOptions.Compiled);
                string normalized_head_name = non_semver_characters_regex.Replace(branch_name, "-");
                
                version_string_builder.Append(version_from_file.Major);
                version_string_builder.Append('.');
                version_string_builder.Append(version_from_file.Minor);
                version_string_builder.Append('.');
                version_string_builder.Append(version_from_file.Patch);

                if(version_from_file.Prerelease != null && version_from_file.Prerelease != String.Empty)
                {
                    version_string_builder.Append('-');
                    version_string_builder.Append(version_from_file.Prerelease);

                    DateTime commit_time = repo.Head.Tip.Committer.When.UtcDateTime;
                    DateTimeOffset commit_time_offset = new DateTimeOffset(commit_time);

                    // The numerical constant here is UNIX time of January 1st, 2020 12:00 AM UTC
                    long commit_time_in_seconds = commit_time_offset.ToUnixTimeSeconds() - 1577836800;

                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time_in_seconds);

                    version_string_builder.Append('.');
                    version_string_builder.Append(normalized_head_name);
                }

                version_without_build_data = version_string_builder.ToString();

                version_string_builder.Append('+');

                Regex codename_regex = new Regex(@"^[0-9A-Za-z-]+$", RegexOptions.ECMAScript | RegexOptions.Compiled);
                if(versionStruct.Codename != null && versionStruct.Codename != String.Empty && codename_regex.IsMatch(versionStruct.Codename))
                {
                    version_string_builder.Append("codename");
                    version_string_builder.Append('.');
                    version_string_builder.Append(versionStruct.Codename);
                    version_string_builder.Append('.');
                }

                version_string_builder.Append("head");
                version_string_builder.Append('.');
                version_string_builder.Append(normalized_head_name);
                version_string_builder.Append('.');
                version_string_builder.Append("commit");
                version_string_builder.Append('.');
                
                commit_hash = repo.Head.Tip.Sha;

                version_string_builder.Append(commit_hash);

                if(version_from_file.Build != null && version_from_file.Build != String.Empty)
                {
                    version_string_builder.Append('.');
                    version_string_builder.Append(version_from_file.Build);
                }

                full_version = version_string_builder.ToString();
            }
        }
    }
}
