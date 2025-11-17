using Microsoft.AspNetCore.Mvc;
using Web.Api.ElasticDatabase;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElasticManagementController(
    IElasticDbContext elasticDbContext) : ControllerBase
{
    private readonly IElasticDbContext _elasticDbContext = elasticDbContext;

    [HttpPost("create-index")]
    public async Task<IActionResult> CreateIndex(string indexName, CancellationToken cancellationToken)
    {
        var response = await _elasticDbContext.CreateIndexIfNotExistsAsync(indexName, cancellationToken);

        return response 
            ? Ok(new { Message = $"Index {indexName} created or already exists." })
            : BadRequest($"Fail to create Index '{indexName}'");
    }

    [HttpPost("remove-index")]
    public async Task<IActionResult> RemoveIndex(string indexName, CancellationToken cancellationToken)
    {
        var response = await _elasticDbContext.RemoveIndexAsync(indexName, cancellationToken);

        return response 
            ? Ok(new { Message = $"Index {indexName} deleted successfully." })
            : BadRequest($"Fail to delete Index '{indexName}'");
    }

    [HttpGet("get-all/{indexName}")]
    public async Task<IActionResult> GetAllDocs(
        string indexName,
        CancellationToken cancellationToken)
    {
        var documents = await _elasticDbContext.GetAllDocsAsync<dynamic>(indexName, cancellationToken);
        return Ok(documents);
    }

    //[HttpGet("get-by-key/{indexName}/{id}")]
    //public async Task<IActionResult> GetDocById(
    //    string indexName,
    //    string id,
    //    CancellationToken cancellationToken)
    //{
    //    var document = await _elasticDbContext.GetDocByKeyAsync<dynamic>(id, indexName, cancellationToken);
    //    return document is not null
    //        ? Ok(document)
    //        : NotFound(new { Message = $"Document not found from Index '{indexName}'." });
    //}

    [HttpPost("upsert/{indexName}")]
    public async Task<IActionResult> UpSertUser(
        [FromRoute] string indexName,
        [FromBody] dynamic document,
        CancellationToken cancellationToken)
    {
        var result = await _elasticDbContext.AddOrUpdateDocAsync(document, indexName, cancellationToken);

        return result == true ?
            Ok(new { Message = "Document added/updated successfully." })
            : StatusCode(500, new { Message = "Failed to add/update document for Index '{indexName}'." });
    }

    //[HttpDelete("remove/{indexName}/{id}")]
    //public async Task<IActionResult> DeleteById(
    //    string indexName,
    //    string id,
    //    CancellationToken cancellationToken)
    //{
    //    var result = await _elasticDbContext.RemoveDocByKeyAsync(id, indexName, cancellationToken);
    //    return result
    //        ? Ok(new { Message = "Document deleted successfully." })
    //        : NotFound(new { Message = "Document for Index '{indexName}' not found." });
    //}

    [HttpDelete("remove-all/{indexName}")]
    public async Task<IActionResult> DeleteAllIndexData(
        string indexName,
        CancellationToken cancellationToken)
    {
        var result = await _elasticDbContext.RemoveAllIndexDataAsync<dynamic>(indexName, cancellationToken);
        return result > 0
            ? Ok(new { Message = "All Documents for Index '{indexName}' deleted successfully." })
            : NotFound(new { Message = "Error on Delete all Documents for Index '{indexName}." });
    }
}
