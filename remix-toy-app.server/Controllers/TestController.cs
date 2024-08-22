using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ReactPWA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        const string cachekey = "TestValue";

        private IMemoryCache _memoryCache;

        public TestController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public string GetTestValue()
        {
            var cacheData = _memoryCache.Get<string>(cachekey);
            return cacheData ?? string.Empty;
        }

        [HttpPost]
        public void SaveTestValue([FromBody] SaveTestValueModel value)
        {
            var expirationTime = new TimeSpan(0, 10, 0);
            _memoryCache.Set(cachekey, value.Value, new MemoryCacheEntryOptions(){SlidingExpiration = expirationTime});
        }
    }
}
