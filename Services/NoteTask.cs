using TerraNotes.Data;
using TerraNotes.Models;

public class NoteTask 
{
    private Note note;
    private readonly List<TypedFile> files;
    private readonly string preset;
    private AppDbContext? _context;
    private ILogger<NoteTask>? _logger;
    private RocketVision? _rocketVision;

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
        _logger!.LogInformation($"Note {note.Id} status set to {status}");
    }

    public async Task SetContent(string content)
    {
        note.Content = content;
        _context!.Notes.Update(note);
        await _context.SaveChangesAsync();
        _logger!.LogInformation($"Note {note.Id} content set");
    }

    public async Task Run(CancellationToken cancellationToken, AppDbContext context, RocketVision rocketVision, ILogger<NoteTask> logger)
    {
        _context = context;
        _rocketVision = rocketVision;
        _logger = logger;

        try
        {
            _logger!.LogInformation($"Processing note {note.Id}");
            while (note.Id == 0) {
                _logger!.LogInformation($"Note {note.Id} is not ready yet");
                await Task.Delay(100);
            }
            await SetStatus("running");

            // Setup transform pipeline
            var pipeline = new Pipeline(preset, rocketVision);

            // TODO: Parallelize this
            string result = "";
            foreach (var file in files)
            {
                string partialResult = await pipeline.Run(file, cancellationToken);
                result += partialResult + "\n";
            }

            await SetContent(result);
            await SetStatus("complete");
        }
        catch (Exception e)
        {
            _logger!.LogError($"Error processing note {note.Id}: {e}");
            await SetStatus("failed");
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