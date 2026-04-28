using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlaywrightTests;

public abstract class PlaywrightTestBase
{
    private TestContext? testContext;

    // MSTest will set this automatically (can live in base class)
    public TestContext? GetTestContext()
    {
        return testContext;
    }

    public void SetTestContext(TestContext? value)
    {
        testContext = value;
    }

    // Add the property MSTest expects so it can populate the TestContext
    public TestContext? TestContext
    {
        get => testContext;
        set => testContext = value;
    }

    protected IPlaywright? Playwright;
    protected IBrowser? Browser;
    protected IBrowserContext? Context;
    protected IPage? Page;

    [TestInitialize]
    public async Task SetupAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    // Helper to run a test body with centralized logging of exceptions + stack trace + optional screenshot
    protected async Task RunTestAsync(Func<Task> testBody)
    {
        var context = GetTestContext();
        if (context != null)
        {
            context.WriteLine($"Starting test: {context.TestName} at {DateTime.UtcNow:O}");
        }

        try
        {
            await testBody();

            if (context != null)
            {
                context.WriteLine($"Outcome: Passed for {context.TestName}");
            }
        }
        catch (Exception ex)
        {
            if (context != null)
            {
                context.WriteLine($"Outcome: Failed for {context.TestName}");
                context.WriteLine($"Exception: {ex.GetType().FullName}: {ex.Message}");
                context.WriteLine("StackTrace:");
                context.WriteLine(ex.StackTrace ?? "<no stacktrace>");
            }

            try
            {
                if (Page != null && context != null)
                {
                    var fileName = $"{context.TestName}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                    var path = Path.Combine(Path.GetTempPath(), fileName);
                    await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
                    context.AddResultFile(path);
                    context.WriteLine($"Screenshot saved: {path}");
                }
            }
            catch (Exception screenshotEx)
            {
                if (context != null)
                {
                    context.WriteLine($"Screenshot capture failed: {screenshotEx}");
                }
            }

            throw;
        }
    }

    [TestCleanup]
    public async Task TeardownAsync()
    {
        // Use a local variable so the null check is effective and the framework-assigned context is used
        var context = GetTestContext();
        if (context != null)
        {
            context.WriteLine($"Test '{context.TestName}' finished with outcome: {context.CurrentTestOutcome}");
        }

        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
    }
}