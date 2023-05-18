using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DaprAndAzureNotificationHub;

public interface IDaprOutputBindingService
{
    Task PublishMessageAsync<T>(T message, string bindingName);
    Task PublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName);
    Task PublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName);
    Task TryPublishMessageAsync<T>(T message, string bindingName);
    Task TryPublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName);
    Task TryPublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName);

    static string GenerateSasTokenForAzureNotificationHub(string uri, int minUntilExpire, string sasKeyName, string sasKeyValue)
    {
        TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
        var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + minUntilExpire);
        string stringToSign = HttpUtility.UrlEncode(uri) + "\n" + expiry;
        HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(sasKeyValue));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        var sasToken = string.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(uri), HttpUtility.UrlEncode(signature), expiry, sasKeyName);
        return sasToken;
    }

    static ConnectionStringForAzureNotificationHub ParseConnectionStringForAzureNotificationHub(string connectionString, string hubName)
    {

        string endpoint = string.Empty;
        string sasKeyName = string.Empty;
        string sasKeyValue = string.Empty;
        char[] separator = { ';' };
        string[] parts = connectionString.Split(separator);

        if (parts.Length != 3)
        {
            throw new InvalidOperationException("Invalid ConnectionString.");
        }
        if (string.IsNullOrWhiteSpace(hubName))
        {
            throw new InvalidOperationException("Invalid HubName.");
        }

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("Endpoint"))
            {
                endpoint = "https" + parts[i][11..];
            }
            if (parts[i].StartsWith("SharedAccessKeyName"))
            {
                sasKeyName = parts[i][20..];
            }
            if (parts[i].StartsWith("SharedAccessKey"))
            {
                sasKeyValue = parts[i][16..];
            }
        }

        return new ConnectionStringForAzureNotificationHub(endpoint, hubName, sasKeyName, sasKeyValue);
    }
}
