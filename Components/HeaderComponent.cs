using Microsoft.Playwright;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlaywrightTests.Pages;

public sealed class HeaderComponent
{
    private readonly IPage page;

    public HeaderComponent(IPage page) => this.page = page;

    // Prefer CSS + stable attributes where possible
    public ILocator ContactUsLink => page.Locator("a[href='/contact_us']");

    public ILocator ProductsLink => page.Locator("a[href='/products']");
    public ILocator CartLink => page.Locator("#header ul > li:nth-child(3) > a");
    public ILocator SignupLoginLink => page.Locator("#header ul > li:nth-child(4) > a");

    public Task<bool> IsContactUsVisibleAsync() => ContactUsLink.IsVisibleAsync();

    public Task<bool> IsProductsVisibleAsync() => ProductsLink.IsVisibleAsync();

    public Task<bool> IsCartVisibleAsync() => CartLink.IsVisibleAsync();

    public Task<bool> IsSignupLoginVisibleAsync() => SignupLoginLink.IsVisibleAsync();
}