using System.Net.Http.Json;
using System.Text.Json.Nodes;
using PlaywrightTests.Config;

namespace PlaywrightTests.TestData;

// Seeds and cleans up user accounts through the REST API so UI tests that need an existing
// account (login, cart-after-login, duplicate signup) don't have to drive the signup form first.
public static class AccountApi
{
    private static readonly HttpClient client = new() { BaseAddress = new Uri(TestSettings.ApiBaseUrl) };

    public static async Task<ApiUser> CreateUserAsync()
    {
        var user = TestDataFactory.CreateApiUser();
        var responseCode = await SendAsync(HttpMethod.Post, "createAccount", user.ToFormFields());

        // The API always answers HTTP 200 - the real status is the "responseCode" in the body.
        if (responseCode != 201)
            throw new InvalidOperationException($"createAccount for {user.Email} returned responseCode {responseCode}, expected 201.");

        return user;
    }

    // Best-effort cleanup: a test may already have deleted the account through the UI.
    public static async Task DeleteUserAsync(ApiUser user)
    {
        try
        {
            await SendAsync(HttpMethod.Delete, "deleteAccount", [("email", user.Email), ("password", user.Password)]);
        }
        catch (Exception)
        {
            // Cleanup must never mask the test's own outcome.
        }
    }

    private static async Task<int> SendAsync(HttpMethod method, string endpoint, (string Key, string Value)[] fields)
    {
        var request = new HttpRequestMessage(method, endpoint)
        {
            Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value)))
        };

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadFromJsonAsync<JsonNode>();
        return (int)json!["responseCode"]!;
    }
}
