using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Web.Boot
{
    public static class IWebHostBuilderExtensions
    {
        public static void ConfigureWebHost(this IWebHostBuilder webBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, false)
                .AddJsonFile($"appsettings.Release.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var dataDir = configuration["boot:dataDir"];
            var extensionsInstallDir = configuration["boot:extensions:installDir"];
            var extensionsActiveDir = configuration["boot:extensions:activeDir"];

            if (!string.IsNullOrWhiteSpace(dataDir))
            {
                dataDir = Path.GetFullPath(dataDir);
                Directory.CreateDirectory(dataDir);
            }

            if (!string.IsNullOrWhiteSpace(extensionsInstallDir))
            {
                extensionsInstallDir = Path.GetFullPath(extensionsInstallDir);
                Directory.CreateDirectory(extensionsInstallDir);
            }

            if (!string.IsNullOrWhiteSpace(extensionsActiveDir))
            {
                extensionsActiveDir = Path.GetFullPath(extensionsActiveDir);
                Directory.CreateDirectory(extensionsActiveDir);
            }

            // activate extensions
            CopyDirectoriesRecursive(extensionsInstallDir, extensionsActiveDir,
                clearTargetDirFirst: true,
                overwriteTargetFiles: true);

            // load extensions
            AssemblyLoader.LoadExtensions(extensionsActiveDir);

            var startupAssembly = configuration["boot:startupAssembly"];

            var assembly = LocateStartupAssembly(startupAssembly, dataDir);

            if (assembly != null)
            {
                Console.WriteLine($"Booting from {assembly.FullName}");

                webBuilder.UseSetting(
                    WebHostDefaults.StartupAssemblyKey,
                    assembly.FullName);

                webBuilder.UseSetting(
                    WebHostDefaults.HostingStartupAssembliesKey,
                    assembly.FullName);
            }
            else
            {
                webBuilder.UseStartup<Startup>();
            }
        }

        private static Assembly LocateStartupAssembly(string startupAssembly, string dataDir)
        {
            if (string.IsNullOrWhiteSpace(startupAssembly))
                return null;

            // is startupAssembly an assembly name
            var filePath = Path.GetExtension(startupAssembly)
                .Equals(".dll", StringComparison.InvariantCultureIgnoreCase) ? startupAssembly : $"{startupAssembly}.dll";
            var fileName = Path.GetFileName(filePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(fileNameWithoutExtension));
            }
            catch
            {
                // ignored
            }

            if (!File.Exists(filePath))
                filePath = Directory.GetFiles(dataDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (File.Exists(filePath))
            {
                var fileDir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(fileDir))
                {
                    Assembly assembly = null;
                    foreach (var d in Directory.GetFiles(fileDir, "*.dll", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var da = AssemblyLoadContext.Default.LoadFromAssemblyPath(d);
                            if (fileName.Equals(Path.GetFileName(d), StringComparison.OrdinalIgnoreCase))
                            {
                                assembly = da;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    return assembly;
                }
            }

            return null;
        }

        private static void CopyDirectoriesRecursive(string sourceDir, string targetDir,
            Func<DirectoryInfo, bool> excludeSourceDir = null, Func<FileInfo, bool> excludeSourceFile = null,
            bool clearTargetDirFirst = false, bool overwriteTargetFiles = false)
        {
            if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(targetDir)) return;
            if (!Directory.Exists(sourceDir) || !Directory.Exists(targetDir)) return;
            if (Path.GetFullPath(sourceDir)
                .Equals(Path.GetFullPath(targetDir), StringComparison.InvariantCultureIgnoreCase)) return;

            if (clearTargetDirFirst)
            {
                DeleteDirectoryContentsSafe(targetDir);
            }

            // Copy directories
            var sourceDirInfo = new DirectoryInfo(sourceDir);
            foreach (var dir in sourceDirInfo.GetDirectories("*", SearchOption.AllDirectories))
            {
                if (excludeSourceDir != null && excludeSourceDir(dir))
                    continue;

                var relativePath = Path.GetRelativePath(sourceDir, dir.FullName);
                Directory.CreateDirectory(Path.Combine(targetDir, relativePath));
            }

            // Copy files
            foreach (var file in sourceDirInfo.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (excludeSourceFile != null && excludeSourceFile(file))
                    continue;

                var relativePath = Path.GetRelativePath(sourceDir, file.FullName);
                var destinationPath = Path.Combine(targetDir, relativePath);

                if (overwriteTargetFiles)
                {
                    if (File.Exists(destinationPath))
                        continue;
                }

                File.Copy(file.FullName, destinationPath, overwriteTargetFiles);
            }
        }

        private static void DeleteDirectoryContentsSafe(string targetDir)
        {
            foreach (var dir in Directory.GetDirectories(targetDir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    // ignored
                }
            }

            foreach (var file in Directory.GetFiles(targetDir, "*.*",
                SearchOption.AllDirectories))
            {
                DeleteFileSafe(file);
            }

            foreach (var dir in Directory.GetDirectories(targetDir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static void DeleteFileSafe(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                try
                {
                    Thread.Sleep(100);
                    File.Delete(file);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
