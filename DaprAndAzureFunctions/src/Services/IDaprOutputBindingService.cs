using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaprAndAzureFunctions.Services;

public interface IDaprOutputBindingService
{
    Task PublishMessageAsync<T>(T message, string bindingName);
    Task PublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName);
    Task PublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName);
    Task TryPublishMessageAsync<T>(T message, string bindingName);
    Task TryPublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName);
    Task TryPublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName);
}
