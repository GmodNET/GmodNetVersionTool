using System;
using Microsoft.Build.Framework;
using GmodNET.VersionTool.Core;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
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

                [DllImport("__Internal")]
                static extern unsafe void mono_dllmap_insert(IntPtr assembly, byte* dll, IntPtr func, byte* tdll, IntPtr tfunc);

                string assembly_directory = new DirectoryInfo(Path.GetDirectoryName(this.GetType().Assembly.Location)).Parent.FullName;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                { 
                    if(IntPtr.Size == 4)
                    {
                        SetDllDirectoryW(Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "win-x86", "native"));
                    }
                    else if(IntPtr.Size == 8)
                    {
                        SetDllDirectoryW(Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "win-x64", "native"));
                    }
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Type.GetType("Mono.Runtime") != null)
                {
                    string git_lib_name = "git2-6777db8";
                    string git_lib_path = Path.Combine(assembly_directory, "netcoreapp3.1", "runtimes", "osx", "native", "libgit2-6777db8.dylib");

                    byte[] git_lib_name_bytes = Encoding.UTF8.GetBytes(git_lib_name);
                    byte[] git_lib_path_bytes = Encoding.UTF8.GetBytes(git_lib_path);

                    unsafe
                    {
                        fixed(byte* git_lib_name_ptr = &git_lib_name_bytes[0])
                        {
                            fixed(byte* git_lib_path_ptr = &git_lib_path_bytes[0])
                            {
                                mono_dllmap_insert(IntPtr.Zero, git_lib_name_ptr, IntPtr.Zero, git_lib_path_ptr, IntPtr.Zero);
                            }
                        }
                    }
                }

                ver_pair = new InnerVersionGenerator().Generate(VersionFiles[0]);
#elif NETCOREAPP3_1
                CustomLoadContext load_context = new CustomLoadContext(this, Log);

                Assembly innerAssembly = load_context.LoadFromAssemblyPath(this.GetType().Assembly.Location);

                dynamic inner_generator = Activator.CreateInstance(innerAssembly.GetTypes().First(type => type.FullName == typeof(InnerVersionGenerator).FullName));

                ver_pair = inner_generator.Generate(VersionFiles[0]);

                load_context.Unload();
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

            public CustomLoadContext(VersionToolGenerate generator, Microsoft.Build.Utilities.TaskLoggingHelper logger) : base(isCollectible: true)
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
