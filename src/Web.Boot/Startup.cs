using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Web.Boot
{
    public class Startup
    {
        // NOT thread-safe, use only at startup in a synchronous way
        private static Dictionary<Type, object> _contributingTypes;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        private static List<Assembly> _contributingAssemblies = null;
        public static List<Assembly> ContributingAssemblies => _contributingAssemblies ?? (_contributingAssemblies = _contributingTypes.Keys.Select(k => k.Assembly).Distinct().ToList());

        public void ConfigureServices(IServiceCollection services)
        {
            var bootConfig = Configuration.GetBootConfig();
            services.AddSingleton(bootConfig);

            var assembliesToScan = ScanForEligibleAssemblies(bootConfig.IncludeAssembliesRegexScan,
                bootConfig.ExcludeAssembliesRegexScan);
            _contributingAssemblies = assembliesToScan.ToList();
            _contributingTypes = assembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToDictionary(t => t, v => (object)null);

            ConfigureWithContributingTypesInternal<IConfigureServices>(contributor =>
                contributor.ConfigureServices(services, Configuration, Environment));
        }

        public void Configure(IApplicationBuilder app)
        {
            ConfigureWithContributingTypesInternal<IConfigureApp>(contributor => contributor.Configure(app));
        }

        internal static bool ContainsMatchingContributingTypes<T>()
        {
            return _contributingTypes.Keys.Any(t => typeof(T).IsAssignableFrom(t));
        }

        internal static void ConfigureWithContributingTypesInternal<T>(Action<T> action)
        {
            if (_contributingTypes == null || _contributingTypes.Count == 0)
                return;

            var contributors = new List<T>();
            foreach (var contributingType in _contributingTypes.Keys
                .Where(t => typeof(T).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null))
            {
                var obj = _contributingTypes[contributingType];
                if (obj == null)
                {
                    obj = Activator.CreateInstance(contributingType);
                    _contributingTypes[contributingType] = obj;
                }

                contributors.Add((T)obj);
            }

            foreach (var contributor in contributors.OrderBy(c => (c as IHasPriority)?.Priority ?? 0))
                action.Invoke(contributor);
        }

        private Assembly[] ScanForEligibleAssemblies(string includeAssembliesRegexScan,
            string excludeAssembliesRegexScan)
        {
            if (string.IsNullOrWhiteSpace(includeAssembliesRegexScan) ||
                string.IsNullOrWhiteSpace(excludeAssembliesRegexScan))
                return Array.Empty<Assembly>();

            bool IncludeCompileLibrary(Library _)
            {
                var excluded = Regex.IsMatch(_.Name, excludeAssembliesRegexScan, RegexOptions.IgnoreCase);
                var included = Regex.IsMatch(_.Name, includeAssembliesRegexScan, RegexOptions.IgnoreCase);
                return !excluded && included;
            }
            bool IncludeAssembly(Assembly _)
            {
                var excluded = Regex.IsMatch(_.GetName().FullName, excludeAssembliesRegexScan, RegexOptions.IgnoreCase);
                var included = Regex.IsMatch(_.GetName().FullName, includeAssembliesRegexScan, RegexOptions.IgnoreCase);
                return !excluded && included;
            }

            var assemblies = new List<Assembly>();
            PopulateRelevantAssembliesFromDependencyContext(assemblies, IncludeCompileLibrary, IncludeAssembly);
            return assemblies.ToArray();
        }

        private static void PopulateRelevantAssembliesFromDependencyContext(List<Assembly> assemblies,
            Func<Library, bool> includeCompileLibrary, Func<Assembly, bool> includeAssembly)
        {
            Console.WriteLine("Scanning compile libraries in default  DependencyContext");
            foreach (var compileLibrary in DependencyContext.Default.CompileLibraries)
                if (includeCompileLibrary(compileLibrary))
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(
                            new AssemblyName(compileLibrary.Name));
                        if (!assemblies.Any(a =>
                            string.Equals(a.FullName, assembly.FullName, StringComparison.OrdinalIgnoreCase)))
                        {
                            assemblies.Add(assembly);
                            Console.WriteLine("  - '{0}' discovered and loaded", assembly.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error loading assembly '{0}'", compileLibrary.Name);
                        Console.Error.WriteLine(ex.ToString());
                    }
                }

            Console.WriteLine("Scanning assemblies in default AssemblyLoadContext");
            foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
                if (includeAssembly(assembly))
                {
                    if (!assemblies.Contains(assembly) &&
                        !assemblies.Any(a =>
                            string.Equals(a.FullName, assembly.FullName, StringComparison.OrdinalIgnoreCase)))
                    {
                        assemblies.Add(assembly);
                        Console.WriteLine("  - '{0}' discovered and loaded", assembly.FullName);
                    }
                }
        }
    }
}