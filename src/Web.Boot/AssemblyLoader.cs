using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

namespace Web.Boot
{
    internal static class AssemblyLoader
    {
        public static void LoadExtensions(string path)
        {
            Load(path, AssemblyLoadContext.Default.Assemblies
                .Where(a => a.GetName().Name != null)
                .Select(a => a.GetName().Name?.ToLowerInvariant()).ToList());
        }

        private static void Load(string path, List<string> previouslyLoadedAssemblyNames)
        {
            foreach (var dll in Directory.EnumerateFiles(path, "*.dll"))
            {
                var fileName = Path.GetFileNameWithoutExtension(dll);
                var fileNameLowerCase = fileName.ToLowerInvariant();

                // don't load the same assembly twice
                if (previouslyLoadedAssemblyNames.Contains(fileNameLowerCase))
                    continue;
                
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                    var assemblyNameLowerCase = assembly.GetName().Name?.ToLowerInvariant();

                    previouslyLoadedAssemblyNames.Add(fileNameLowerCase);

                    if (assemblyNameLowerCase != null &&
                        assemblyNameLowerCase != fileNameLowerCase &&
                        !previouslyLoadedAssemblyNames.Contains(assemblyNameLowerCase))
                        previouslyLoadedAssemblyNames.Add(assemblyNameLowerCase);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Unable to load assembly {dll}: {ex.Message}", ex);
                }
            }

            foreach (var subPath in Directory.GetDirectories(path))
                Load(subPath, previouslyLoadedAssemblyNames);
        }
    }
}