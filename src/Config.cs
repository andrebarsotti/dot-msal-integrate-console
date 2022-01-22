using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

internal static class Config
{
    private static IConfiguration? _config;
    private static PublicClientApplicationOptions? _publicClientApplicationOptions;
    private static string[]? _scopes;
    private static string? _encryptKey;
    private static string? _developerToken;
    private static string? _customerId;
    private static string? _merchantId;
    private static AuthenticationFlow? _authenticationFlow;
    private static bool? _useEncriptedCache;
    private static string? _secretValue;
    private static bool? _useConfidentialAppFlow;
    private static string? _clientId;
    private static string? _authority;
    private static string? _baseAddres;
    private static string? _urlPath;

    public static IConfiguration Configuration
    {
        get
        {
            if (_config is null)
                SetConfig();

            return _config!;
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

    public static AuthenticationFlow AuthenticationFlow => _authenticationFlow ??= (GetConfig<AuthenticationFlow?>(nameof(AuthenticationFlow))
                                                                                  ?? AuthenticationFlow.DeviceCodeFlow);

    public static bool UseEncriptedCache => _useEncriptedCache ??= (GetConfig<bool?>(nameof(UseEncriptedCache)) ?? true);

    public static string DeveloperToken => _developerToken ??= GetConfig<string>(nameof(DeveloperToken));

    public static string CustomerId => _customerId ??= GetConfig<string>(nameof(CustomerId));

    public static string MerchantId => _merchantId ??= GetConfig<string>(nameof(MerchantId));

    public static bool UseConfidentialAppFlow => _useConfidentialAppFlow ??= (GetConfig<bool?>(nameof(UseConfidentialAppFlow)) ?? false);

    public static string SecretValue => _secretValue ??= GetConfig<string>(nameof(SecretValue));

    public static string ClienteId => _clientId ??= GetConfig<string>("AzureAd:ClientId");

    public static string Authority => _authority ??= $"{Config.GetConfig<string>("AzureAd:Instance")}{Config.GetConfig<string>("AzureAd:TenantId")}";

    public static string BaseAddress => _baseAddres ??= Config.GetConfig<string>(nameof(BaseAddress));

    public static string UrlPath => _urlPath ??= Config.GetConfig<string>(nameof(UrlPath));
}
