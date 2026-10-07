using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// Left sidebar with the Category accordion and Brands list (home, /products, category and brand pages).
public sealed class SidebarComponent
{
    private readonly IPage page;

    public SidebarComponent(IPage page) => this.page = page;

    private ILocator _categoryAccordion => page.Locator("#accordian");
    private ILocator _brands => page.Locator(".brands_products .brands-name");

    // Brand links have the form "/brand_products/{brand}" with a "(count)" badge before the name.
    private ILocator BrandLink(string brand) => _brands.Locator($"a[href='/brand_products/{brand}']");

    public Task ExpandCategoryAsync(string category) =>
        _categoryAccordion.Locator($"a[href='#{category}']").ClickAsync();

    // Sub-category names repeat across categories (Women > Dress and Kids > Dress), so scope to the
    // expanded panel. Click auto-waits for the panel's collapse animation to make the link visible.
    public async Task OpenSubCategoryAsync(string category, string subCategory)
    {
        await ExpandCategoryAsync(category);
        await page.Locator($"#{category}")
            .GetByRole(AriaRole.Link, new() { Name = subCategory, Exact = true })
            .ClickAsync();
    }

    public Task OpenBrandAsync(string brand) => BrandLink(brand).ClickAsync();

    public async Task<int> GetBrandProductCountAsync(string brand)
    {
        var badge = await BrandLink(brand).Locator("span").InnerTextAsync();
        return int.Parse(Regex.Match(badge, @"\d+").Value);
    }
}
