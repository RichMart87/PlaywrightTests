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

        for (int i = 0; i < count; i++)
        {
            var id = await _productItems.Nth(i).GetAttributeAsync("data-product-id");
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

    public async Task<bool> IsCartCountUpdatedAsync (int expectedCount)
    {
        var cartCountLocator = page.Locator("a[href='/view_cart'] .cart-count");
        var cartCountText = await cartCountLocator.InnerTextAsync();
        return int.TryParse(cartCountText, out int actualCount) && actualCount == expectedCount;
    }
}