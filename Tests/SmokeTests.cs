using PlaywrightTests;
using PlaywrightTests.Pages;

namespace PlaywrightTests.Tests;

[TestClass]
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
}