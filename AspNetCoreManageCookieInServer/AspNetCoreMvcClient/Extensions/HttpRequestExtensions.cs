using Microsoft.AspNetCore.Http;

namespace AspNetCoreMvcClient.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Retrieves the IP of client
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetClientIP(this HttpRequest request)
        {
            try
            {
                const string X_Forwarded_For = "X-Forwarded-For";
                const string X_Original_For = "X-Original-For";

                if (!string.IsNullOrWhiteSpace(request.Headers[X_Forwarded_For]))
                {
                    return request.Headers[X_Forwarded_For].ToString().Split(',')[0];
                }
                if (!string.IsNullOrWhiteSpace(request.Headers[X_Original_For]))
                {
                    return request.Headers[X_Original_For].ToString().Split(',')[0];
                }
                return request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
