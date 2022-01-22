using System.Net.Http.Headers;
using Microsoft.Identity.Client;

await Main();

async Task Main()
{
    AuthenticationResult result = Config.UseConfidentialAppFlow ? await ExecuteConfidencialClientFlow() :
                                                                  await ExecutePublicClientFlow();

    Console.WriteLine();

    if (result is not null)
        await CallMicrosoftGraphAPI(result);
}

async Task CallMicrosoftGraphAPI(AuthenticationResult authentication)
{
    HttpClient client = new();

    client.BaseAddress = new Uri(Config.BaseAddress);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authentication.TokenType,
                                                                               authentication.AccessToken);

    var result = await client.GetAsync(Config.UrlPath);

    result.EnsureSuccessStatusCode();

    Console.WriteLine(await result.Content.ReadAsStringAsync());
    Console.WriteLine("");
}

async Task<AuthenticationResult> ExecutePublicClientFlow()
{
    var clientApp = PublicClientApplicationBuilder.CreateWithApplicationOptions(Config.PublicClientApplicationOptions)
                                                  .Build();

    TokenCacheHelper.EnableSerialization(clientApp.UserTokenCache);

    var accounts = await clientApp.GetAccountsAsync();
    Console.WriteLine($"Numero de contas => {accounts.Count()}");

    foreach (var item in accounts)
    {
        Console.WriteLine($"UserName => {item.Username}");
        Console.WriteLine();
    }

    AuthenticationResult result;
    try
    {
        Console.WriteLine("Tentando recuperar o token do Cache...");
        result = await clientApp.AcquireTokenSilent(Config.Scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
        Console.WriteLine("Token recuperado do cache.");
    }
    catch (MsalUiRequiredException)
    {
        Console.WriteLine("Adquirindo novo token...");
        Console.WriteLine();

        switch (Config.AuthenticationFlow)
        {
            case AuthenticationFlow.DeviceCodeFlow:
                result = await ExecuteDeviceCodeFlow(clientApp);
                break;
            case AuthenticationFlow.InteractiveFlow:
                result = await clientApp.AcquireTokenInteractive(Config.Scopes)
                                .ExecuteAsync();
                break;
            case AuthenticationFlow.WindowsIntegrated:
                try{
                    result = await clientApp.AcquireTokenByIntegratedWindowsAuth(Config.Scopes)
                                    .ExecuteAsync();
                }
                catch(PlatformNotSupportedException)
                {
                    result = await ExecuteDeviceCodeFlow(clientApp);
                }
                break;
            default:
                result = null!;
                break;
        }

        Console.WriteLine("Novo token adquirido.");
    }
    return result;
}

async Task<AuthenticationResult> ExecuteDeviceCodeFlow(IPublicClientApplication clientApp)
 =>  await clientApp.AcquireTokenWithDeviceCode(Config.Scopes,
                    deviceCodeResult =>
                    {
                        Console.WriteLine(deviceCodeResult.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();


async Task<AuthenticationResult> ExecuteConfidencialClientFlow()
{
    var app = ConfidentialClientApplicationBuilder.Create(Config.ClienteId)
                .WithClientSecret(Config.SecretValue)
                .WithAuthority(new Uri(Config.Authority))
                .Build();

    TokenCacheHelper.EnableSerialization(app.AppTokenCache);

    AuthenticationResult result = null!;
    try
    {
        Console.WriteLine("Tentando recuperar o token do Cache ou gerá-lo...");
        result = await app.AcquireTokenForClient(Config.Scopes)
                          .ExecuteAsync();
        Console.WriteLine("Token recuperado do cache.");
    }
    catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
    {
        // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
        // Mitigation: change the scope to be as expected
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Escopo informado não é suprtado");
        Console.ResetColor();
    }

    return result;
}
