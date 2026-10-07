using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

public sealed class HeaderComponent
{
    private readonly IPage page;

    public HeaderComponent(IPage page) => this.page = page;

    // Scoped to #header and keyed on href rather than position: the menu gains Logout / Delete Account /
    // "Logged in as" items after login, which shifts nth-child indexes, and links like /view_cart also
    // appear elsewhere on the page (e.g. the add-to-cart modal).
    private ILocator _menu => page.Locator("#header .shop-menu");

    public ILocator HomeLink => _menu.Locator("a[href='/']");
    public ILocator ContactUsLink => _menu.Locator("a[href='/contact_us']");
    public ILocator ProductsLink => _menu.Locator("a[href='/products']");
    public ILocator CartLink => _menu.Locator("a[href='/view_cart']");
    public ILocator SignupLoginLink => _menu.Locator("a[href='/login']");
    public ILocator LogoutLink => _menu.Locator("a[href='/logout']");
    public ILocator DeleteAccountLink => _menu.Locator("a[href='/delete_account']");
    public ILocator VideoTutorialsLink => _menu.Locator("a[href*='youtube.com']");

    // Rendered as: <a><i class="fa fa-user"></i> Logged in as <b>{name}</b></a>
    public ILocator LoggedInAs => _menu.Locator("a:has-text('Logged in as')");

    public Task<bool> IsContactUsVisibleAsync() => ContactUsLink.IsVisibleAsync();

    public Task<bool> IsProductsVisibleAsync() => ProductsLink.IsVisibleAsync();

    public Task<bool> IsCartVisibleAsync() => CartLink.IsVisibleAsync();

    public Task<bool> IsSignupLoginVisibleAsync() => SignupLoginLink.IsVisibleAsync();

    public Task<bool> IsLogoutVisibleAsync() => LogoutLink.IsVisibleAsync();

    public Task<string> GetLoggedInUserNameAsync() => LoggedInAs.Locator("b").InnerTextAsync();

    public Task ClickProductsAsync() => ProductsLink.ClickAsync();

    public Task ClickCartAsync() => CartLink.ClickAsync();

    public Task ClickSignupLoginAsync() => SignupLoginLink.ClickAsync();

    public Task ClickLogoutAsync() => LogoutLink.ClickAsync();

    public Task ClickDeleteAccountAsync() => DeleteAccountLink.ClickAsync();

    // Clicks the menu link with the given href (e.g. "/products") and returns the response for the
    // page it opens, so callers can check the status code as well as where they landed.
    public async Task<IResponse> NavigateAsync(string href)
    {
        var response = await page.RunAndWaitForResponseAsync(
            () => _menu.Locator($"a[href='{href}']").ClickAsync(),
            response => response.Request.IsNavigationRequest && response.Frame == page.MainFrame);

        // The response arrives before the new page is parsed - wait for its load event before callers inspect it.
        await page.WaitForURLAsync(response.Url);
        return response;
    }
}
