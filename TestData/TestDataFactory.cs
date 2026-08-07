namespace PlaywrightTests.TestData;

public sealed record ApiUser(
    string Name,
    string Email,
    string Password,
    string Title,
    string BirthDate,
    string BirthMonth,
    string BirthYear,
    string Firstname,
    string Lastname,
    string Company,
    string Address1,
    string Address2,
    string Country,
    string Zipcode,
    string State,
    string City,
    string MobileNumber);

public static class TestDataFactory
{
    // Add methods to create complex fixtures or load JSON test data
    public static IEnumerable<object[]> GetTestData()
    {
        // Example of returning test data for parameterized tests
        yield return new object[] { "TestData1", 123 };
        yield return new object[] { "TestData2", 456 };
    }

    // Unique email per call so parallel/rerun test runs never collide against an already-registered account.
    public static ApiUser CreateApiUser()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        return new ApiUser(
            Name: $"QA Automation {unique}",
            Email: $"qa.playwright.{unique}@mailinator.com",
            Password: "P@ssw0rd!123",
            Title: "Mr",
            BirthDate: "15",
            BirthMonth: "6",
            BirthYear: "1990",
            Firstname: "QA",
            Lastname: "Automation",
            Company: "Acme Corp",
            Address1: "123 Test Street",
            Address2: "Suite 100",
            Country: "United States",
            Zipcode: "10001",
            State: "New York",
            City: "New York",
            MobileNumber: "5555550100");
    }
}
