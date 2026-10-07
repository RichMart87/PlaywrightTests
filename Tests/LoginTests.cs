using PlaywrightTests.Infrastructure;
using PlaywrightTests.Pages;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Tests;

[TestClass]
[TestCategory(TestCategories.Regression)]
public sealed class LoginTests : PlaywrightTestBase
{
    private const string InvalidCredentialsMessage = "Your email or password is incorrect!";

    [TestMethod]
    public async Task WhenLoggingInWithValidCredentials_UserIsLoggedIn()
    {
        var user = await AccountApi.CreateUserAsync();

        try
        {
            var login = new LoginPage(Page!);
            await login.GotoAsync();
            Assert.IsTrue(await login.LoginHeading.IsVisibleAsync(), "Expected the 'Login to your account' form.");

            await login.LoginAsync(user.Email, user.Password);

            Assert.AreEqual(user.Name, await login.Header.GetLoggedInUserNameAsync());
            Assert.IsTrue(await login.Header.IsLogoutVisibleAsync(), "Expected a Logout link once logged in.");
            Assert.IsFalse(await login.Header.IsSignupLoginVisibleAsync(), "Signup / Login link should be hidden once logged in.");
        }
        finally
        {
            await AccountApi.DeleteUserAsync(user);
        }
    }

    [TestMethod]
    public async Task WhenLoggingInWithWrongPassword_ErrorIsShown()
    {
        var user = await AccountApi.CreateUserAsync();

        try
        {
            var login = new LoginPage(Page!);
            await login.GotoAsync();

            await login.LoginAsync(user.Email, "definitely-not-the-password");

            Assert.AreEqual(InvalidCredentialsMessage, (await login.LoginErrorMessage.InnerTextAsync()).Trim());
            Assert.IsFalse(await login.Header.IsLogoutVisibleAsync(), "User should not be logged in.");
        }
        finally
        {
            await AccountApi.DeleteUserAsync(user);
        }
    }

    [TestMethod]
    public async Task WhenLoggingInWithUnregisteredEmail_ErrorIsShown()
    {
        var login = new LoginPage(Page!);
        await login.GotoAsync();

        await login.LoginAsync(TestDataFactory.CreateUniqueEmail(), "whatever-password");

        Assert.AreEqual(InvalidCredentialsMessage, (await login.LoginErrorMessage.InnerTextAsync()).Trim());
    }

    [TestMethod]
    public async Task WhenLoggingInWithEmptyPassword_BrowserBlocksSubmission()
    {
        var login = new LoginPage(Page!);
        await login.GotoAsync();

        await login.LoginAsync(TestDataFactory.CreateUniqueEmail(), "");

        Assert.IsFalse(await login.IsLoginPasswordValidAsync(), "Empty password should fail HTML5 'required' validation.");
        Assert.AreEqual(0, await login.LoginErrorMessage.CountAsync(), "Form should not have reached the server.");
    }

    [TestMethod]
    public async Task WhenLoggingOut_UserIsReturnedToLoginPage()
    {
        var user = await AccountApi.CreateUserAsync();

        try
        {
            var login = new LoginPage(Page!);
            await login.GotoAsync();
            await login.LoginAsync(user.Email, user.Password);
            Assert.AreEqual(user.Name, await login.Header.GetLoggedInUserNameAsync());

            await login.Header.ClickLogoutAsync();

            Assert.EndsWith("/login", login.Url);
            Assert.IsTrue(await login.Header.IsSignupLoginVisibleAsync(), "Expected the Signup / Login link after logout.");
            Assert.AreEqual(0, await login.Header.LoggedInAs.CountAsync(), "'Logged in as' should be gone after logout.");
        }
        finally
        {
            await AccountApi.DeleteUserAsync(user);
        }
    }

    [TestMethod]
    public async Task WhenLoggingInAfterAccountDeleted_ErrorIsShown()
    {
        var user = await AccountApi.CreateUserAsync();

        try
        {
            var login = new LoginPage(Page!);
            await login.GotoAsync();
            await login.LoginAsync(user.Email, user.Password);
            await login.Header.ClickDeleteAccountAsync();
            Assert.IsTrue(await new AccountStatusPage(Page!).AccountDeletedHeading.IsVisibleAsync(), "Expected 'Account Deleted!'.");

            await login.GotoAsync();
            await login.LoginAsync(user.Email, user.Password);

            Assert.AreEqual(InvalidCredentialsMessage, (await login.LoginErrorMessage.InnerTextAsync()).Trim(), "Deleted account should no longer be able to log in.");
        }
        finally
        {
            await AccountApi.DeleteUserAsync(user);
        }
    }
}
