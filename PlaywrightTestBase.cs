using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

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

    private static readonly System.Text.RegularExpressions.Regex AdHosts = new(
        @"googlesyndication\.com|doubleclick\.net|googleadservices\.com|adservice\.google\.|fundingchoicesmessages\.google\.com|googletagservices\.com|googletagmanager\.com|google-analytics\.com");

    [TestInitialize]
    public async Task SetupAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !RunSettings.Headed,
            SlowMo = System.Diagnostics.Debugger.IsAttached ? 100 : null
        });

        Context = await Browser.NewContextAsync();

        // automationexercise.com serves Google ads whose full-page "vignette" interstitials hijack
        // clicks and navigations at random. Blocking ad hosts removes that flakiness (and speeds pages up).
        await Context.RouteAsync(AdHosts, route => route.AbortAsync());

        if (RunSettings.Trace != TraceMode.Off)
        {
            await Context.Tracing.StartAsync(new TracingStartOptions
            {
                Title = GetTestContext()?.TestName,
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        Page = await Context.NewPageAsync();
    }

    // Helper to run a test body with centralized logging of exceptions + stack trace.
    // Screenshots and traces on failure are captured in TeardownAsync for every test, whether or not it uses this helper.
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

            throw;
        }
    }

    [TestCleanup]
    public async Task TeardownAsync()
    {
        // Use local variable so the null check is effective and the framework-assigned context is used
        var context = GetTestContext();
        var testName = context?.TestName ?? GetType().Name;
        var failed = context != null && context.CurrentTestOutcome is not (UnitTestOutcome.Passed or UnitTestOutcome.Inconclusive);

        if (context != null)
        {
            context.WriteLine($"Test '{testName}' finished with outcome: {context.CurrentTestOutcome}");
        }

        if (failed)
        {
            await CaptureScreenshotAsync(context, testName);
        }

        await StopTracingAsync(context, testName, keep: RunSettings.Trace == TraceMode.On || failed);

        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
    }

    private async Task CaptureScreenshotAsync(TestContext? context, string testName)
    {
        if (Page == null || Page.IsClosed)
        {
            return;
        }

        try
        {
            var path = ArtifactPaths.For(testName, "png");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
            context?.AddResultFile(path);
            context?.WriteLine($"Screenshot saved: {path}");
        }
        catch (Exception ex)
        {
            context?.WriteLine($"Screenshot capture failed: {ex}");
        }
    }

    private async Task StopTracingAsync(TestContext? context, string testName, bool keep)
    {
        if (Context == null || RunSettings.Trace == TraceMode.Off)
        {
            return;
        }

        try
        {
            if (!keep)
            {
                // Stopping without a path discards the recording.
                await Context.Tracing.StopAsync();
                return;
            }

            var path = ArtifactPaths.For(testName, "zip");
            await Context.Tracing.StopAsync(new TracingStopOptions { Path = path });
            context?.AddResultFile(path);
            context?.WriteLine($"Trace saved: {path} (open with Scripts/show-trace.ps1 or https://trace.playwright.dev)");
        }
        catch (Exception ex)
        {
            context?.WriteLine($"Trace capture failed: {ex}");
        }
    }
}
