using Demo.ApiGateway.Configuration;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Demo.ApiGateway.Providers;

public class DaprTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        if (context.Route.Metadata?.TryGetValue(DaprYarpConstants.MetaKeys.DaprEnabled, out string? daprEnabled) ?? false)
        { 
            if (string.IsNullOrWhiteSpace(daprEnabled))
            {
                throw new ArgumentException("A non empty DaprEnabled value is required");
            }

            if (!bool.TryParse(daprEnabled, out bool enabled))
            {
                throw new ArgumentException("A valid DaprEnabled value is required");
            }

            if (enabled)
            {
                if (context.Route.Metadata?.TryGetValue(DaprYarpConstants.MetaKeys.DaprAppId, out string? appId) ?? false)
                {
                    if (string.IsNullOrWhiteSpace(appId))
                    {
                        throw new ArgumentException("A valid Dapr AppId value is required");
                    }
                    context.AddRequestTransform(transformContext =>
                    {
                        transformContext.ProxyRequest.Headers.Add("dapr-app-id", appId);
                        transformContext.ProxyRequest.RequestUri = 
                                        new Uri($"{transformContext.DestinationPrefix}{transformContext.Path.Value!}{transformContext.Query.QueryString.Value}");
                        return ValueTask.CompletedTask;
                    });
                }
            }
        }

    }
}