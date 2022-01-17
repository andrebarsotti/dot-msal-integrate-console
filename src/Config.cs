using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

internal static class Config
{
    private static IConfiguration _config;

    private static PublicClientApplicationOptions _publicClientApplicationOptions;

    private static string[] _scopes;

    private static string _encryptKey;

    public static IConfiguration Configuration
    {
        get
        {
            if (_config is null)
                SetConfig();

            return _config;
        }
    }

    private static void SetConfig()
    {
        _config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddUserSecrets("msal-integrate-console")
                        .AddEnvironmentVariables()
                        .Build();
    }

    public static T GetConfig<T>(string sectionName)
    {
        return Configuration.GetSection(sectionName).Get<T>();
    }

    public static string[] Scopes => _scopes ??= GetConfig<string[]>(nameof(Scopes));

    public static PublicClientApplicationOptions PublicClientApplicationOptions
    {
        get
        {
            if (_publicClientApplicationOptions is null)
            {
                _publicClientApplicationOptions = new();
                Configuration.Bind("AzureAd", _publicClientApplicationOptions);
            }
            return _publicClientApplicationOptions;
        }
    }

    public static string EncryptKey => _encryptKey ??= GetConfig<string>(nameof(EncryptKey));

    public static bool UseDeviceCodeFlow => GetConfig<bool?>(nameof(UseDeviceCodeFlow)) ?? true;

    public static bool UseEncriptedCache => GetConfig<bool?>(nameof(UseEncriptedCache)) ?? true;

    public static string DeveloperToken => GetConfig<string>(nameof(DeveloperToken));

    public static string CustomerId => GetConfig<string>(nameof(CustomerId));

    public static string MerchantId => GetConfig<string>(nameof(MerchantId));
}
