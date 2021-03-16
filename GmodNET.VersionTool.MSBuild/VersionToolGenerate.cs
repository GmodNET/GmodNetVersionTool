using System;
using Microsoft.Build.Framework;
using GmodNET.VersionTool.Core;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Linq;
#if NETCOREAPP3_1
using System.Runtime.Loader;
#endif

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

                Tuple<string, string> ver_pair;

#if NET472
                [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
                static extern bool SetDllDirectoryW(string folder);

                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string assembly_directory = new DirectoryInfo(Path.GetDirectoryName(this.GetType().Assembly.Location)).Parent.FullName;

                    if(IntPtr.Size == 4)
                    {
                        Log.LogWarning($"Add path for dlls: {Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "win-x86", "native")}");
                        SetDllDirectoryW(Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "win-x86", "native"));
                    }
                    else if(IntPtr.Size == 8)
                    {
                        Log.LogWarning($"Add path for dlls: {Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "win-x64", "native")}");
                        SetDllDirectoryW(Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "win-x64", "native"));
                    }
                }

                ver_pair = new InnerVersionGenerator().Generate(VersionFiles[0]);
#elif NETCOREAPP3_1
                Assembly innerAssembly = new CustomLoadContext(this, Log).LoadFromAssemblyPath(this.GetType().Assembly.Location);

                dynamic inner_generator = Activator.CreateInstance(innerAssembly.GetTypes().First(type => type.FullName == typeof(InnerVersionGenerator).FullName));

                ver_pair = inner_generator.Generate(VersionFiles[0]);
#endif

                FullVersion = ver_pair.Item1;
                ShortVersion = ver_pair.Item2;

                return true;    
            }
            catch(Exception e)
            {
                Log.LogError("(GmodNET.VersionTool.MSBuild) Exception was thrown while executing VersionToolGenerate task: " + e.ToString());
                return false;
            }
        }

#if NETCOREAPP3_1
        class CustomLoadContext : AssemblyLoadContext
        {
            VersionToolGenerate generator;
            AssemblyDependencyResolver resolver;
            Microsoft.Build.Utilities.TaskLoggingHelper logger;

            public CustomLoadContext(VersionToolGenerate generator, Microsoft.Build.Utilities.TaskLoggingHelper logger) : base()
            {
                this.generator = generator;
                resolver = new AssemblyDependencyResolver(generator.GetType().Assembly.Location);
                this.logger = logger;
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                string assembly_path = resolver.ResolveAssemblyToPath(assemblyName);

                try
                {
                    return this.LoadFromAssemblyPath(assembly_path);
                }
                catch
                {
                    return null;
                }
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string lib_path = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

                try
                {
                    return NativeLibrary.Load(lib_path);
                }
                catch
                {
                    return IntPtr.Zero;
                }
            }
        }
#endif
    }

    public class InnerVersionGenerator
    {
        public Tuple<string, string> Generate(string version_file_path)
        {
            VersionGenerator gen = new VersionGenerator(version_file_path);

            return new Tuple<string, string>(gen.FullVersion, gen.VersionWithoutBuildData);
        }
    }
}
