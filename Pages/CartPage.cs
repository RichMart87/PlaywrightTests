using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

public sealed class CartPage
{
    private readonly IPage page;

    // Each cart line is a row with id="product-<productId>"
    private ILocator _cartRows => page.Locator("#cart_info_table tbody tr[id^='product-']");
    private ILocator _proceedToCheckoutButton => page.Locator("#do_action a.check_out");

    public HeaderComponent Header { get; }
    public SubscriptionComponent Subscription { get; }

    public CartPage(IPage page)
    {
        this.page = page;
        Header = new HeaderComponent(page);
        Subscription = new SubscriptionComponent(page);
    }

    // The cart is tied to the browser session, so items added on other pages in the same context show up here
    public Task<IResponse?> GotoAsync() => page.GotoAsync($"{Config.TestSettings.BaseUrl}/view_cart");

    public ILocator EmptyCartMessage => page.Locator("#empty_cart");

    // Shown to guests who click "Proceed To Checkout"
    public ILocator CheckoutModal => page.Locator("#checkoutModal");

    public ILocator CheckoutModalRegisterLoginLink => CheckoutModal.Locator("a[href='/login']");

    private ILocator Row(string productId) => page.Locator($"#product-{productId}");

    public async Task<List<string>> GetProductIdsAsync()
    {
        var productIds = new List<string>();

        foreach (var row in await _cartRows.AllAsync())
        {
            var id = await row.GetAttributeAsync("id");
            if (id != null)
                productIds.Add(id["product-".Length..]);
        }

        return productIds;
    }

    public async Task<string> GetProductNameAsync(string productId) =>
        (await Row(productId).Locator(".cart_description h4").InnerTextAsync()).Trim();

    public async Task<int> GetPriceAsync(string productId) =>
        ParseRupees(await Row(productId).Locator(".cart_price").InnerTextAsync());

    public async Task<int> GetQuantityAsync(string productId) =>
        int.Parse((await Row(productId).Locator(".cart_quantity").InnerTextAsync()).Trim());

    public async Task<int> GetTotalAsync(string productId) =>
        ParseRupees(await Row(productId).Locator(".cart_total").InnerTextAsync());

    public async Task RemoveProductAsync(string productId)
    {
        await Row(productId).Locator(".cart_quantity_delete").ClickAsync();
        // Removal is an AJAX call that deletes the row in place - no page reload to wait on.
        await Row(productId).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    public async Task<bool> IsEmptyCartMessageVisibleAsync()
    {
        try
        {
            await EmptyCartMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public Task ProceedToCheckoutAsync() => _proceedToCheckoutButton.ClickAsync();

    public async Task<bool> IsCheckoutModalVisibleAsync()
    {
        try
        {
            await CheckoutModal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    // "Rs. 1500" -> 1500
    private static int ParseRupees(string text) => int.Parse(Regex.Match(text, @"\d+").Value);
}
