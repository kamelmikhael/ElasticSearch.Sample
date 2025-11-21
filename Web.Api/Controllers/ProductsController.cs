using Elastic.Clients.Elasticsearch;
using Infrastructure.Database.Data;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Models;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IElasticDbSet<Product> elasticService) : ControllerBase
    {
        private readonly IElasticDbSet<Product> _elasticService = elasticService;

        [HttpPost("add")]
        public async Task<IActionResult> AddProduct(
            [FromBody] Product product,
            CancellationToken cancellationToken)
        {
            var result = await _elasticService.AddOrUpdateAsync(product, cancellationToken);

            return result == true ?
                Ok(new { Message = "Product added/updated successfully." })
                : StatusCode(500, new { Message = "Failed to add/update product." });
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _elasticService.GetAllAsync(cancellationToken);
            return Ok(response);
        }
    }
}
