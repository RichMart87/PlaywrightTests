using System.Net.Http.Json;
using System.Text.Json.Nodes;
using PlaywrightTests.Config;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Tests;

// Hits automationexercise.com's REST API directly (no browser needed).
// Note: this API always returns transport-level HTTP 200 - the real status
// lives in the "responseCode" field of the JSON body, so assertions target that field.
[TestClass]
public sealed class ApiTests
{
    private static HttpClient client = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        client = new HttpClient { BaseAddress = new Uri(TestSettings.ApiBaseUrl) };
    }

    [ClassCleanup]
    public static void ClassCleanup() => client.Dispose();

    // ---- Products ----

    [TestMethod]
    public async Task GetProductsList_ReturnsAllProducts()
    {
        var json = await GetAsync("productsList");

        Assert.AreEqual(200, (int)json["responseCode"]!);
        Assert.IsTrue(json["products"]!.AsArray().Count > 0, "Expected at least one product.");
    }

    [TestMethod]
    public async Task PostProductsList_MethodNotSupported_Returns405()
    {
        var json = await PostAsync("productsList");

        Assert.AreEqual(405, (int)json["responseCode"]!);
    }

    // ---- Brands ----

    [TestMethod]
    public async Task GetBrandsList_ReturnsAllBrands()
    {
        var json = await GetAsync("brandsList");

        Assert.AreEqual(200, (int)json["responseCode"]!);
        Assert.IsTrue(json["brands"]!.AsArray().Count > 0, "Expected at least one brand.");
    }

    [TestMethod]
    public async Task PutBrandsList_MethodNotSupported_Returns405()
    {
        var json = await PutAsync("brandsList");

        Assert.AreEqual(405, (int)json["responseCode"]!);
    }

    // ---- Search product ----

    [TestMethod]
    public async Task SearchProduct_WithValidTerm_ReturnsMatchingProducts()
    {
        var json = await PostAsync("searchProduct", ("search_product", "Top"));

        Assert.AreEqual(200, (int)json["responseCode"]!);
        Assert.IsTrue(json["products"]!.AsArray().Count > 0, "Expected at least one matching product.");
    }

    [TestMethod]
    public async Task SearchProduct_MissingParameter_Returns400()
    {
        var json = await PostAsync("searchProduct");

        Assert.AreEqual(400, (int)json["responseCode"]!);
    }

    // ---- Verify login ----

    [TestMethod]
    public async Task VerifyLogin_WithRegisteredUser_ReturnsUserExists()
    {
        var user = TestDataFactory.CreateApiUser();
        await PostAsync("createAccount", ToFields(user));

        try
        {
            var json = await PostAsync("verifyLogin", ("email", user.Email), ("password", user.Password));
            Assert.AreEqual(200, (int)json["responseCode"]!);
        }
        finally
        {
            await DeleteAsync("deleteAccount", ("email", user.Email), ("password", user.Password));
        }
    }

    [TestMethod]
    public async Task VerifyLogin_MissingEmail_Returns400()
    {
        var json = await PostAsync("verifyLogin", ("password", "whatever-password"));

        Assert.AreEqual(400, (int)json["responseCode"]!);
    }

    [TestMethod]
    public async Task DeleteVerifyLogin_MethodNotSupported_Returns405()
    {
        var json = await DeleteAsync("verifyLogin");

        Assert.AreEqual(405, (int)json["responseCode"]!);
    }

    [TestMethod]
    public async Task VerifyLogin_WithUnknownUser_Returns404()
    {
        var json = await PostAsync(
            "verifyLogin",
            ("email", $"no-such-user.{Guid.NewGuid():N}@mailinator.com"),
            ("password", "wrong-password"));

        Assert.AreEqual(404, (int)json["responseCode"]!);
    }

    // ---- Account lifecycle ----

    [TestMethod]
    public async Task CreateAccount_WithValidData_ReturnsCreated()
    {
        var user = TestDataFactory.CreateApiUser();

        try
        {
            var json = await PostAsync("createAccount", ToFields(user));
            Assert.AreEqual(201, (int)json["responseCode"]!);
        }
        finally
        {
            await DeleteAsync("deleteAccount", ("email", user.Email), ("password", user.Password));
        }
    }

    [TestMethod]
    public async Task GetUserDetailByEmail_ForExistingUser_ReturnsProfile()
    {
        var user = TestDataFactory.CreateApiUser();
        await PostAsync("createAccount", ToFields(user));

        try
        {
            var json = await GetAsync($"getUserDetailByEmail?email={Uri.EscapeDataString(user.Email)}");

            Assert.AreEqual(200, (int)json["responseCode"]!);
            Assert.AreEqual(user.Email, json["user"]!["email"]!.ToString());
        }
        finally
        {
            await DeleteAsync("deleteAccount", ("email", user.Email), ("password", user.Password));
        }
    }

    [TestMethod]
    public async Task UpdateAccount_WithValidData_ReturnsUpdated()
    {
        var user = TestDataFactory.CreateApiUser();
        await PostAsync("createAccount", ToFields(user));

        try
        {
            var updated = user with { Firstname = "Updated" };
            var json = await PutAsync("updateAccount", ToFields(updated));

            Assert.AreEqual(200, (int)json["responseCode"]!);
        }
        finally
        {
            await DeleteAsync("deleteAccount", ("email", user.Email), ("password", user.Password));
        }
    }

    [TestMethod]
    public async Task DeleteAccount_ForExistingUser_ReturnsOk()
    {
        var user = TestDataFactory.CreateApiUser();
        await PostAsync("createAccount", ToFields(user));

        var json = await DeleteAsync("deleteAccount", ("email", user.Email), ("password", user.Password));

        Assert.AreEqual(200, (int)json["responseCode"]!);
    }

    // ---- Helpers ----

    private static (string Key, string Value)[] ToFields(ApiUser user) =>
    [
        ("name", user.Name),
        ("email", user.Email),
        ("password", user.Password),
        ("title", user.Title),
        ("birth_date", user.BirthDate),
        ("birth_month", user.BirthMonth),
        ("birth_year", user.BirthYear),
        ("firstname", user.Firstname),
        ("lastname", user.Lastname),
        ("company", user.Company),
        ("address1", user.Address1),
        ("address2", user.Address2),
        ("country", user.Country),
        ("zipcode", user.Zipcode),
        ("state", user.State),
        ("city", user.City),
        ("mobile_number", user.MobileNumber),
    ];

    private static async Task<JsonNode> GetAsync(string endpoint)
    {
        var response = await client.GetAsync(endpoint);
        return (await response.Content.ReadFromJsonAsync<JsonNode>())!;
    }

    private static Task<JsonNode> PostAsync(string endpoint, params (string Key, string Value)[] fields) =>
        SendAsync(HttpMethod.Post, endpoint, fields);

    private static Task<JsonNode> PutAsync(string endpoint, params (string Key, string Value)[] fields) =>
        SendAsync(HttpMethod.Put, endpoint, fields);

    private static Task<JsonNode> DeleteAsync(string endpoint, params (string Key, string Value)[] fields) =>
        SendAsync(HttpMethod.Delete, endpoint, fields);

    private static async Task<JsonNode> SendAsync(HttpMethod method, string endpoint, (string Key, string Value)[] fields)
    {
        var request = new HttpRequestMessage(method, endpoint)
        {
            Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value)))
        };

        var response = await client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<JsonNode>())!;
    }
}
