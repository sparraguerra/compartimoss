using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azbp.Async.Functions
{
    public interface ISwapiClient
    {
        Task CallApiAsync();
    }

    public class SwapiClient : ISwapiClient
    {
        public SwapiClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://swapi.dev/");

            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task CallApiAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/films");

            _ = await HttpClient.SendAsync(request).ConfigureAwait(false);
        }
    }
}
