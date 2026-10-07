using PlaywrightTests.Infrastructure;
using PlaywrightTests.Pages;

namespace PlaywrightTests.Tests;

[TestClass]
[TestCategory(TestCategories.Regression)]
public sealed class ProductsPageTests : PlaywrightTestBase
{
    [TestMethod]
    public async Task WhenNavigatingFromHeader_AllProductsAreListed()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();
        await home.Header.ClickProductsAsync();

        var products = new ProductsPage(Page!);
        Assert.EndsWith("/products", products.Url);
        Assert.AreEqual("All Products", await products.Products.GetTitleAsync(), ignoreCase: true);
        Assert.IsTrue(await products.IsSearchBoxVisibleAsync(), "Expected the product search box to be visible.");
        Assert.IsGreaterThan(0, await products.Products.CountAsync(), "Expected at least one product.");
    }

    [TestMethod]
    public async Task WhenViewingProduct_DetailsMatchListingAndAreComplete()
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();

        var productId = (await products.Products.GetProductIdsAsync()).First();
        var listedName = await products.Products.GetProductNameAsync(productId);
        var listedPrice = await products.Products.GetProductPriceAsync(productId);

        await products.Products.ViewProductAsync(productId);

        var details = new ProductDetailsPage(Page!);
        Assert.EndsWith($"/product_details/{productId}", details.Url);
        Assert.AreEqual(listedName, await details.GetNameAsync(), "Product name should match the listing.");
        Assert.AreEqual(listedPrice, await details.GetPriceAsync(), "Product price should match the listing.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(await details.GetCategoryAsync()), "Expected a category.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(await details.GetAvailabilityAsync()), "Expected an availability value.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(await details.GetConditionAsync()), "Expected a condition value.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(await details.GetBrandAsync()), "Expected a brand.");
    }

    // Search matches product name OR category (e.g. "top" also returns shirts in the Tops category),
    // so the "every result contains the term" assertion uses a term that only matches by name.
    [TestMethod]
    [DataRow("jeans")]
    [DataRow("JEANS")]
    public async Task WhenSearchingByName_OnlyMatchingProductsAreShown(string term)
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();

        await products.SearchAsync(term);

        Assert.AreEqual("Searched Products", await products.Products.GetTitleAsync(), ignoreCase: true);
        var names = await products.Products.GetProductNamesAsync();
        Assert.IsGreaterThan(0, names.Count, $"Expected search results for '{term}'.");
        foreach (var name in names)
        {
            Assert.Contains(term.ToLowerInvariant(), name.ToLowerInvariant(), $"Search result '{name}' does not match '{term}'.");
        }
    }

    [TestMethod]
    public async Task WhenSearchingForUnknownProduct_NoProductsAreShown()
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();

        await products.SearchAsync($"no-such-product-{Guid.NewGuid():N}");

        Assert.AreEqual("Searched Products", await products.Products.GetTitleAsync(), ignoreCase: true);
        Assert.AreEqual(0, await products.Products.CountAsync(), "Expected no results for a nonsense search term.");
    }

    [TestMethod]
    public async Task WhenAddingSearchResultsToCart_AllResultsAreInCart()
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();
        await products.SearchAsync("jeans");

        var resultIds = await products.Products.GetProductIdsAsync();
        Assert.IsGreaterThan(0, resultIds.Count, "Expected search results to add to the cart.");

        foreach (var productId in resultIds)
        {
            await products.Products.AddToCartAsync(productId);
            Assert.IsTrue(await products.CartModal.IsVisibleAsync(), $"Expected the 'Added to cart' modal for product {productId}.");
            await products.CartModal.ContinueShoppingAsync();
        }

        var cart = new CartPage(Page!);
        await cart.GotoAsync();
        CollectionAssert.AreEquivalent(resultIds, await cart.GetProductIdsAsync(), "Expected the cart to contain exactly the searched products.");
    }

    [TestMethod]
    [DataRow("Polo")]
    [DataRow("H&M")]
    [DataRow("Biba")]
    public async Task WhenSelectingBrand_ShowsThatBrandsProducts(string brand)
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();

        // The sidebar badge "(n)" is the site's own claim of how many products the brand has
        var expectedCount = await products.Sidebar.GetBrandProductCountAsync(brand);
        await products.Sidebar.OpenBrandAsync(brand);

        Assert.AreEqual($"Brand - {brand} Products", await products.Products.GetTitleAsync(), ignoreCase: true);
        Assert.AreEqual(expectedCount, await products.Products.CountAsync(), $"Product count for {brand} should match the sidebar badge.");
    }

    [TestMethod]
    public async Task WhenSwitchingBetweenBrands_ListingUpdates()
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();

        await products.Sidebar.OpenBrandAsync("Polo");
        var poloIds = await products.Products.GetProductIdsAsync();

        await products.Sidebar.OpenBrandAsync("Madame");
        Assert.AreEqual("Brand - Madame Products", await products.Products.GetTitleAsync(), ignoreCase: true);

        var madameIds = await products.Products.GetProductIdsAsync();
        Assert.IsGreaterThan(0, madameIds.Count, "Expected Madame products.");
        Assert.IsFalse(madameIds.Intersect(poloIds).Any(), "Brand listings should not share products.");
    }

    [TestMethod]
    public async Task WhenSubmittingProductReview_ThankYouMessageIsShown()
    {
        var details = new ProductDetailsPage(Page!);
        await details.GotoAsync("1");

        await details.SubmitReviewAsync("QA Automation", TestData.TestDataFactory.CreateUniqueEmail(), "Great fit and fast delivery.");

        Assert.IsTrue(await details.IsReviewSuccessVisibleAsync(), "Expected the 'Thank you for your review.' message.");
    }
}
