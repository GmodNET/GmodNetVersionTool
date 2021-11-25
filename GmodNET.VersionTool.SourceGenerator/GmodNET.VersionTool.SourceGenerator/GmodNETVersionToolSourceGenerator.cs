using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using GmodNET.VersionTool.Core;

namespace GmodNET.VersionTool.SourceGenerator
{
    [Generator]
    public class GmodNETVersionToolSourceGenerator : ISourceGenerator
    {
        static readonly DiagnosticDescriptor NoVersionFile =
            new DiagnosticDescriptor(
                id: "GNVT001",
                title: "There is no version file specified",
                messageFormat: "There is no version file specified for a project. Add <VersionFile> item to your project's ItemGroup.",
                category: "GmodNET.VersionTool.SourceGenerator",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );

        static readonly DiagnosticDescriptor MultipleVersionFiles =
            new DiagnosticDescriptor(
                id: "GNVT002",
                title: "There are multiple version files specified",
                messageFormat: "There are multiple version files specified for a project. Only single version file can be specified.",
                category: "GmodNET.VersionTool.SourceGenerator",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );

        static DiagnosticDescriptor ReportException(Exception e) => new DiagnosticDescriptor(
            id: "GNVT003",
            title: "Exception was thrown by GmodNET.VersionTool.SourceGenerator",
            messageFormat: $"Exception was thrown by GmodNET.VersionTool.SourceGenerator: {e}",
            category: "GmodNET.VersionTool.SourceGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        static IEnumerable<AdditionalText> GetVersionFiles(GeneratorExecutionContext context) =>
            context
                .AdditionalFiles
                .Where(f =>
                    context.AnalyzerConfigOptions.GetOptions(f).TryGetValue(
                        "build_metadata.AdditionalFiles.IsVersionFile",
                        out string isVersionFile) && isVersionFile == "true");

        public void Execute(GeneratorExecutionContext context)
        {
            var versionFiles = GetVersionFiles(context);

            if (!versionFiles.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(NoVersionFile, Location.None));
                return;
            }

            if (versionFiles.Count() > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(MultipleVersionFiles, Location.None));
                return;
            }

            try
            {
                VersionGenerator gen = new VersionGenerator(versionFiles.First().Path);

                context.AddSource("GmodNET.VersionTool.Info.cs", $@"
using System;

namespace GmodNET.VersionTool.Info
{{
    internal static class BuildInfo
    {{
        /// <summary>
        /// Gets a full version with build metadata.
        /// </summary>
        public static string FullVersion => ""{gen.FullVersion}"";

        /// <summary>
        /// Gets a version without build metadata.
        /// </summary>
        public static string VersionWithoutBuildData => ""{gen.VersionWithoutBuildData}"";

        /// <summary>
        /// Gets build's commit git repository HEAD name in human-readable format.
        /// </summary>
        public static string BranchName => ""{gen.BranchName}"";

        /// <summary>
        /// Gets build's commit hash as a hex string.
        /// </summary>
        public static string CommitHash => ""{gen.CommitHash}"";
    }}
}}");
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(ReportException(e), Location.None));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }
    }
}
