using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// Footer "Subscription" form, present on every page.
public sealed class SubscriptionComponent
{
    private readonly IPage page;

    public SubscriptionComponent(IPage page) => this.page = page;

    public ILocator Heading => page.Locator("footer .single-widget h2", new() { HasText = "Subscription" });

    // The site's own id is misspelled ("susbscribe_email").
    private ILocator _emailInput => page.Locator("#susbscribe_email");
    private ILocator _subscribeButton => page.Locator("#subscribe");
    private ILocator _successAlert => page.Locator("#success-subscribe");

    public async Task SubscribeAsync(string email)
    {
        await Heading.ScrollIntoViewIfNeededAsync();
        await _emailInput.FillAsync(email);
        await _subscribeButton.ClickAsync();
    }

    public async Task<bool> IsSuccessMessageVisibleAsync()
    {
        // Revealed by removing the "hide" class after an AJAX call, then hidden again a few seconds later.
        try
        {
            await _successAlert.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public Task<string> GetSuccessMessageAsync() => _successAlert.InnerTextAsync();
}
