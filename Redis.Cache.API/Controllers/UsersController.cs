using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Redis.Cache.API.Models;

namespace Redis.Cache.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private string filePath = @$"{Directory.GetCurrentDirectory()}/Data/mock-data.json";

        public UsersController(IDistributedCache  distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet("store-cache")]
        public  async Task<IActionResult> StoreRedisCache()
        {
            //check if data is stored in redis cache
            var cacheData = await _distributedCache.GetStringAsync("UsersData");

            //if cache is null
            if(cacheData == null)
            {
                //read from mock data file
                var data = System.IO.File.ReadAllText(filePath);

                //set expiration data
                var expireDate = DateTime.Now.AddDays(1);
                var expDateSeconds = expireDate.Subtract(DateTime.Now).TotalSeconds;

                //init redis caching
                var distributedCacheEntryOptions = new DistributedCacheEntryOptions();
                distributedCacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expDateSeconds);
                distributedCacheEntryOptions.SlidingExpiration = null;

                await _distributedCache.SetStringAsync("UsersData", data, distributedCacheEntryOptions);

                return Ok(data);
            }
            var userData = JsonConvert.DeserializeObject<List<UserModel>>(cacheData);

            return Ok(userData);
        }

        [HttpGet("get-cache")]
        public async Task<IActionResult> GetRedisCache()
        {
            //get data from cache
            var cacheData = await _distributedCache.GetStringAsync("UsersData");
            var userData = JsonConvert.DeserializeObject<List<UserModel>>(cacheData);

            return Ok(userData);
        }
    }
}
