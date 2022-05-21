using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureFunctionsAzureWebPubSub
{
    public static class AwpsFunctions
    { 
        [FunctionName(nameof(Negotiate))]
        public static WebPubSubConnection Negotiate([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
                                                    ILogger log,
                                                    [WebPubSubConnection(Hub = "CompartimossFunctionHub", UserId = "{query.userid}")] WebPubSubConnection connection)
        {
            log.LogInformation("Connecting...");
            return connection;
        }

        [FunctionName(nameof(WebPubSubTrigger))]
        public static void WebPubSubTrigger([WebPubSubTrigger("CompartimossFunctionHub", WebPubSubEventType.User, "message")]
                                            UserEventRequest request,
                                            WebPubSubConnectionContext context,
                                            string data,
                                            WebPubSubDataType dataType)
        {
            Console.WriteLine($"Request from: {context.UserId}");
            Console.WriteLine($"Request message data: {data}");
            Console.WriteLine($"Request message dataType: {dataType}");
        }

        [FunctionName(nameof(WebPubSubTriggerWithReturnValue))]
        public static WebPubSubEventResponse WebPubSubTriggerWithReturnValue([WebPubSubTrigger("CompartimossFunctionHub", WebPubSubEventType.User, "message")]
                                            UserEventRequest request,
                                            WebPubSubConnectionContext context,
                                            string data,
                                            WebPubSubDataType dataType)
        {
            return new UserEventResponse
            {
                Data = BinaryData.FromString("ack"),
                DataType = WebPubSubDataType.Text
            };
        }

        [FunctionName("WebPubSubOutputBinding")]
         public static async Task RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
                                           [WebPubSub(Hub = "CompartimossFunctionHub")] IAsyncCollector<WebPubSubAction> actions)
        {
            await actions.AddAsync(WebPubSubAction.CreateSendToAllAction("Hola Compartimoss!!!", WebPubSubDataType.Text));
        }
    }
}
