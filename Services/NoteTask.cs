using TerraNotes.Data;
using TerraNotes.Models;

public class NoteTask 
{
    private Note note;
    private readonly List<string> files;
    private AppDbContext? _context;

    public NoteTask(Note note, List<string> files, string pipeline)
    {
        this.note = note;
        this.files = files;
    }

    private async Task SetStatus(string status)
    {
        note.Status = status;
        _context!.Notes.Update(note);
        await _context.SaveChangesAsync();
    }

    public async Task Run(CancellationToken cancellationToken, AppDbContext context)
    {
        _context = context;

        try
        {
            Console.WriteLine($"Processing note {note.Id}");
            await SetStatus("running");
            Console.WriteLine($"Processing note {note.Id} - status set to running");
            // Delay to simulate processing
            await Task.Delay(5000, cancellationToken);
            Console.WriteLine($"Processing note {note.Id} - delay finished");
            await SetStatus("complete");
            Console.WriteLine($"Processing note {note.Id} - status set to complete");
        }
        finally
        {
            // Delete the files
            foreach (var file in files)
            {
                File.Delete(file);
            }
            Console.WriteLine($"Processing note {note.Id} - files deleted");
        }
    }
}