using PlaywrightTests.Infrastructure;
using PlaywrightTests.Pages;

namespace PlaywrightTests.Tests;

[TestClass]
[TestCategory(TestCategories.Smoke)]
public sealed class SmokeTests : PlaywrightTestBase
{
    [TestMethod]
    public void WhenNavigatingToHomePage_TitleContainsExpectedText()
    {
        var home = new HomePage(Page!);
        home.GotoAsync().GetAwaiter().GetResult();

        // Assert that title contains expected text
        var title = home.TitleAsync().GetAwaiter().GetResult();

        Assert.IsTrue(title.Contains("Automation Exercise", System.StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void WhenNavigatingToHomePage_LogoIsVisible()
    {
        var home = new HomePage(Page!);
        home.GotoAsync().GetAwaiter().GetResult();

        // Assert that the logo is visible
        var isLogoVisible = home.IsLogoVisibleAsync().GetAwaiter().GetResult();
        Assert.IsTrue(isLogoVisible, "Expected the site logo to be visible, but it was not.");
    }

    [TestMethod]
    public void WhenNavigatingToHomePage_HeaderLinksAreVisible()
    {
        var home = new HomePage(Page!);
        home.GotoAsync().GetAwaiter().GetResult();

        // Assert the header links are visible
        Assert.IsTrue(home.Header.IsContactUsVisibleAsync().GetAwaiter().GetResult(), "Expected 'Contact Us' link to be visible.");
        Assert.IsTrue(home.Header.IsProductsVisibleAsync().GetAwaiter().GetResult(), "Expected 'Products' link to be visible.");
        Assert.IsTrue(home.Header.IsCartVisibleAsync().GetAwaiter().GetResult(), "Expected 'Cart' link to be visible.");
        Assert.IsTrue(home.Header.IsSignupLoginVisibleAsync().GetAwaiter().GetResult(), "Expected 'Signup/Login' link to be visible.");
    }

    // landmarkText is a heading (or breadcrumb) unique to the destination page, confirming the right page rendered.
    [TestMethod]
    [DataRow("/products", "All Products", DisplayName = "Header link navigates to Products")]
    [DataRow("/view_cart", "Shopping Cart", DisplayName = "Header link navigates to Cart")]
    [DataRow("/login", "Login to your account", DisplayName = "Header link navigates to Signup / Login")]
    [DataRow("/test_cases", "Test Cases", DisplayName = "Header link navigates to Test Cases")]
    [DataRow("/api_list", "APIs List for practice", DisplayName = "Header link navigates to API Testing")]
    [DataRow("/contact_us", "Get In Touch", DisplayName = "Header link navigates to Contact us")]
    public async Task WhenClickingHeaderLinkFromHomePage_ExpectedPageLoads(string path, string landmarkText)
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        var response = await home.Header.NavigateAsync(path);

        Assert.AreEqual(200, response.Status, $"Expected HTTP 200 for {path}.");
        Assert.AreEqual($"{Config.TestSettings.BaseUrl}{path}", Page!.Url);
        Assert.IsTrue(
            await Page.Locator("h2, .breadcrumb .active", new() { HasText = landmarkText }).First.IsVisibleAsync(),
            $"Expected '{landmarkText}' to be visible on {path}.");
    }

    // External link: check the target rather than navigating, so the smoke suite doesn't depend on YouTube.
    [TestMethod]
    public async Task WhenNavigatingToHomePage_VideoTutorialsLinkPointsToYouTubeChannel()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        Assert.AreEqual("https://www.youtube.com/c/AutomationExercise", await home.Header.VideoTutorialsLink.GetAttributeAsync("href"));
    }
}