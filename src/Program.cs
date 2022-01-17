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
    Console.WriteLine("Tentando pegar o token do Cache...");
    result = await pca.AcquireTokenSilent(Config.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
    Console.WriteLine("Token recuperado do cache...");
}
catch (MsalUiRequiredException)
{
    Console.WriteLine("Adquirindo novo token...");
    Console.WriteLine();

    switch (Config.AuthenticationFlow)
    {
        case AuthenticationFlow.DeviceCodeFlow:
            result = await pca.AcquireTokenWithDeviceCode(Config.Scopes,
                deviceCodeResult =>
                {
                    Console.WriteLine(deviceCodeResult.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync();
            break;
        case AuthenticationFlow.InteractiveFlow:
            result = await pca.AcquireTokenInteractive(Config.Scopes)
                              .ExecuteAsync();
            break;
        case AuthenticationFlow.WindowsIntegrated:
            try{
                result = await pca.AcquireTokenByIntegratedWindowsAuth(Config.Scopes)
                                .ExecuteAsync();
            }
            catch(PlatformNotSupportedException)
            {
                result = await pca.AcquireTokenWithDeviceCode(Config.Scopes,
                    deviceCodeResult =>
                    {
                        Console.WriteLine(deviceCodeResult.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();
            }
            break;
        default:
            result = null!;
            break;
    }

    Console.WriteLine("Novo token adquirido.");
}
Console.WriteLine();

if (result is not null)
    await ChamarApiDeCatalogDoBingAds(result.AccessToken);


async Task ChamarApiDeCatalogDoBingAds(string accessToken)
{
    HttpClient client = new();

    client.BaseAddress = new Uri("https://content.api.bingads.microsoft.com/");
    client.DefaultRequestHeaders.Add("AuthenticationToken", accessToken);
    client.DefaultRequestHeaders.Add("DeveloperToken", Config.DeveloperToken);
    client.DefaultRequestHeaders.Add("CustomerId", Config.CustomerId);

    Console.WriteLine("Buscando catalogo no BingAds...");
    var result = await client.GetAsync($"shopping/v9.1/bmc/{Config.MerchantId}/catalogs");

    result.EnsureSuccessStatusCode();

    Console.WriteLine("Encontrado o catalogo no BingAds:");
    Console.WriteLine(await result.Content.ReadAsStringAsync());
    Console.WriteLine("");
}
