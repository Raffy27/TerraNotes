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

    // This will get the api ket from the http context and use it to find the user
    [HttpGet("Me")]
    [ServiceFilter(typeof(APIKeyAuthFilter))]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        // Obtain the user from the context
        var apiKey = HttpContext.Items["APIKey"] as APIKey;

        // Find the user in the database
        var user = await _context.Users.FindAsync(apiKey!.UserCreatedId);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }
}