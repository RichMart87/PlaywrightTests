using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

public sealed class HomePage
{
    private readonly IPage page;
    private ILocator _recommendedItems => page.Locator(".recommended_items");
    private ILocator _scrollUpButton => page.Locator("#scrollUp");

    public HeaderComponent Header { get; }
    public ProductGridComponent Products { get; }
    public SidebarComponent Sidebar { get; }
    public CartModalComponent CartModal { get; }
    public SubscriptionComponent Subscription { get; }

    public HomePage(IPage page)
    {
        this.page = page;
        Header = new HeaderComponent(page);
        Products = new ProductGridComponent(page);
        Sidebar = new SidebarComponent(page);
        CartModal = new CartModalComponent(page);
        Subscription = new SubscriptionComponent(page);
    }

    // Navigate to the URL of the home page
    public Task<IResponse?> GotoAsync() => page.GotoAsync(Config.TestSettings.BaseUrl);

    // Get the title of the home page
    public Task<string> TitleAsync() => page.TitleAsync();

    public ILocator Logo => page.Locator("img[alt='Website for automation practice']");

    public Task<bool> IsLogoVisibleAsync() => Logo.IsVisibleAsync();

    public ILocator FeaturedProductsSection => Products.Root;

    public Task<bool> IsFeaturedProductsVisibleAsync() => FeaturedProductsSection.IsVisibleAsync();

    // Hero carousel tagline, used to confirm the page is scrolled back to the top
    public ILocator HeroTagline => page.Locator("#slider-carousel .item.active h2");

    public ILocator GetFeaturedProductByName(string productName) =>
        FeaturedProductsSection.Locator($".productinfo p:has-text('{productName}')");

    public Task ClickOnContinueShoppingAsync() => CartModal.ContinueShoppingAsync();

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
        var productIds = await Products.GetProductIdsAsync();

        if (productIds.Count == 0)
            throw new InvalidOperationException("No product items were found on the home page.");

        return productIds;
    }

    public Task ClickOnProductByIdAsync(string productId) => Products.ViewProductAsync(productId);

    // Goes through the featured-items grid because the same product ids are
    // repeated in a "recommended items" carousel further down the page.
    public Task AddProductToCartByIdAsync(string productId) => Products.AddToCartAsync(productId);

    public ILocator AddToCartModal => CartModal.Root;

    public Task<bool> IsAddToCartConfirmationVisibleAsync() => CartModal.IsVisibleAsync();

    // ---- Recommended items carousel ----

    public ILocator RecommendedItemsHeading => _recommendedItems.Locator("h2.title");

    // The carousel rotates, so only the active slide's cards are visible and clickable.
    public async Task<string> AddFirstVisibleRecommendedItemToCartAsync()
    {
        await RecommendedItemsHeading.ScrollIntoViewIfNeededAsync();
        var addButton = _recommendedItems.Locator(".item.active a.add-to-cart").First;
        var productId = await addButton.GetAttributeAsync("data-product-id")
            ?? throw new InvalidOperationException("Recommended item has no data-product-id.");
        await addButton.ClickAsync();
        return productId;
    }

    // ---- Scrolling ----

    public Task ScrollToBottomAsync() =>
        page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");

    public Task ClickScrollUpArrowAsync() => _scrollUpButton.ClickAsync();

    public async Task<bool> WaitForScrolledToTopAsync()
    {
        // jquery.scrollUp animates the scroll, so poll rather than reading scrollY once.
        try
        {
            await page.WaitForFunctionAsync("() => window.scrollY === 0");
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
