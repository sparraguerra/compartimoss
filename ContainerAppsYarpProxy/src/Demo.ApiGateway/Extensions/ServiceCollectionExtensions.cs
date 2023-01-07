using Demo.ApiGateway.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace Demo.ApiGateway.Extensions;

public static class ServiceCollectionextensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(nameof(CorsOptions)).Get<CorsOptions>();

        return services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(corsSettings?.Origins ?? new[] { "*" });
            });
        });
    }

    public static IServiceCollection AddAuthenticationDefault(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddMicrosoftIdentityWebApi(options =>
        {
            configuration.Bind("AzureAd", options);
            
        },
        options => { configuration.Bind("AzureAd", options); });


        return services;
    }
}
