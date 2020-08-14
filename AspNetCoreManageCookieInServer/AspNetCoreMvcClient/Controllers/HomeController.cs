using AspNetCoreMvcClient.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCoreMvcClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor, ILogger<HomeController> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> SignOut()
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                var props = new AuthenticationProperties()
                {
                    RedirectUri = "/"
                };
                await HttpContext.SignOutAsync("cookie");
                await HttpContext.SignOutAsync("oidc", props);
            }

            foreach (var cookieKey in HttpContext.Request.Cookies.Keys)
            {
                HttpContext.Response.Cookies.Delete(cookieKey);
            }

            string endSessionUrl = string.Empty;
            var token = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            using (var client = new HttpClient())
            {
                var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
                if (disco.IsError)
                {
                    throw new InvalidOperationException(disco.Error);
                }

                var requestUrl = new RequestUrl(disco.EndSessionEndpoint);

                endSessionUrl = requestUrl.CreateEndSessionUrl(token, WebUtility.UrlEncode("https://localhost:44378/")); 
            }
                
            return new RedirectResult(endSessionUrl);
        }
    }
}
