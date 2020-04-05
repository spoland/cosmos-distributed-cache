using cosmos_distributed_cache;
using cosmos_distributed_cache.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace cosmos_distributed_cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly JsonPlaceholderClient _httpClient;
        private readonly ILogger _logger;

        public PostsController(JsonPlaceholderClient httpClient, ILogger<PostsController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetAllPosts()
        {
            var sw = new Stopwatch();
            sw.Start();

            var data = await _httpClient.Get();
            
            sw.Stop();            
            _logger.LogInformation($"Request duration: {sw.ElapsedMilliseconds}ms");

            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var sw = new Stopwatch();
            sw.Start();

            var data = await _httpClient.Get(id);

            sw.Stop();
            _logger.LogInformation($"Request duration: {sw.ElapsedMilliseconds}ms");

            return Ok(data);
        }
    }
}