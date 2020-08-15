using System;
using CommandLine;

namespace GmodNET.VersionTool
{
    class Program
    {
        [Verb("SemVer", HelpText = "Get `SemVer 2.0.0` version string without build metadata.")]
        class SemVerOptions
        {
            [Value(0, MetaName="path-to-version-file", Required=true, HelpText="Path to the *.version.json file at the root of git repository.")]
            public string VersionFilePath { get; set; }
        }

        [Verb("AssemblyVer", HelpText = "Get .NET-like assembly version string.")]
        class AssemblyVerOptions
        {
            [Value(0, MetaName="path-to-version-file", Required=true, HelpText="Path to the *.version.json file at the root of git repository.")]
            public string VersionFilePath { get; set; }
        }

        [Verb("FullVer", HelpText = "Get `SemVer 2.0.0` with additioanl metadata version string.")]
        class FileVerOptions
        {
            [Value(0, MetaName="path-to-version-file", Required=true, HelpText="Path to the *.version.json file at the root of git repository.")]
            public string VersionFilePath { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
