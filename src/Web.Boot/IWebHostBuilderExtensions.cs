using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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
            if (!string.IsNullOrWhiteSpace(dataDir))
            {
                dataDir = Path.GetFullPath(dataDir);
                Directory.CreateDirectory(dataDir);
            }
            var startupAssembly = configuration["boot:startupAssembly"];

            var assembly = LocateAssembly(startupAssembly, dataDir);

            if (assembly != null)
            {
                Console.WriteLine($"Booting with {assembly.FullName}");

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

        private static Assembly LocateAssembly(string startupAssembly, string dataDir)
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
    }
}
