using PlaywrightTests.Infrastructure;
using PlaywrightTests.Pages;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Tests;

[TestClass]
[TestCategory(TestCategories.Regression)]
public sealed class CartTests : PlaywrightTestBase
{
    [TestMethod]
    public async Task WhenCartIsEmpty_EmptyCartMessageIsShown()
    {
        var cart = new CartPage(Page!);
        await cart.GotoAsync();

        Assert.IsTrue(await cart.IsEmptyCartMessageVisibleAsync(), "Expected the 'Cart is empty!' message.");
        Assert.AreEqual(0, (await cart.GetProductIdsAsync()).Count);
    }

    [TestMethod]
    public async Task WhenAddingProductsFromProductsPage_CartShowsPriceQuantityAndTotal()
    {
        var products = new ProductsPage(Page!);
        await products.GotoAsync();

        var productIds = (await products.Products.GetProductIdsAsync()).Take(2).ToList();
        var expected = new Dictionary<string, (string Name, string Price)>();
        foreach (var productId in productIds)
        {
            expected[productId] = (await products.Products.GetProductNameAsync(productId), await products.Products.GetProductPriceAsync(productId));
            await products.Products.AddToCartAsync(productId);
            Assert.IsTrue(await products.CartModal.IsVisibleAsync(), $"Expected the 'Added to cart' modal for product {productId}.");
            await products.CartModal.ContinueShoppingAsync();
        }

        var cart = new CartPage(Page!);
        await cart.GotoAsync();

        CollectionAssert.AreEquivalent(productIds, await cart.GetProductIdsAsync());
        foreach (var productId in productIds)
        {
            var price = await cart.GetPriceAsync(productId);
            Assert.AreEqual(expected[productId].Name, await cart.GetProductNameAsync(productId), $"Name mismatch for product {productId}.");
            Assert.Contains(price.ToString(), expected[productId].Price, $"Price mismatch for product {productId}.");
            Assert.AreEqual(1, await cart.GetQuantityAsync(productId), $"Quantity mismatch for product {productId}.");
            Assert.AreEqual(price, await cart.GetTotalAsync(productId), $"Total should equal price x 1 for product {productId}.");
        }
    }

    [TestMethod]
    public async Task WhenAddingProductWithQuantity_CartShowsQuantityAndTotal()
    {
        const int quantity = 4;
        var details = new ProductDetailsPage(Page!);
        await details.GotoAsync("1");

        await details.SetQuantityAsync(quantity);
        await details.AddToCartAsync();
        Assert.IsTrue(await details.CartModal.IsVisibleAsync(), "Expected the 'Added to cart' confirmation modal to appear.");
        await details.CartModal.ViewCartAsync();

        var cart = new CartPage(Page!);
        Assert.AreEqual(quantity, await cart.GetQuantityAsync("1"));
        Assert.AreEqual(await cart.GetPriceAsync("1") * quantity, await cart.GetTotalAsync("1"), "Total should equal price x quantity.");
    }

    [TestMethod]
    public async Task WhenAddingSameProductTwice_QuantityIsIncremented()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();
        var productId = (await home.GetAllProductIdsAsync()).First();

        for (var i = 0; i < 2; i++)
        {
            await home.AddProductToCartByIdAsync(productId);
            Assert.IsTrue(await home.IsAddToCartConfirmationVisibleAsync(), "Expected the 'Added to cart' confirmation modal to appear.");
            await home.ClickOnContinueShoppingAsync();
        }

        var cart = new CartPage(Page!);
        await cart.GotoAsync();
        CollectionAssert.AreEqual(new[] { productId }, await cart.GetProductIdsAsync(), "Expected a single cart line, not a duplicate row.");
        Assert.AreEqual(2, await cart.GetQuantityAsync(productId));
    }

    [TestMethod]
    public async Task WhenRemovingProduct_OnlyThatProductIsRemoved()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();
        var productIds = (await home.GetAllProductIdsAsync()).Take(2).ToList();
        foreach (var productId in productIds)
        {
            await home.AddProductToCartByIdAsync(productId);
            Assert.IsTrue(await home.IsAddToCartConfirmationVisibleAsync());
            await home.ClickOnContinueShoppingAsync();
        }

        var cart = new CartPage(Page!);
        await cart.GotoAsync();
        await cart.RemoveProductAsync(productIds[0]);

        CollectionAssert.AreEqual(new[] { productIds[1] }, await cart.GetProductIdsAsync(), "Only the removed product should be gone.");
    }

    [TestMethod]
    public async Task WhenRemovingLastProduct_EmptyCartMessageIsShown()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();
        var productId = (await home.GetAllProductIdsAsync()).First();
        await home.AddProductToCartByIdAsync(productId);
        Assert.IsTrue(await home.IsAddToCartConfirmationVisibleAsync());
        await home.CartModal.ViewCartAsync();

        var cart = new CartPage(Page!);
        await cart.RemoveProductAsync(productId);

        Assert.IsTrue(await cart.IsEmptyCartMessageVisibleAsync(), "Expected the 'Cart is empty!' message after removing the last product.");
    }

    [TestMethod]
    public async Task WhenGuestProceedsToCheckout_PromptedToRegisterOrLogin()
    {
        var home = new HomePage(Page!);
        await home.GotoAsync();
        await home.AddProductToCartByIdAsync((await home.GetAllProductIdsAsync()).First());
        Assert.IsTrue(await home.IsAddToCartConfirmationVisibleAsync());
        await home.CartModal.ViewCartAsync();

        var cart = new CartPage(Page!);
        await cart.ProceedToCheckoutAsync();

        Assert.IsTrue(await cart.IsCheckoutModalVisibleAsync(), "Expected guests to be prompted to register/login before checkout.");
        await cart.CheckoutModalRegisterLoginLink.ClickAsync();

        var login = new LoginPage(Page!);
        Assert.EndsWith("/login", login.Url);
    }

    [TestMethod]
    public async Task WhenGuestCartThenLogsIn_CartContentsArePreserved()
    {
        var user = await AccountApi.CreateUserAsync();

        try
        {
            var products = new ProductsPage(Page!);
            await products.GotoAsync();
            await products.SearchAsync("jeans");
            var addedIds = await products.Products.GetProductIdsAsync();
            foreach (var productId in addedIds)
            {
                await products.Products.AddToCartAsync(productId);
                Assert.IsTrue(await products.CartModal.IsVisibleAsync());
                await products.CartModal.ContinueShoppingAsync();
            }

            var login = new LoginPage(Page!);
            await login.GotoAsync();
            await login.LoginAsync(user.Email, user.Password);
            Assert.AreEqual(user.Name, await login.Header.GetLoggedInUserNameAsync());

            var cart = new CartPage(Page!);
            await cart.GotoAsync();
            CollectionAssert.AreEquivalent(addedIds, await cart.GetProductIdsAsync(), "Guest cart should carry over after login.");
        }
        finally
        {
            await AccountApi.DeleteUserAsync(user);
        }
    }

    [TestMethod]
    public async Task WhenSubscribingFromCartPage_ShowsSuccessMessage()
    {
        var cart = new CartPage(Page!);
        await cart.GotoAsync();

        await cart.Subscription.SubscribeAsync(TestDataFactory.CreateUniqueEmail());

        Assert.IsTrue(await cart.Subscription.IsSuccessMessageVisibleAsync(), "Expected the subscription success alert to appear.");
    }
}
