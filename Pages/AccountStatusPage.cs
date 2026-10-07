using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// The confirmation pages shown after creating (/account_created) or deleting (/delete_account) an account.
// Both are a heading plus a "Continue" button back to the home page.
public sealed class AccountStatusPage
{
    private readonly IPage page;

    public AccountStatusPage(IPage page) => this.page = page;

    public ILocator AccountCreatedHeading => page.Locator("[data-qa='account-created']");

    public ILocator AccountDeletedHeading => page.Locator("[data-qa='account-deleted']");

    public Task ContinueAsync() => page.Locator("[data-qa='continue-button']").ClickAsync();
}
