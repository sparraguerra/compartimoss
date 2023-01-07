namespace Demo.ApiGateway.Configuration;

internal static class DaprDefaults
{
    private static string? httpEndpoint;
    private static string? grpcEndpoint;
    private static string? apiToken;

    public static string? GetDefaultApiToken()
    {
        if (apiToken is null)
        {
            var value = Environment.GetEnvironmentVariable("DAPR_API_TOKEN");
             
            apiToken = value == string.Empty ? null : apiToken;
        }

        return apiToken;
    }

    public static string? GetDefaultHttpEndpoint()
    {
        if (httpEndpoint is null)
        {
            var port = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            port = string.IsNullOrEmpty(port) ? "3500" : port;
            httpEndpoint = $"http://127.0.0.1:{port}";
        }

        return httpEndpoint;
    }

    public static string? GetDefaultGrpcEndpoint()
    {
        if (grpcEndpoint == null)
        {
            var port = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT");
            port = string.IsNullOrEmpty(port) ? "50001" : port;
            grpcEndpoint = $"http://127.0.0.1:{port}";
        }

        return grpcEndpoint;
    }
}

public static class DaprYarpConstants
{
    public static class MetaKeys
    {
        public const string DaprEnabled = nameof(DaprEnabled);
        public const string DaprAppId = nameof(DaprAppId);
    }
}