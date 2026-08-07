namespace PlaywrightTests.Config;

public static class TestSettings
{
    public const string BaseUrl = "https://automationexercise.com";

    // Trailing slash required so relative endpoint paths (e.g. "productsList") resolve under /api/ instead of replacing it.
    public const string ApiBaseUrl = "https://automationexercise.com/api/";
}
