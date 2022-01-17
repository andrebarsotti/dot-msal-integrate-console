using Microsoft.Identity.Client;

var pca = PublicClientApplicationBuilder.CreateWithApplicationOptions(Config.PublicClientApplicationOptions)
                                        .Build();

TokenCacheHelper.EnableSerialization(pca.UserTokenCache);

var accounts = await pca.GetAccountsAsync();
Console.WriteLine($"Number of Accounts => {accounts.Count()}");

foreach (var item in accounts)
{
    Console.WriteLine($"UserName => {item.Username}");
    Console.WriteLine();
}

AuthenticationResult result;
try
{
    Console.WriteLine("Tentando pegar o token do Cache");
    result = await pca.AcquireTokenSilent(Config.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    Console.WriteLine("Adquirindo novo token.");
    Console.WriteLine();

    if (Config.UseDeviceCodeFlow)
        result = await pca.AcquireTokenWithDeviceCode(Config.Scopes,
            deviceCodeResult =>
            {
                Console.WriteLine(deviceCodeResult.Message);
                return Task.FromResult(0);
            }).ExecuteAsync();
    else
        result = await pca.AcquireTokenInteractive(Config.Scopes)
                          .ExecuteAsync();
}

if (result is not null)
    await ChamarApiDeCatalogDoBingAds(result.AccessToken);


async Task ChamarApiDeCatalogDoBingAds(string accessToken)
{
    HttpClient client = new();

    client.BaseAddress = new Uri("https://content.api.bingads.microsoft.com/");
    client.DefaultRequestHeaders.Add("AuthenticationToken", accessToken);
    client.DefaultRequestHeaders.Add("DeveloperToken", Config.DeveloperToken);
    client.DefaultRequestHeaders.Add("CustomerId", Config.CustomerId);

    var result = await client.GetAsync($"shopping/v9.1/bmc/{Config.MerchantId}/catalogs");

    result.EnsureSuccessStatusCode();

    Console.WriteLine(await result.Content.ReadAsStringAsync());
}
