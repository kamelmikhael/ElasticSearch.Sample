using Microsoft.AspNetCore.Mvc;
using Web.Api.ElasticDatabase;
using Web.Api.Models;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IElasticDbSet<User> _userDbSet;

    public UsersController(ILogger<UsersController> logger,
        IElasticDbSet<User> userDbSet)
    {
        _logger = logger;
        _userDbSet = userDbSet;
        _userDbSet.SetIndexName("users");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userDbSet.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id,
                                                 CancellationToken cancellationToken)
    {
        var user = await _userDbSet.GetByKeyAsync(id, cancellationToken);
        return user is not null
            ? Ok(user)
            : NotFound(new { Message = "User not found." });
    }

    [HttpPost]
    public async Task<IActionResult> UpSertUser([FromBody] User user,
                                             CancellationToken cancellationToken)
    {
        var result = await _userDbSet.AddOrUpdateAsync(user, cancellationToken);

        return result == true ?
            Ok(new { Message = "User added/updated successfully." })
            : StatusCode(500, new { Message = "Failed to add/update user." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(string id,
                                                    CancellationToken cancellationToken)
    {
        var result = await _userDbSet.RemoveByKeyAsync(id, cancellationToken);
        return result
            ? Ok(new { Message = "User deleted successfully." })
            : NotFound(new { Message = "User not found." });
    }
}
