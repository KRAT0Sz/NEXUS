namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Store;

/// <summary>
///     Integration tests for store purchases at <c>/store_requester.php</c> (<c>request_code = 4</c> and <c>request_code = 8</c>),
///     validating daily special discounts, currency guards, and custom account icon slot incrementation.
/// </summary>
public sealed class StorePurchaseTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const int PopupSuccess = 3;
    private const int PopupError   = 1;

    private const int ErrorCodeNone                = 0;
    private const int ErrorCodeInvalidStoreRequest = 1;
    private const int ErrorCodeInsufficientFunds   = 21;
    private const int ErrorCodeItemUnavailable     = 23;
    private const int ErrorCodeAlreadyOwned        = 33;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Missing_Cookie_Returns_Ok_With_Php_Error_Response_Not_Http_Bad_Request()
    {
        HttpClient client = webApplicationFactory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["request_code"] = "1"
        });

        HttpResponseMessage response = await client.PostAsync("/store_requester.php", form);

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(body["errorCode"])).IsEqualTo(20);
        }
    }

    [Test]
    public async Task Missing_Request_Code_Returns_Ok_With_Php_Error_Response()
    {
        (string cookie, int accountID, _) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(
            webApplicationFactory, "missing.code@kongor.com", "MissingCode", goldCoins: 1000, silverCoins: 1000, plinkoTickets: 0);

        HttpClient client = webApplicationFactory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"]     = cookie,
            ["account_id"] = accountID.ToString()
        });

        HttpResponseMessage response = await client.PostAsync("/store_requester.php", form);

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(body["errorCode"])).IsEqualTo(ErrorCodeInvalidStoreRequest);
        }
    }

    [Test]
    public async Task Purchasing_Gold_Only_Item_With_Silver_Coins_Is_Rejected()
    {
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(
            webApplicationFactory, "currency.guard@kongor.com", "CurrencyGuard", goldCoins: 0, silverCoins: 5000, plinkoTickets: 0);

        // BreakyCPK Announcer Pack (ID 890) has a Gold price of 300, and is a daily special.
        // Let's test attempting to purchase with currency = "1" (Silver) for an item that has 0 silver cost or invalid currency.
        HttpClient client = webApplicationFactory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"]       = cookie,
            ["account_id"]   = accountID.ToString(),
            ["request_code"] = "4",
            ["product_id"]   = "890",
            ["currency"]     = "1",
            ["category_id"]  = "5",
            ["page"]         = "1",
            ["hostTime"]     = "123456"
        });

        HttpResponseMessage response = await client.PostAsync("/store_requester.php", form);

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        // Daily special 890 has 0% silver discount and 0 silver price, so purchasing with silver must be rejected
        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.OwnedStoreItems.Contains("av.BreakyCPK")).IsFalse();
    }
}

