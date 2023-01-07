using Demo.ApiGateway.Configuration;
using Demo.ApiGateway.Middleware;
using Demo.ApiGateway.Providers;

namespace Demo.ApiGateway.Extensions;

public static class ReverseProxyExtensions
{
    public static void AddReverseProxy(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddReverseProxy()
                        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                        .AddTransforms<DaprTransformProvider>();

        builder.Services.AddAuthenticationDefault(configuration);
        builder.Services.AddAuthorization()
                        .AddCorsPolicy(configuration)
                        .AddHttpContextAccessor();

        builder.Services.AddSingleton(configuration);
        builder.Services.AddHealthChecks();                        
    }

    private static void UseYarp(this WebApplication app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/hc",
                    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => true });

            endpoints.MapHealthChecks("/liveness",
                new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            endpoints.MapReverseProxy(pipeline =>
            {
                pipeline.UseReverseProxyPipeline();
            });
        });
    }

    public static void UseReverseProxy(this WebApplication app)
    {
        app.UseRouting();
        app.UseCors("CorsPolicy");
       // app.UseAuthentication();
       // app.UseAuthorization();

        app.UseYarp();
    }

    public static IConfigurationBuilder AddDaprConfiguration(this IConfigurationBuilder configuration)
    {
        var httpEndpoint = DaprDefaults.GetDefaultHttpEndpoint();
        return configuration.AddInMemoryCollection(new[]
        {
                new KeyValuePair<string, string>("ReverseProxy:Clusters:dapr-sidecar:Destinations:d1:Address", httpEndpoint!),
            });
    }
}