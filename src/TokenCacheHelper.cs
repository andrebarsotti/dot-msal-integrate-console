using System.Text;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

internal static class TokenCacheHelper
{
    public static void EnableSerialization(ITokenCache tokenCache)
    {
        if (Config.UseEncriptedCache)
            CustomEncryptedTextFileCache(tokenCache);
        else
            PlainTextFileCache(tokenCache);
    }

    private static void PlainTextFileCache(ITokenCache tokenCache)
    {
        var storageProperties = new StorageCreationPropertiesBuilder(CacheFileName,
                                                                     CacheDir)
                                        .WithUnprotectedFile()
                                        .Build();

        var taskCacheHelper = MsalCacheHelper.CreateAsync(storageProperties);
        taskCacheHelper.Wait();

        var cacheHelper = taskCacheHelper.Result;

        cacheHelper.RegisterCache(tokenCache);
    }

    private static void CustomEncryptedTextFileCache(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    private static string CacheFilePath => Path.Combine(CacheDir, CacheFileName);

    private static string CacheDir => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;

    private static string CacheFileName => $"{Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location)}.msalcache.bin3";

    private static readonly object FileLock = new object();

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        lock (FileLock)
        {
            byte[]? serializedToken = null;

            if (File.Exists(CacheFilePath))
            {
                using var fileStream = new System.IO.FileStream(CacheFilePath, FileMode.Open);
                using var aes = Aes.Create();
                aes.Key = Encoding.Default.GetBytes(Config.EncryptKey);

                byte[] iv = new byte[aes.IV.Length];
                int numBytesToRead = aes.IV.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                    if (n == 0) break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                using CryptoStream cryptoStream = new(fileStream,
                                                       aes.CreateDecryptor(aes.Key, iv),
                                                       CryptoStreamMode.Read);

                using StreamReader decryptReader = new(cryptoStream);
                string decryptedMessage = decryptReader.ReadToEnd();
                serializedToken = Encoding.Default.GetBytes(decryptedMessage);
            }

            args.TokenCache.DeserializeMsalV3(serializedToken);
        }
    }


    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        if (args.HasStateChanged)
        {
            lock (FileLock)
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.Default.GetBytes(Config.EncryptKey);

                byte[] iv = aes.IV;
                using FileStream fileStream = new(CacheFilePath, FileMode.OpenOrCreate);
                fileStream.Write(iv, 0, iv.Length);

                using CryptoStream cryptoStream = new(fileStream,
                                                       aes.CreateEncryptor(),
                                                       CryptoStreamMode.Write);
                using StreamWriter encryptWriter = new(cryptoStream);

                encryptWriter.Write(Encoding.Default.GetString(args.TokenCache.SerializeMsalV3()));
            }
        }
    }
}
