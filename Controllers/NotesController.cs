using Microsoft.AspNetCore.Mvc;
using TerraNotes.Data;
using TerraNotes.Models;

[Route("api/[controller]")]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotesController> _logger;

    public NotesController(AppDbContext context, ILogger<NotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{{id}}")]
    [ServiceFilter(typeof(APIKeyAuthFilter))]
    public async Task<ActionResult<Note>> GetNoteById(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        return note;
    }

    [HttpPost]
    [ServiceFilter(typeof(APIKeyAuthFilter))]
    public async Task<ActionResult<Note>> CreateNote()
    {
        var apiKey = (HttpContext.Items["APIKey"] as APIKey)!;

        // Check if there are enough uses left
        if (apiKey.Uses >= apiKey.MaxUses)
        {
            return BadRequest("API Key has no uses left");
        }

        // Increment the uses
        apiKey.Uses++;
        _context.APIKeys.Update(apiKey);
        await _context.SaveChangesAsync();

        // Create the note
        
    }
}