namespace Web.Boot
{
    public class BootConfig
    {
        public string StartupAssembly { get; set; }
        public string DataDir { get; set; }
        public string ConfigDir { get; set; }
        public string WebDir { get; set; }
        public string ExtensionsDir { get; set; }
        public string RuntimeExtensionsDir { get; set; }
        public bool EnableScan { get; set; } = true;
        public string IncludeAssembliesRegexScan { get; set; }
        public string ExcludeAssembliesRegexScan { get; set; }
        public bool EnableForwardedHeaders { get; set; } = true;
        public bool EnableHttpsRedirection { get; set; } = false;
        public bool RewriteSchemeToHttps { get; set; } = true;
        public bool EnableHsts { get; set; } = true;
        public bool EnableCookiePolicy { get; set; } = true;
        public bool EnableCors { get; set; } = true;
        public bool EnableControllers { get; set; } = true;
        public bool EnableRestart { get; set; } = true;
    }
}