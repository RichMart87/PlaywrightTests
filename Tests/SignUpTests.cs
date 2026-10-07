using PlaywrightTests.Infrastructure;
using PlaywrightTests.Pages;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Tests;

[TestClass]
[TestCategory(TestCategories.Regression)]
public sealed class SignUpTests : PlaywrightTestBase
{
    [TestMethod]
    public async Task WhenRegisteringNewUser_AccountIsCreatedLoggedInAndCanBeDeleted()
    {
        var user = TestDataFactory.CreateApiUser();

        try
        {
            var home = new HomePage(Page!);
            await home.GotoAsync();
            await home.Header.ClickSignupLoginAsync();

            var login = new LoginPage(Page!);
            Assert.IsTrue(await login.SignupHeading.IsVisibleAsync(), "Expected the 'New User Signup!' form.");
            await login.StartSignupAsync(user.Name, user.Email);

            var signup = new SignupPage(Page!);
            Assert.IsTrue(await signup.AccountInfoHeading.IsVisibleAsync(), "Expected the 'Enter Account Information' form.");
            await signup.FillAccountDetailsAsync(user);
            await signup.CreateAccountAsync();

            var status = new AccountStatusPage(Page!);
            Assert.IsTrue(await status.AccountCreatedHeading.IsVisibleAsync(), "Expected 'Account Created!'.");
            await status.ContinueAsync();

            Assert.AreEqual(user.Name, await home.Header.GetLoggedInUserNameAsync(), "New user should be logged in after signup.");

            await home.Header.ClickDeleteAccountAsync();
            Assert.IsTrue(await status.AccountDeletedHeading.IsVisibleAsync(), "Expected 'Account Deleted!'.");
        }
        finally
        {
            // No-op when the UI delete above succeeded; cleans up if the test failed part-way.
            await AccountApi.DeleteUserAsync(user);
        }
    }

    [TestMethod]
    public async Task WhenStartingSignup_AccountInfoIsPrefilledFromSignupForm()
    {
        var name = "QA Prefill";
        var email = TestDataFactory.CreateUniqueEmail();

        var login = new LoginPage(Page!);
        await login.GotoAsync();
        await login.StartSignupAsync(name, email);

        // Nothing is persisted until "Create Account", so no cleanup is needed.
        var signup = new SignupPage(Page!);
        Assert.IsTrue(await signup.AccountInfoHeading.IsVisibleAsync(), "Expected the 'Enter Account Information' form.");
        Assert.AreEqual(name, await signup.NameInput.InputValueAsync());
        Assert.AreEqual(email, await signup.EmailInput.InputValueAsync());
        Assert.IsTrue(await signup.EmailInput.IsDisabledAsync(), "Email chosen on the previous step should not be editable.");
    }

    [TestMethod]
    public async Task WhenSigningUpWithExistingEmail_ErrorIsShown()
    {
        var existing = await AccountApi.CreateUserAsync();

        try
        {
            var login = new LoginPage(Page!);
            await login.GotoAsync();
            await login.StartSignupAsync("Someone Else", existing.Email);

            Assert.AreEqual("Email Address already exist!", (await login.SignupErrorMessage.InnerTextAsync()).Trim());
            Assert.EndsWith("/signup", login.Url);
        }
        finally
        {
            await AccountApi.DeleteUserAsync(existing);
        }
    }

    [TestMethod]
    [DataRow("not-an-email")]
    [DataRow("missing-domain@")]
    public async Task WhenSigningUpWithInvalidEmailFormat_BrowserBlocksSubmission(string invalidEmail)
    {
        var login = new LoginPage(Page!);
        await login.GotoAsync();

        await login.StartSignupAsync("QA Invalid", invalidEmail);

        Assert.IsFalse(await login.IsSignupEmailValidAsync(), $"'{invalidEmail}' should fail HTML5 email validation.");
        Assert.EndsWith("/login", login.Url, "Form should not have been submitted.");
    }
}
