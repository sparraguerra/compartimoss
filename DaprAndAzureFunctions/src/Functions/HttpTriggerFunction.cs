using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using System.Text.Json;
using Microsoft.Azure.Functions.Extensions.Dapr.Core;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;

namespace DaprAndAzureFunctions.Functions;


public class HttpTriggerFunction
{
    private readonly ILogger logger;

    public HttpTriggerFunction(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<HttpTriggerFunction>();
    }

    [FunctionName(nameof(SendMessageToQueue))]
    [OpenApiOperation(operationId: "SendMessageToQueue", tags: new[] { "SendMessageToQueue" })]
    [OpenApiSecurity("code", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    public async Task<IActionResult> SendMessageToQueue(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [DaprBinding(BindingName = "storage-queue", Operation = "create")] IAsyncCollector<JsonElement> messages)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        string? name = req.Query["name"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        name ??= data?.name;

        string responseMessage = string.IsNullOrEmpty(name)
            ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            : $"Hello, {name}. This HTTP triggered function executed successfully.";

        var deserialized = JsonConvert.DeserializeObject<MyDocument>(requestBody);

        var message = new
        {
            data = new
            {
                deserialized?.Id,
                deserialized?.Text
            }
        };

        var jsonDocument = System.Text.Json.JsonSerializer.SerializeToDocument(message);
        await messages.AddAsync(jsonDocument.RootElement);

        return new OkObjectResult(responseMessage);
    }
}
