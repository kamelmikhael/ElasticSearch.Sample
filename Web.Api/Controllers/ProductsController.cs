using Microsoft.AspNetCore.Mvc;
using Web.Api.ElasticDatabaseV2;
using Web.Api.Models;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IElasticService<Product> _elasticService;

        public ProductsController(IElasticService<Product> elasticService)
        {
            _elasticService = elasticService;
        }

        [HttpGet("seed-data")]
        public IEnumerable<Product> SeedData()
        {
            var searchResponse = _elasticService.SeedData();

            return searchResponse;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            (bool isvalid, string result) = await _elasticService.UpSertDocAsync(product);
            return isvalid ? Ok(result) : BadRequest("Error happen when Update/Insert document");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var response = await _elasticService.GetAll(keyword);
            return Ok(response);
        }
    }
}
