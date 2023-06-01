using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaprAndAzureFunctions.Services;

public class DaprOutputBindingService : IDaprOutputBindingService
{
    private readonly DaprClient client;
    private readonly ILogger<DaprOutputBindingService> logger;

    public DaprOutputBindingService(DaprClient client, ILogger<DaprOutputBindingService> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task PublishMessageAsync<T>(T message, string bindingName) =>
        await PublishMessageAsync(message, default, bindingName);

    public async Task PublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName) =>
       await PublishMessageAsync(message, "create", metadata, bindingName);

    public async Task PublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName)
    {
        logger.LogInformation("Publishing message {Message} to {BindingName}", message, bindingName);

        await client.InvokeBindingAsync(bindingName, operation, message, metadata);
    }

    public async Task TryPublishMessageAsync<T>(T message, string bindingName) =>
        await TryPublishMessageAsync(message, default, bindingName);

    public async Task TryPublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName) =>
       await TryPublishMessageAsync(message, "create", metadata, bindingName);

    public async Task TryPublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName)
    {
        try
        {
            await PublishMessageAsync(message, operation, metadata, bindingName);
        }
        catch (Exception ex)
        {
            logger.LogError("Error publishing message {Message} to {BindingName}: {MessageException}", message, bindingName, ex.Message);
        }
    }

}
