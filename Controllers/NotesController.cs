using Microsoft.AspNetCore.Mvc;
using TerraNotes.Data;
using TerraNotes.Models;

[Route("api/[controller]")]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotesController> _logger;
    private readonly NoteProcessor _noteProcessor;

    public NotesController(AppDbContext context, ILogger<NotesController> logger, NoteProcessor noteProcessor)
    {
        _context = context;
        _logger = logger;
        _noteProcessor = noteProcessor;
    }

    [HttpGet("{{id}}")]
    [ServiceFilter(typeof(APIKeyAuthFilter))]
    public async Task<ActionResult<Note>> GetNoteById(int id)
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
    public async Task<ActionResult<Note>> CreateNote(List<IFormFile> files)
    {
        var apiKey = (HttpContext.Items["APIKey"] as APIKey)!;

        // Check if there are enough uses left
        if (apiKey.Uses >= apiKey.MaxUses)
        {
            return BadRequest("API Key has no uses left");
        }

        // Check if the files are valid
        if (files.Count == 0)
        {
            return BadRequest("No files were uploaded");
        }
        string? fileType = null;
        foreach (var file in files)
        {
            var type = GetNoteInputType(file);
            if (type == null)
            {
                return BadRequest("Invalid file type");
            }
            else if (fileType == null)
            {
                fileType = type;
            }
            else if (fileType != type)
            {
                return BadRequest("All files must be of the same type");
            }
        }

        // Increment the uses
        apiKey.Uses++;
        _context.APIKeys.Update(apiKey);
        await _context.SaveChangesAsync();

        // Save the files to disk
        var filePaths = new List<string>();
        var filesDir = Path.Combine(Directory.GetCurrentDirectory(), "Files");
        Directory.CreateDirectory(filesDir);
        foreach (var file in files)
        {
            var fileName = $"{Guid.NewGuid()}.{fileType}";
            var filePath = Path.Combine(filesDir, fileName);
            filePaths.Add(filePath);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        // Create the note
        var note = new Note
        {
            Status = "pending",
            UserCreatedId = apiKey.UserCreatedId,
            UserCreated = apiKey.UserCreated,
            DateCreated = DateTime.UtcNow,
            InputFormat = fileType
        };

        // Create the task
        var task = new NoteTask(note, filePaths, "default");

        // Try to submit the task
        if (await _noteProcessor.TrySubmitTask(task))
        {
            // Actually save the note to the database
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        } else {
            // Delete the files and return an error
            foreach (var file in filePaths)
            {
                System.IO.File.Delete(file);
            }
            return StatusCode(503, "Server is busy");
        }

        return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
    }

    // Get the file type from the file by matching magic bytes (pdf, jpg, png)
    private string? GetNoteInputType(IFormFile file)
    {
        // Read the first 4 bytes of the file
        var magicBytes = new byte[4];
        file.OpenReadStream().Read(magicBytes, 0, 4);

        // Create a dictionary of file types and their magic bytes
        var fileTypes = new Dictionary<string, byte[]>
        {
            { "pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            { "jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { "png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } }
        };

        // Check if the magic bytes match any of the file types
        foreach (var fileType in fileTypes)
        {
            if (magicBytes.Take(fileType.Value.Length).SequenceEqual(fileType.Value))
            {
                return fileType.Key;
            }
        }

        // If the file name ends in .txt, then assume it's a text file
        if (file.FileName.EndsWith(".txt"))
        {
            return "txt";
        }

        return null;
    }
}