using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

// /login hosts two forms side by side: "Login to your account" and "New User Signup!".
public sealed class LoginPage
{
    private readonly IPage page;

    private ILocator _loginForm => page.Locator(".login-form");
    private ILocator _loginEmail => page.Locator("[data-qa='login-email']");
    private ILocator _loginPassword => page.Locator("[data-qa='login-password']");
    private ILocator _loginButton => page.Locator("[data-qa='login-button']");

    private ILocator _signupForm => page.Locator(".signup-form");
    private ILocator _signupName => page.Locator("[data-qa='signup-name']");
    private ILocator _signupEmail => page.Locator("[data-qa='signup-email']");
    private ILocator _signupButton => page.Locator("[data-qa='signup-button']");

    public HeaderComponent Header { get; }

    public LoginPage(IPage page)
    {
        this.page = page;
        Header = new HeaderComponent(page);
    }

    public Task<IResponse?> GotoAsync() => page.GotoAsync($"{Config.TestSettings.BaseUrl}/login");

    public string Url => page.Url;

    public ILocator LoginHeading => _loginForm.Locator("h2");
    public ILocator SignupHeading => _signupForm.Locator("h2");

    // Server-rendered red <p> inside the form, e.g. "Your email or password is incorrect!"
    public ILocator LoginErrorMessage => _loginForm.Locator("form p");

    // e.g. "Email Address already exist!"
    public ILocator SignupErrorMessage => _signupForm.Locator("form p");

    public async Task LoginAsync(string email, string password)
    {
        await _loginEmail.FillAsync(email);
        await _loginPassword.FillAsync(password);
        await _loginButton.ClickAsync();
    }

    public async Task StartSignupAsync(string name, string email)
    {
        await _signupName.FillAsync(name);
        await _signupEmail.FillAsync(email);
        await _signupButton.ClickAsync();
    }

    // Browser-side (HTML5) validation blocks submission without a server round trip,
    // so these read the native validity state instead of looking for an error element.
    public Task<bool> IsSignupEmailValidAsync() =>
        _signupEmail.EvaluateAsync<bool>("el => el.checkValidity()");

    public Task<bool> IsLoginPasswordValidAsync() =>
        _loginPassword.EvaluateAsync<bool>("el => el.checkValidity()");
}
