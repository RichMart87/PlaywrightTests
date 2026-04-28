using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlaywrightTests;

[TestClass]
public sealed class Test1 : PlaywrightTestBase
{
    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmTitle()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");

        // Basic assertion: title contains expected text
        var title = await Page.TitleAsync();
        Assert.IsTrue(title.Contains("Automation Exercise", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmLogoVisible()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the logo is visible on the page
        var logo = Page.Locator("img[alt='Website for automation practice']");
        Assert.IsTrue(await logo.IsVisibleAsync(), "Expected the site logo to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmContactUsLink()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the "Contact Us" link is visible and has the correct href
        var contactUsLink = Page.Locator("a[href='/contact_us']");
        Assert.IsTrue(await contactUsLink.IsVisibleAsync(), "Expected the 'Contact Us' link to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmProductsLink()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the "Products" link is visible and has the correct href
        var productsLink = Page.Locator("a[href='/products']");
        Assert.IsTrue(await productsLink.IsVisibleAsync(), "Expected the 'Products' link to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmCartLink()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the "Cart" link is visible and has the correct href
        var cartLink = Page.Locator("//*[@id='header']//ul/li[3]/a");
        Assert.IsTrue(await cartLink.IsVisibleAsync(), "Expected the 'Cart' link to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmSignupLoginLink()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the "Signup / Login" link is visible and has the correct href
        var signupLoginLink = Page.Locator("//*[@id='header']//ul/li[4]/a");
        Assert.IsTrue(await signupLoginLink.IsVisibleAsync(), "Expected the 'Signup / Login' link to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmTestCasesLink()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the "Test Cases" link is visible and has the correct href
        var testCasesLink = Page.Locator("//*[@id='header']//ul/li[5]/a");
        Assert.IsTrue(await testCasesLink.IsVisibleAsync(), "Expected the 'Test Cases' link to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmAPITestingLink()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Assert that the "API Testing" link is visible and has the correct href
        var apiTestingLink = Page.Locator("//*[@id='header']//ul/li[6]/a");
        Assert.IsTrue(await apiTestingLink.IsVisibleAsync(), "Expected the 'API Testing' link to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmContactUsPage()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Click on the "Contact Us" link
        await Page.ClickAsync("a[href='/contact_us']");
        // Assert that the URL is correct and the contact form is visible
        Assert.IsTrue(Page.Url.Contains("/contact_us"), "Expected to navigate to the Contact Us page, but did not.");
        var contactForm = Page.Locator("form[action='/contact_us']");
        Assert.IsTrue(await contactForm.IsVisibleAsync(), "Expected the contact form to be visible, but it was not.");
    }

    [TestMethod]
    public async Task WhenNavigatingToSiteConfirmProductsPage()
    {
        // Navigate to target site
        await Page!.GotoAsync("https://automationexercise.com");
        // Click on the "Products" link
        await Page.ClickAsync("a[href='/products']");
        // Assert that the URL is correct and the products list is visible
        Assert.IsTrue(Page.Url.Contains("/products"), "Expected to navigate to the Products page, but did not.");
        var productsList = Page.Locator(".features_items");
        Assert.IsTrue(await productsList.IsVisibleAsync(), "Expected the products list to be visible, but it was not.");
    }
}