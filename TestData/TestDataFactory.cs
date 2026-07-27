namespace PlaywrightTests.TestData;

public static class TestDataFactory
{
    // Add methods to create complex fixtures or load JSON test data
    public static IEnumerable<object[]> GetTestData()
    {
        // Example of returning test data for parameterized tests
        yield return new object[] { "TestData1", 123 };
        yield return new object[] { "TestData2", 456 };
    }

    


}