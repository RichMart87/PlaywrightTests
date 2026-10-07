namespace PlaywrightTests.Infrastructure;

// One predictable location for per-test output (screenshots, traces) instead of the system temp folder.
// Layout: <root>/<TestName>/<TestName>_<timestamp>.<ext>
public static class ArtifactPaths
{
    public static string Root =>
        RunSettings.ArtifactsDirectory is { Length: > 0 } dir
            ? Path.GetFullPath(dir)
            : Path.Combine(AppContext.BaseDirectory, "artifacts");

    public static string For(string testName, string extension)
    {
        var safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
        var folder = Path.Combine(Root, safeName);
        Directory.CreateDirectory(folder);

        return Path.Combine(folder, $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.{extension.TrimStart('.')}");
    }
}
