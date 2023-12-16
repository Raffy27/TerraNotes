using TerraNotes.Data;
using TerraNotes.Models;

public class NoteTask 
{
    private Note note;
    private readonly List<TypedFile> files;
    private readonly string preset;
    private AppDbContext? _context;
    private ILogger<NoteTask>? _logger;

    public NoteTask(Note note, List<TypedFile> files, string preset)
    {
        this.note = note;
        this.files = files;
        this.preset = preset;
    }

    private async Task SetStatus(string status)
    {
        note.Status = status;
        _context!.Notes.Update(note);
        await _context.SaveChangesAsync();
        _logger!.LogDebug($"Note {note.Id} status set to {status}");
    }

    public async Task Run(CancellationToken cancellationToken, AppDbContext context, ILogger<NoteTask> logger)
    {
        _context = context;
        _logger = logger;

        try
        {
            _logger!.LogInformation($"Processing note {note.Id}");
            await SetStatus("running");

            // Setup transform pipeline
            var pipeline = new Pipeline(preset);

            // TODO: Parallelize this
            string result = "";
            foreach (var file in files)
            {
                string partialResult = await pipeline.Run(file, cancellationToken);
                result += partialResult;
            }

            await SetStatus("complete");
        }
        finally
        {
            // Delete the files
            foreach (var file in files)
            {
                File.Delete(file.Path);
            }
            _logger!.LogInformation($"Deleted files for note {note.Id}");
        }
    }
}