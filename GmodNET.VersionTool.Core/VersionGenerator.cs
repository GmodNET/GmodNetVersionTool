using System;
using System.IO;

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
        }
    }
}
