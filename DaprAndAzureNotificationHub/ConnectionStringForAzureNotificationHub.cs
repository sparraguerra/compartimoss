namespace DaprAndAzureNotificationHub;

public class ConnectionStringForAzureNotificationHub
{
    public ConnectionStringForAzureNotificationHub(string endpoint, string hubName, string sasKeyName, string sasKeyValue)
    {
        Endpoint = endpoint;
        HubName = hubName;
        SasKeyName = sasKeyName;
        SasKeyValue = sasKeyValue;
    }

    public string Endpoint { get; set; } = string.Empty;
    public string HubName { get; set; } = string.Empty;
    public string SasKeyName { get; set; } = string.Empty;
    public string SasKeyValue { get; set; } = string.Empty;
    public string GetNotificationHubUri() => $"{Endpoint}{HubName}{CoreConstants.PUSH_NOTIFICATION_URI_PATH}";
}
