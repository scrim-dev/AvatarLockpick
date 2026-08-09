using System.Text.Json;
using System.Text.RegularExpressions;

var root = FindRepositoryRoot();
var previous = File.Exists(Path.Combine(root, "version.txt"))
    ? File.ReadAllText(Path.Combine(root, "version.txt")).Trim()
    : string.Empty;

var today = DateOnly.FromDateTime(DateTime.Today);
var build = NextBuild(previous, today);
var version = $"{today:yyyy.MM.dd}-{build}";
var assemblyVersion = $"{today.Year}.{today.Month}.{today.Day}.{build}";

Replace(Path.Combine(root, "AvatarLockpick", "Program.cs"),
    @"public const string AppVersion = ""[^""]+"";",
    $"public const string AppVersion = \"{version}\";");
Replace(Path.Combine(root, "AvatarLockpick.Service", "Program.cs"),
    @"public const string AppVersion = ""[^""]+"";",
    $"public const string AppVersion = \"{version}\";");

foreach (var project in new[]
{
    Path.Combine(root, "AvatarLockpick", "AvatarLockpick.csproj"),
    Path.Combine(root, "AvatarLockpick.Service", "AvatarLockpick.Service.csproj")
})
{
    Replace(project, @"<AssemblyVersion>[^<]+</AssemblyVersion>", $"<AssemblyVersion>{assemblyVersion}</AssemblyVersion>");
    Replace(project, @"<FileVersion>[^<]+</FileVersion>", $"<FileVersion>{assemblyVersion}</FileVersion>");
    UpsertInformationalVersion(project, version);
}

File.WriteAllText(Path.Combine(root, "version.txt"), version + Environment.NewLine);
File.WriteAllText(Path.Combine(root, "AvatarLockpick", "version.json"),
    JsonSerializer.Serialize(new { AppVersion = version }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

Console.WriteLine($"Version set to {version} (assembly {assemblyVersion}).");

static int NextBuild(string previous, DateOnly today)
{
    var match = Regex.Match(previous, @"^(?<year>\d{4})\.\d{1,2}\.\d{1,2}-(?<build>\d+)$");
    if (!match.Success || int.Parse(match.Groups["year"].Value) != today.Year)
        return 1;

    var value = int.Parse(match.Groups["build"].Value);
    if (value >= ushort.MaxValue)
        throw new InvalidOperationException("The yearly build counter reached 65535, which cannot be stored in a Windows assembly version.");
    return value + 1;
}

static void Replace(string path, string pattern, string replacement)
{
    var content = File.ReadAllText(path);
    var updated = Regex.Replace(content, pattern, replacement, RegexOptions.Multiline);
    if (updated == content)
        throw new InvalidOperationException($"Expected version marker was not found in {path}.");
    File.WriteAllText(path, updated);
}

static void UpsertInformationalVersion(string path, string version)
{
    var content = File.ReadAllText(path);
    if (Regex.IsMatch(content, @"<InformationalVersion>[^<]+</InformationalVersion>"))
        content = Regex.Replace(content, @"<InformationalVersion>[^<]+</InformationalVersion>", $"<InformationalVersion>{version}</InformationalVersion>");
    else
        content = content.Replace("<FileVersion>", $"<InformationalVersion>{version}</InformationalVersion>{Environment.NewLine}    <FileVersion>");
    File.WriteAllText(path, content);
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "AvatarLockpick.sln")))
            return directory.FullName;
        directory = directory.Parent;
    }
    return Directory.GetCurrentDirectory();
}
