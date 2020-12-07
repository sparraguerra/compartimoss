using AspNetCoreMvcClient.Data;
using AspNetCoreMvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AspNetCoreMvcClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDistributedCache distributedCache;
        private readonly DataProtectionKeysContext dbContext;

        public HomeController(IDistributedCache distributedCache, DataProtectionKeysContext dbContext)
        {
            this.distributedCache = distributedCache;
            this.dbContext = dbContext;
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

        public async Task<IActionResult> AddCacheItem()
        {
            string key = "customerKey:1";
            var customer = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "Test Name",
                Surname = "Test Surname",
                BirthDay = DateTime.UtcNow.AddYears(-18)
            };
            await dbContext.AddAsync<Customer>(customer);
            await dbContext.SaveChangesAsync();

            await distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(customer));

            return Ok("success");
        }

        public async Task<IActionResult> GetCacheItem()
        {
            string key = "customerKey:1";
            var cachedItem = await distributedCache.GetStringAsync(key);

            if (!string.IsNullOrWhiteSpace(cachedItem))
            {
                return Ok(JsonConvert.DeserializeObject<Customer>(cachedItem));
            }
            else
            {
                var customer = await dbContext.Customers.FirstOrDefaultAsync(t => t.Name == "Test Name");
                await distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(customer));

                return Ok(customer);
            }
        }

        public async Task<IActionResult> DeleteCacheItem()
        {
            string key = "customerKey:1";
            await distributedCache.RemoveAsync(key);
            return Ok("success");
        }
    }
}
