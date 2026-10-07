using PlaywrightTests.Infrastructure;
using PlaywrightTests.Pages;

namespace PlaywrightTests.Tests;

[TestClass]
[TestCategory(TestCategories.Regression)]
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

    [TestMethod]
    public async Task WhenNavigatingToHomePageCanAddMultipleProductsToCart()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        var productIds = await home.GetAllProductIdsAsync();
        Assert.IsGreaterThanOrEqualTo(2, productIds.Count,$"Expected at least 2 products on the home page, found {productIds.Count}.");
        var expectedIds = productIds.Take(2).ToList();

        foreach (var productId in expectedIds)
        {
            await home.AddProductToCartByIdAsync(productId);
            Assert.IsTrue(await home.IsAddToCartConfirmationVisibleAsync(), $"Expected the 'Added to cart' confirmation modal to appear for product {productId}.");
            await home.ClickOnContinueShoppingAsync();
        }

        var cart = new CartPage(Page!);
        await cart.GotoAsync();
        CollectionAssert.AreEquivalent(expectedIds, await cart.GetProductIdsAsync(), "Expected the cart to contain exactly the products that were added.");
    }

    [TestMethod]
    public async Task WhenNavigatingToHomePage_FeaturesItemsAreListed()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        Assert.AreEqual("Features Items", await home.Products.GetTitleAsync(), ignoreCase: true);
        Assert.IsGreaterThan(0, await home.Products.CountAsync(), "Expected at least one featured product.");
    }

    [TestMethod]
    [DataRow("Women", "Dress", "Women - Dress Products")]
    [DataRow("Women", "Tops", "Women - Tops Products")]
    [DataRow("Men", "Jeans", "Men - Jeans Products")]
    [DataRow("Kids", "Tops & Shirts", "Kids - Tops & Shirts Products")]
    public async Task WhenSelectingCategoryFromSidebar_ShowsCategoryProducts(string category, string subCategory, string expectedTitle)
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        await home.Sidebar.OpenSubCategoryAsync(category, subCategory);

        Assert.Contains("/category_products/", Page!.Url);
        Assert.AreEqual(expectedTitle, await home.Products.GetTitleAsync(), ignoreCase: true);
        Assert.IsGreaterThan(0, await home.Products.CountAsync(), $"Expected products under {category} > {subCategory}.");
    }

    [TestMethod]
    public async Task WhenSubscribingFromHomePage_ShowsSuccessMessage()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        await home.Subscription.SubscribeAsync(TestData.TestDataFactory.CreateUniqueEmail());

        Assert.IsTrue(await home.Subscription.IsSuccessMessageVisibleAsync(), "Expected the subscription success alert to appear.");
        Assert.Contains("You have been successfully subscribed!", await home.Subscription.GetSuccessMessageAsync());
    }

    [TestMethod]
    public async Task WhenAddingRecommendedItemToCart_ItAppearsInCart()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        var productId = await home.AddFirstVisibleRecommendedItemToCartAsync();
        Assert.IsTrue(await home.CartModal.IsVisibleAsync(), "Expected the 'Added to cart' confirmation modal to appear.");
        await home.CartModal.ViewCartAsync();

        var cart = new CartPage(Page!);
        CollectionAssert.AreEqual(new[] { productId }, await cart.GetProductIdsAsync(), "Expected the recommended item to be the only product in the cart.");
    }

    [TestMethod]
    public async Task WhenClickingScrollUpArrow_PageReturnsToTop()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();

        await home.ScrollToBottomAsync();
        Assert.IsTrue(await home.Subscription.Heading.IsVisibleAsync(), "Expected the footer subscription section after scrolling down.");

        await home.ClickScrollUpArrowAsync();

        Assert.IsTrue(await home.WaitForScrolledToTopAsync(), "Expected the page to scroll back to the top.");
        Assert.IsTrue(await home.HeroTagline.IsVisibleAsync(), "Expected the hero tagline to be visible at the top of the page.");
    }
}
