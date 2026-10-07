using Microsoft.Playwright;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Pages;

// The "Enter Account Information" form at /signup, reached after submitting name + email on /login.
public sealed class SignupPage
{
    private readonly IPage page;

    private ILocator _password => page.Locator("[data-qa='password']");
    private ILocator _days => page.Locator("[data-qa='days']");
    private ILocator _months => page.Locator("[data-qa='months']");
    private ILocator _years => page.Locator("[data-qa='years']");
    private ILocator _newsletter => page.Locator("#newsletter");
    private ILocator _optin => page.Locator("#optin");
    private ILocator _firstName => page.Locator("[data-qa='first_name']");
    private ILocator _lastName => page.Locator("[data-qa='last_name']");
    private ILocator _company => page.Locator("[data-qa='company']");
    private ILocator _address1 => page.Locator("[data-qa='address']");
    private ILocator _address2 => page.Locator("[data-qa='address2']");
    private ILocator _country => page.Locator("[data-qa='country']");
    private ILocator _state => page.Locator("[data-qa='state']");
    private ILocator _city => page.Locator("[data-qa='city']");
    private ILocator _zipcode => page.Locator("[data-qa='zipcode']");
    private ILocator _mobileNumber => page.Locator("[data-qa='mobile_number']");
    private ILocator _createAccountButton => page.Locator("[data-qa='create-account']");

    public SignupPage(IPage page) => this.page = page;

    public ILocator AccountInfoHeading => page.Locator("h2.title", new() { HasText = "Enter Account Information" });

    // Pre-filled from the /login signup form
    public ILocator NameInput => page.Locator("[data-qa='name']");
    public ILocator EmailInput => page.Locator("[data-qa='email']");

    public async Task FillAccountDetailsAsync(ApiUser user, bool newsletter = true, bool specialOffers = true)
    {
        // Title radios are id_gender1 (Mr) / id_gender2 (Mrs); the value attribute matches ApiUser.Title
        await page.Locator($"input[name='title'][value='{user.Title}']").CheckAsync();
        await _password.FillAsync(user.Password);

        // Option values are plain numbers (months are 1-12), matching ApiUser's birth fields
        await _days.SelectOptionAsync(user.BirthDate);
        await _months.SelectOptionAsync(user.BirthMonth);
        await _years.SelectOptionAsync(user.BirthYear);

        await _newsletter.SetCheckedAsync(newsletter);
        await _optin.SetCheckedAsync(specialOffers);

        await _firstName.FillAsync(user.Firstname);
        await _lastName.FillAsync(user.Lastname);
        await _company.FillAsync(user.Company);
        await _address1.FillAsync(user.Address1);
        await _address2.FillAsync(user.Address2);
        await _country.SelectOptionAsync(user.Country);
        await _state.FillAsync(user.State);
        await _city.FillAsync(user.City);
        await _zipcode.FillAsync(user.Zipcode);
        await _mobileNumber.FillAsync(user.MobileNumber);
    }

    public Task CreateAccountAsync() => _createAccountButton.ClickAsync();
}
