using Newtonsoft.Json;
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
    public class VersionGenerator
    {
        readonly string full_version;
        readonly string branch_name;
        readonly string commit_hash;

        public string FullVersion
        {
            get {return full_version;}
        }

        public string BranchName
        {
            get {return branch_name;}
        }

        public string CommitHash
        {
            get {return commit_hash;}
        }

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

            if(versionStruct.Version == null || versionStruct.Version == String.Empty || !SemVersion.TryParse(versionStruct.Version, out version_from_file))
            {
                throw new ArgumentException("Version JSON file does not contain proper Version value", "path_to_version_file");
            }

            using(Repository repo = new Repository(version_file_info.DirectoryName))
            {
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

                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time.Year);
                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time.Month);
                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time.Day);
                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time.Hour);
                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time.Minute);
                    version_string_builder.Append('.');
                    version_string_builder.Append(commit_time.Second);

                    version_string_builder.Append('.');
                    version_string_builder.Append(normalized_head_name);
                }

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
                
                commit_hash = repo.Head.Tip.Sha.Substring(0, 7);

                version_string_builder.Append(commit_hash);
            }
        }
    }
}
