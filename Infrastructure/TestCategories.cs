namespace PlaywrightTests.Infrastructure;

// Suite names used with [TestCategory]. CI and Scripts/run-tests.ps1 select suites with
// --filter "TestCategory=<name>", so a new test class joins a suite just by being tagged.
// Keep in sync with the matrix in .github/workflows/playwright-tests.yml.
public static class TestCategories
{
    public const string Api = "Api";
    public const string Smoke = "Smoke";
    public const string Regression = "Regression";
}
