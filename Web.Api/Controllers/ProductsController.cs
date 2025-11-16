using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using Web.Api.Models;
using Web.Api.Settings;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IElasticClient _elasticClient;
        private readonly ElasticSettings _elasticSettings;

        public ProductsController(ILogger<ProductsController> logger
            , IOptions<ElasticSettings> elasticSettingsOptions)
        {
            _logger = logger;
            _elasticSettings = elasticSettingsOptions.Value
                ?? throw new ArgumentNullException("Elastic Settings not found");

            var settings = new ConnectionSettings(new Uri(_elasticSettings.Url))
                .DefaultIndex(_elasticSettings.DefaultIndex);

            _elasticClient = new ElasticClient(settings);
        }

        [HttpGet("seed-data")]
        public IEnumerable<Product> SeedData()
        {
            // Index (Insert) document
            var product = new Product { Id = 1, Name = "Laptop", Price = 1200 };
            _elasticClient.IndexDocument(product);

            // Search
            var searchResponse = _elasticClient.Search<Product>(s => s
                .Query(q => q.Match(m => m.Field(f => f.Name).Query("laptop"))));

            return searchResponse.Hits.Select(x => x.Source).ToList();
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            var response = await _elasticClient.IndexDocumentAsync(product);
            return Ok(response.Result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var response = await _elasticClient.SearchAsync<Product>(s => s
                .Query(q => q.Match(m => m.Field(f => f.Name).Query(keyword))));
            return Ok(response.Documents);
        }
    }
}
