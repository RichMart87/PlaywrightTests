using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// The product card grid (div.features_items) shared by the home page, /products, search results,
// and the category / brand listing pages.
public sealed class ProductGridComponent
{
    private readonly IPage page;

    public ProductGridComponent(IPage page) => this.page = page;

    public ILocator Root => page.Locator("div.features_items");

    // e.g. "Features Items", "All Products", "Searched Products", "Brand - Polo Products".
    // The heading is upper-cased with CSS, so read it with TextContentAsync (raw DOM text) rather than InnerTextAsync.
    public ILocator Title => Root.Locator("h2.title");

    private ILocator _productItems => Root.Locator(".product-image-wrapper");

    // data-product-id lives on the "Add to cart" link inside .productinfo, and the same link is repeated
    // inside .product-overlay - scope to .productinfo so each card resolves to exactly one element.
    private ILocator ProductCard(string productId) =>
        _productItems.Filter(new LocatorFilterOptions
        {
            Has = page.Locator($".productinfo [data-product-id='{productId}']")
        });

    public async Task<string> GetTitleAsync() => (await Title.TextContentAsync() ?? "").Trim();

    public Task<int> CountAsync() => _productItems.CountAsync();

    public async Task<List<string>> GetProductIdsAsync()
    {
        var productIds = new List<string>();

        foreach (var item in await _productItems.AllAsync())
        {
            var id = await item.Locator(".productinfo a.add-to-cart").GetAttributeAsync("data-product-id");
            if (id != null)
                productIds.Add(id);
        }

        return productIds;
    }

    public async Task<List<string>> GetProductNamesAsync() =>
        (await _productItems.Locator(".productinfo p").AllInnerTextsAsync()).Select(n => n.Trim()).ToList();

    public async Task<string> GetProductNameAsync(string productId) =>
        (await ProductCard(productId).Locator(".productinfo p").InnerTextAsync()).Trim();

    public async Task<string> GetProductPriceAsync(string productId) =>
        (await ProductCard(productId).Locator(".productinfo h2").InnerTextAsync()).Trim();

    public Task AddToCartAsync(string productId) =>
        ProductCard(productId).Locator(".productinfo a.add-to-cart").ClickAsync();

    public Task ViewProductAsync(string productId) =>
        ProductCard(productId).Locator($"a[href='/product_details/{productId}']").ClickAsync();
}
