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

        // Get a list of product ids from the page and add the first one to the cart
        var productIds = await home.GetAllProductIdsAsync();
        await home.AddProductToCartByIdAsync(productIds.First());

        Assert.IsTrue(await home.IsAddToCartConfirmationVisibleAsync(), "Expected the 'Added to cart' confirmation modal to appear.");
    }
}
