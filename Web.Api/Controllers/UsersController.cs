using Microsoft.AspNetCore.Mvc;
using Web.Api.ElasticServices;
using Web.Api.Models;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IElasticService<User> _elasticService;

    public UsersController(ILogger<UsersController> logger,
        IElasticService<User> elasticService)
    {
        _logger = logger;
        _elasticService = elasticService;
        _elasticService.SetIndexName("Users");
    }

    [HttpPost("create-index")]
    public async Task<IActionResult> CreateIndex(string indexName, CancellationToken cancellationToken)
    {
        await _elasticService.CreateIndexIfNotExistsAsync(indexName, cancellationToken);

        return Ok(new { Message = $"Index {indexName} created or already exists." });
    }

    [HttpPost("upsert-user")]
    public async Task<IActionResult> UpSertUser([FromBody] User user,
                                             CancellationToken cancellationToken)
    {
        var result = await _elasticService.AddOrUpdateAsync(user, cancellationToken);

        return result == true ?
            Ok(new { Message = "User added/updated successfully." })
            : StatusCode(500, new { Message = "Failed to add/update user." });
    }

    [HttpGet("get-user/{id}")]
    public async Task<IActionResult> GetUserById(string id,
                                                 CancellationToken cancellationToken)
    {
        var user = await _elasticService.GetByKeyAsync(id, cancellationToken);
        return user is not null
            ? Ok(user)
            : NotFound(new { Message = "User not found." });
    }

    [HttpGet("get-all-users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _elasticService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpDelete("delete-user/{id}")]
    public async Task<IActionResult> DeleteUserById(string id,
                                                    CancellationToken cancellationToken)
    {
        var result = await _elasticService.RemoveByKeyAsync(id, cancellationToken);
        return result
            ? Ok(new { Message = "User deleted successfully." })
            : NotFound(new { Message = "User not found." });
    }
}
