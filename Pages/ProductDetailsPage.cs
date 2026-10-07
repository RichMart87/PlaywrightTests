using Microsoft.Playwright;

namespace PlaywrightTests.Pages;

public sealed class ProductDetailsPage
{
    private readonly IPage page;
    private ILocator _info => page.Locator(".product-information");
    private ILocator _quantityInput => page.Locator("#quantity");
    private ILocator _addToCartButton => _info.Locator("button.cart");

    private ILocator _reviewName => page.Locator("#review-form #name");
    private ILocator _reviewEmail => page.Locator("#review-form #email");
    private ILocator _reviewText => page.Locator("#review-form #review");
    private ILocator _submitReviewButton => page.Locator("#button-review");
    // Target the alert itself: its #review-section wrapper only contains a floated column, so the
    // wrapper has zero height and Playwright never considers it visible.
    private ILocator _reviewSuccess => page.Locator("#review-section .alert-success");

    public CartModalComponent CartModal { get; }

    public ProductDetailsPage(IPage page)
    {
        this.page = page;
        CartModal = new CartModalComponent(page);
    }

    public Task<IResponse?> GotoAsync(string productId) =>
        page.GotoAsync($"{Config.TestSettings.BaseUrl}/product_details/{productId}");

    public string Url => page.Url;

    public ILocator Name => _info.Locator("h2");

    public async Task<string> GetNameAsync() => (await Name.InnerTextAsync()).Trim();

    // e.g. "Rs. 500" - the first span inside the price/quantity block
    public async Task<string> GetPriceAsync() => (await _info.Locator("span > span").First.InnerTextAsync()).Trim();

    // "Category: Women > Tops" -> "Women > Tops"
    public Task<string> GetCategoryAsync() => GetLabelledValueAsync("Category");

    public Task<string> GetAvailabilityAsync() => GetLabelledValueAsync("Availability");

    public Task<string> GetConditionAsync() => GetLabelledValueAsync("Condition");

    public Task<string> GetBrandAsync() => GetLabelledValueAsync("Brand");

    public Task SetQuantityAsync(int quantity) => _quantityInput.FillAsync(quantity.ToString());

    public Task AddToCartAsync() => _addToCartButton.ClickAsync();

    public async Task SubmitReviewAsync(string name, string email, string review)
    {
        await _reviewName.FillAsync(name);
        await _reviewEmail.FillAsync(email);
        await _reviewText.FillAsync(review);
        await _submitReviewButton.ClickAsync();
    }

    public async Task<bool> IsReviewSuccessVisibleAsync()
    {
        // The "Thank you for your review." alert is un-hidden for only 2 seconds after submit
        try
        {
            await _reviewSuccess.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    // Product attributes render as "<p><b>Label:</b> value</p>" (Category is "<p>Category: value</p>")
    private async Task<string> GetLabelledValueAsync(string label)
    {
        var text = await _info.Locator("p", new() { HasText = $"{label}:" }).InnerTextAsync();
        return text[(text.IndexOf(':') + 1)..].Trim();
    }
}
