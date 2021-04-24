using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Azbp.Monitoring.Functions
{
    public class MonitoringFunctions
    {
        private readonly TelemetryClient telemetryClient;

        public MonitoringFunctions(TelemetryConfiguration telemetryConfiguration)
        {
            telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName(nameof(MonitoringFunction))]
        public async Task<IActionResult> MonitoringFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            DateTime start = DateTime.UtcNow;
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;
            
            var evt = new EventTelemetry("Function MonitoringFunction called");
            evt.Context.User.Id = name;
            telemetryClient.TrackEvent(evt);

            telemetryClient.GetMetric("contentLength").TrackValue(req.ContentLength);

            var dependency = new DependencyTelemetry
            {
                Name = "GET api/planets/1/",
                Target = "swapi.dev",
                Data = "https://swapi.dev/api/planets/1/",
                Timestamp = start,
                Duration = DateTime.UtcNow - start,
                Success = true
            };
            dependency.Context.User.Id = name;
            telemetryClient.TrackDependency(dependency);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
