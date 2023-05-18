namespace DaprAndAzureNotificationHub;

public static class CoreConstants
{
    public const string PUSH_NOTIFICATION_TAGS_HEADER_NAME = "ServiceBusNotification-Tags";
    public const string PUSH_NOTIFICATION_FORMAT_HEADER_NAME = "ServiceBusNotification-Format";
    public const string PUSH_NOTIFICATION_FORMAT_HEADER_VALUE = "template";
    public const string PUSH_NOTIFICATION_URI_PATH = "/messages/?api-version=2015-01";
    public const string BINDING_NAME_HTTP_PUSH_NOTIFICATIONS = "http-push-notifications";

    public const string AUTHORIZATION_HEADER_NAME = "Authorization";
    public const string CONTENT_TYPE_HEADER_NAME = "Content-Type";
    public const string JSON_UTF8 = "application/json;charset=utf-8";
}
