using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Web.Boot
{
    public static class BootConfigExtensions
    {
        private static BootConfig _bootConfig;
        private static readonly object BootConfigLock = new();

        public static BootConfig GetBootConfig(this IConfiguration configuration)
        {
            if (_bootConfig == null)
                lock (BootConfigLock)
                {
                    if (_bootConfig == null)
                    {
                        var config = new BootConfig();
                        configuration.GetSection("boot").Bind(config);
                        _bootConfig = Ensure(config);
                    }
                }

            return _bootConfig;
        }

        private static BootConfig Ensure(BootConfig bootConfig)
        {
            if (string.IsNullOrWhiteSpace(bootConfig.StartupAssembly))
                bootConfig.StartupAssembly = "Web.Boot";
            if (string.IsNullOrWhiteSpace(bootConfig.DataDir))
                bootConfig.DataDir = "../data";
            if (string.IsNullOrWhiteSpace(bootConfig.WebDir))
                bootConfig.WebDir = "../data/wwwroot";
            if (string.IsNullOrWhiteSpace(bootConfig.ConfigDir))
                bootConfig.ConfigDir = "../data/config";
            if (string.IsNullOrWhiteSpace(bootConfig.ExtensionsDir))
                bootConfig.ExtensionsDir = "../data/extensions";
            if (string.IsNullOrWhiteSpace(bootConfig.RuntimeExtensionsDir))
                bootConfig.RuntimeExtensionsDir = "./bin/extensions";

            // if scanning is completely omitted from the configuration, turn it on by default
            if (bootConfig.EnableScan)
            {
                if (string.IsNullOrWhiteSpace(bootConfig.IncludeAssembliesRegexScan))
                    bootConfig.IncludeAssembliesRegexScan = "^(.*)$";
                if (string.IsNullOrWhiteSpace(bootConfig.ExcludeAssembliesRegexScan))
                    bootConfig.ExcludeAssembliesRegexScan =
                        "^(runtime.*|Remotion.*|Oracle.*|Microsoft.*|Aws.*|Google.*|ExtCore.*|MySql.*|Newtonsoft.*|NETStandard.*|Npgsql.*|ServiceStack.*|SQLite.*|System.*|e_.*|mscorlib.*|netstandard.*|WindowsBase.*)$";
            }

            bootConfig.DataDir = Path.GetFullPath(bootConfig.DataDir).TrimEnd('/', '\\');

            Func<string, string> NormalizePath = p => Path.GetFullPath(p.Replace("~data/", $"{bootConfig.DataDir}/").Replace("~/", $"{Directory.GetCurrentDirectory().TrimEnd('/', '\\')}/"));
            bootConfig.WebDir = NormalizePath(bootConfig.WebDir);
            bootConfig.ConfigDir = NormalizePath(bootConfig.ConfigDir);
            bootConfig.ExtensionsDir = NormalizePath(bootConfig.ExtensionsDir);
            bootConfig.RuntimeExtensionsDir = NormalizePath(bootConfig.RuntimeExtensionsDir);

            Directory.CreateDirectory(bootConfig.DataDir);
            Directory.CreateDirectory(bootConfig.WebDir);
            Directory.CreateDirectory(bootConfig.ConfigDir);
            Directory.CreateDirectory(bootConfig.ExtensionsDir);
            Directory.CreateDirectory(bootConfig.RuntimeExtensionsDir);

            return bootConfig;
        }
    }
}