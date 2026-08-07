using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

public sealed class HomePage
{
    private readonly IPage page;
    private ILocator _productItems => page.Locator("div.features_items .product-image-wrapper");
    public HeaderComponent Header { get; }

    public HomePage(IPage page)
    {
        this.page = page;
        Header = new HeaderComponent(page);
    }

    // Navigate to the URL of the home page
    public Task<IResponse?> GotoAsync() => page.GotoAsync(Config.TestSettings.BaseUrl);

    // Get the title of the home page
    public Task<string> TitleAsync() => page.TitleAsync();

    public ILocator Logo => page.Locator("img[alt='Website for automation practice']");

    public Task<bool> IsLogoVisibleAsync() => Logo.IsVisibleAsync();

    public ILocator FeaturedProductsSection => page.Locator("div.features_items");

    public Task<bool> IsFeaturedProductsVisibleAsync() => FeaturedProductsSection.IsVisibleAsync();

    public ILocator GetFeaturedProductByName(string productName) =>
        FeaturedProductsSection.Locator($".productinfo p:has-text('{productName}')");

    public Task ClickOnFeaturedProductByNameAsync(string productName) =>
        GetFeaturedProductByName(productName).ClickAsync();

    public ILocator GetCategoryLinkByName(string categoryName) =>
        page.Locator($"//a[normalize-space(text())='{categoryName}']");

    public Task ClickOnCategoryLinkByNameAsync(string categoryName) =>
        GetCategoryLinkByName(categoryName).ClickAsync();

    public ILocator GetSubCategoryLinkByName(string subCategoryName) =>
        page.Locator($"//a[normalize-space(text())='{subCategoryName}']");

    public Task ClickOnSubCategoryLinkByNameAsync(string subCategoryName) =>
        GetSubCategoryLinkByName(subCategoryName).ClickAsync();

    public Task ClickOnProductCategoryAsync(string categoryName) =>
        page.Locator($"//a[normalize-space(text())='{categoryName}']").ClickAsync();

    public async Task<List<string>> GetAllProductIdsAsync()
    {
        var productIds = new List<string>();
        var count = await _productItems.CountAsync();

        if (count == 0)
            throw new InvalidOperationException("No product items were found on the home page.");

        for (int i = 0; i < count; i++)
        {
            // data-product-id lives on the "Add to cart" link inside .productinfo, not on the wrapper
            // itself, and the same link is repeated inside .product-overlay - scope to .productinfo so
            // the locator resolves to exactly one element per product instead of two.
            var id = await _productItems.Nth(i).Locator(".productinfo a.add-to-cart").GetAttributeAsync("data-product-id");
            if (id != null)
                productIds.Add(id);
        }

        return productIds;
    }

    public async Task ClickOnProductByIdAsync(string productId)
    {
        var productLocator = _productItems.Filter(new LocatorFilterOptions
        {
            Has = page.Locator($"[data-product-id='{productId}']")
        });

        await productLocator.ClickAsync();
    }

    public Task AddProductToCartByIdAsync(string productId) =>
        // Scoped to _productItems (the featured-items grid) because the same product ids are
        // repeated in a "recommended items" carousel further down the page.
        _productItems
            .Filter(new LocatorFilterOptions { Has = page.Locator($"[data-product-id='{productId}']") })
            .Locator(".productinfo a.add-to-cart")
            .ClickAsync();

    public ILocator AddToCartModal => page.Locator("#cartModal");

    public async Task<bool> IsAddToCartConfirmationVisibleAsync()
    {
        // The modal fades in after the add-to-cart AJAX call completes, so unlike a plain
        // IsVisibleAsync() check (no auto-wait) this needs to actively wait for that state.
        try
        {
            await AddToCartModal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
