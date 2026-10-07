using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// /products, plus the pages that reuse its layout: search results, /category_products/{id} and /brand_products/{brand}.
public sealed class ProductsPage
{
    private readonly IPage page;
    private ILocator _searchInput => page.Locator("#search_product");
    private ILocator _searchButton => page.Locator("#submit_search");

    public HeaderComponent Header { get; }
    public ProductGridComponent Products { get; }
    public SidebarComponent Sidebar { get; }
    public CartModalComponent CartModal { get; }

    public ProductsPage(IPage page)
    {
        this.page = page;
        Header = new HeaderComponent(page);
        Products = new ProductGridComponent(page);
        Sidebar = new SidebarComponent(page);
        CartModal = new CartModalComponent(page);
    }

    public Task<IResponse?> GotoAsync() => page.GotoAsync($"{Config.TestSettings.BaseUrl}/products");

    public string Url => page.Url;

    public async Task SearchAsync(string term)
    {
        await _searchInput.FillAsync(term);
        // The button's click handler navigates to /products?search=<term>
        await _searchButton.ClickAsync();
        await page.WaitForURLAsync("**/products?search=*");
    }

    public Task<bool> IsSearchBoxVisibleAsync() => _searchInput.IsVisibleAsync();
}
