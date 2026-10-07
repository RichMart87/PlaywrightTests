using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// The "Added!" modal shown after any add-to-cart click (home, products and product details pages).
public sealed class CartModalComponent
{
    private readonly IPage page;

    public CartModalComponent(IPage page) => this.page = page;

    public ILocator Root => page.Locator("#cartModal");

    private ILocator _continueShoppingButton => Root.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
    private ILocator _viewCartLink => Root.Locator("a[href='/view_cart']");

    public async Task<bool> IsVisibleAsync()
    {
        // The modal fades in after the add-to-cart AJAX call completes, so unlike a plain
        // IsVisibleAsync() check (no auto-wait) this needs to actively wait for that state.
        try
        {
            await Root.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task ContinueShoppingAsync()
    {
        await _continueShoppingButton.ClickAsync();
        // The modal fades out rather than closing instantly - wait for it to be gone so the next
        // add-to-cart check can't be satisfied by this (old) modal still being on screen.
        await Root.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task ViewCartAsync()
    {
        await _viewCartLink.ClickAsync();
        // Wait for the cart page's load event: its click handlers (e.g. Proceed To Checkout) are bound by
        // inline scripts at the end of <body>, so acting on it earlier can click a button that does nothing.
        await page.WaitForURLAsync("**/view_cart");
    }
}
