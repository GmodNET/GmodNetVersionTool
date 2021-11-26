using System.Threading.Tasks;
using Xunit;
using GmodNET.VersionTool.SourceGenerator;
using GmodNET.VersionTool.Core;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System.IO;
using System.Linq;
using VerifyCS = 
    GmodNET.VersionTool.SourceGenerator.Test.GmodNetSourceGeneratorVerifier<GmodNET.VersionTool.SourceGenerator.GmodNETVersionToolSourceGenerator>;

namespace GmodNET.VersionTool.SourceGenerator.Test
{
    public class GmodNETVersionToolSourceGeneratorUnitTest
    {
        static string SampleCode => $@"
using System;

namespace GmodNET.VersionTool.SourceGenerator.Test
{{
    internal class Class1
    {{
    }}
}}";

        static string GetExpectedGeneratorResult(string fullVersion, string version, string branch, string commitSha) =>
            $@"
using System;

namespace GmodNET.VersionTool.Info
{{
    internal static class BuildInfo
    {{
        /// <summary>
        /// Gets a full version with build metadata.
        /// </summary>
        public static string FullVersion => ""{fullVersion}"";

        /// <summary>
        /// Gets a version without build metadata.
        /// </summary>
        public static string VersionWithoutBuildData => ""{version}"";

        /// <summary>
        /// Gets build's commit git repository HEAD name in human-readable format.
        /// </summary>
        public static string BranchName => ""{branch}"";

        /// <summary>
        /// Gets build's commit hash as a hex string.
        /// </summary>
        public static string CommitHash => ""{commitSha}"";
    }}
}}";

        [Fact]
        public async Task TestGeneratedFile()
        {
            string filePath = Path.GetFullPath("Test1.version.json");

            VersionGenerator gen = new VersionGenerator(filePath);

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { SampleCode },
                    AdditionalFiles =
                    {
                        (filePath, File.ReadAllText(filePath))
                    },
                    AnalyzerConfigFiles =
                    {
                        (Path.GetFullPath("./.globalconfig"), $@"
is_global = true

[{filePath.Replace('\\', '/')}]
build_metadata.AdditionalFiles.IsVersionFile = true
"),
                    },
                    GeneratedSources =
                    {
                        (typeof(GmodNETVersionToolSourceGenerator), "GmodNET.VersionTool.Info.cs",
                            GetExpectedGeneratorResult(gen.FullVersion, gen.VersionWithoutBuildData,
                                gen.BranchName, gen.CommitHash))
                    }
                },
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task TryRiseError()
        {
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { SampleCode },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("GNVT001")
                    }
                }
            };

            await test.RunAsync();
        }
    }
}
