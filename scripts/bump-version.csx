#!/usr/bin/env dotnet-script

var dir = Directory.GetCurrentDirectory();

// See: https://github.com/filipw/dotnet-script
var args = Args.ToArray();
var showHelp = (args == null || args.Length == 0 || args.Contains("-h") || args.Contains("--help"));

var forceVersion = (string) null;
var type = args.Contains("--major") ? "major": (
    args.Contains("--minor") ? "minor": (
        args.Contains("--build") ? "build": null
    )
);
if (type == null && args.Contains("--force") && args.Length > (Array.IndexOf(args, "--force") + 1))
{
    type = "force";
    forceVersion = args[Array.IndexOf(args, "--force") + 1];
}
else if (type == null)
{
    showHelp = true;
}

if (showHelp)
{
    Console.WriteLine(@"
    Call this script as follows:
    
      -> ./bump-version.csx -- --help
      -> ./bump-version.csx -- --minor
      -> ./bump-version.csx -- --major
      -> ./bump-version.csx -- --build
      -> ./bump-version.csx -- --force 2.3.45
    ");
    return;
}

var firstVersion = forceVersion != null ? new Version(forceVersion): new Version(0,0,1);
var lastVersion = firstVersion;

var csProjFiles = Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories);
foreach(var csProjFile in csProjFiles)
{
    var csProjFileName = Path.GetFileName(csProjFile);
    var csProjContents = File.ReadAllText(csProjFile);
    if (!csProjContents.Contains("<version>", StringComparison.OrdinalIgnoreCase))
    {
        // add version 0.0.1
        var idx0 = csProjContents.IndexOf("</PropertyGroup>", StringComparison.OrdinalIgnoreCase);
        if (idx0 > -1)
        {
            csProjContents = csProjContents.Substring(0, idx0) + $"<Version>{firstVersion}</Version>\n  " + csProjContents.Substring(idx0);
            File.WriteAllText(csProjFile, csProjContents);
            Console.WriteLine($"Updated project {csProjFileName} to version {firstVersion}.");
        }
        else
        {
            Console.WriteLine($"WARN!! Unable to locate a version placement in project {csProjFileName}.");
        }
        continue;
    }

    var idx1 = csProjContents.IndexOf("<version>", StringComparison.OrdinalIgnoreCase);
    var idx2 = csProjContents.IndexOf("</version>", StringComparison.OrdinalIgnoreCase);
    if (idx1 > 0 && idx2 > idx1)
    {
        var version = new Version(csProjContents.Substring(idx1 + 9, idx2 - idx1 - 9));
        var newVersion = forceVersion != null ? new Version(forceVersion): 
            new Version(
                type == "major" ? version.Major + 1: version.Major, 
                type == "minor" ? version.Minor + 1: version.Minor, 
                type == "build" ? version.Build + 1: version.Build);
        csProjContents = csProjContents.Substring(0, idx1 + 9) + newVersion + csProjContents.Substring(idx2);
        File.WriteAllText(csProjFile, csProjContents);
        Console.WriteLine($"Updated project {csProjFileName} to version {newVersion}.");
        lastVersion = newVersion;
    }
}

var shScript = Path.Combine(dir, "scripts", "some_other_script.sh");
if (File.Exists(shScript))
{	
	var shFileLines = File.ReadAllLines(shScript);
	var appVersionLineIdx = Array.FindIndex(shFileLines, l => l.Trim().StartsWith("APP_VERSION="));
	shFileLines[appVersionLineIdx]=$"APP_VERSION={lastVersion}";
	File.WriteAllLines(shScript, shFileLines);
}
