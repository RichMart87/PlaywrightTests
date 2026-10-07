namespace PlaywrightTests.Infrastructure;

public enum TraceMode
{
    Off,
    On,
    RetainOnFailure
}

// Run-time switches read from environment variables so scripts/CI can change behaviour without code edits.
// Scripts/run-tests.ps1 sets these; all have sensible defaults when unset.
public static class RunSettings
{
    // HEADED=1 shows the browser. A debugger attached always runs headed.
    public static bool Headed =>
        System.Diagnostics.Debugger.IsAttached || IsTruthy(Environment.GetEnvironmentVariable("HEADED"));

    // PW_TRACE=off|on|retain-on-failure (default). "on" keeps traces for passing tests too.
    public static TraceMode Trace =>
        (Environment.GetEnvironmentVariable("PW_TRACE") ?? "").Trim().ToLowerInvariant() switch
        {
            "off" or "0" or "false" => TraceMode.Off,
            "on" or "1" or "true" => TraceMode.On,
            _ => TraceMode.RetainOnFailure
        };

    // TEST_ARTIFACTS_DIR overrides where screenshots/traces are written.
    public static string? ArtifactsDirectory => Environment.GetEnvironmentVariable("TEST_ARTIFACTS_DIR");

    private static bool IsTruthy(string? value) =>
        value is not null && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));
}
