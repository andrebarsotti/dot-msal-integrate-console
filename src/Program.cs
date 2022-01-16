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
{
    Console.WriteLine($"AccessToken => {result.AccessToken};");
    Console.WriteLine($"TokenType => {result.TokenType};");
}
