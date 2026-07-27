using PlaywrightTests.Pages;

namespace PlaywrightTests.Tests;

[TestClass]
public sealed class HomePageTests : PlaywrightTestBase
{
    [TestMethod]
    public async Task WhenNavigatingToHomePageCanAddProductToCart()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        // Get a list of prouct ids from page and click on the first one
        var productIds = await home.GetAllProductIdsAsync();
        await home.ClickOnProductCategoryAsync(productIds.First());
        Assert.IsTrue(await home.IsCartCountUpdatedAsync(1), "Cart count should be updated to 1 after adding a product.");
    }
}