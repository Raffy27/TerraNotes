using Microsoft.AspNetCore.Mvc;
using TerraNotes.Data;
using TerraNotes.Models;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // This will get the API Key from the http context and use it to find the user
    [HttpGet("Me")]
    [ServiceFilter(typeof(APIKeyAuthFilter))]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        // Obtain the user from the http context
        var apiKey = (HttpContext.Items["APIKey"] as APIKey)!;
        await _context.Entry(apiKey).Reference(a => a.UserCreated).LoadAsync();
        return apiKey.UserCreated;
    }
}