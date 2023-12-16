using System.Diagnostics;
using TerraNotes.Models;

public class FileToText : IFileToTextTransform
{
    public string Name => "FileToText";

    private readonly RocketVision _rocketVision;

    public FileToText(RocketVision rocketVision)
    {
        _rocketVision = rocketVision;
    }

    public async Task<string> Transform(TypedFile file, CancellationToken cancellationToken)
    {
        string? result = null;

        switch (file.Type) {
            case "pdf": {
                result = await ExtractTextFromPdf(file.Path, cancellationToken);
                if (result == null) {
                    throw new Exception($"Error extracting text from PDF {file.Path}");
                }
            } break;
            case "jpg": {
                var baseName = Path.GetFileName(file.Path);
                await _rocketVision.UploadFile(file.Path);
                result = await _rocketVision.TranscribeFile(baseName);
                if (result == null) {
                    throw new Exception($"Error transcribing image {file.Path}");
                }
            } break;
        }

        return result!;
    }

    private async Task<string?> ExtractTextFromPdf(string path, CancellationToken cancellationToken)
    {
        // Create a process and read the output: pdftotext -layout -nopgbrk -eol unix <path> -
        var process = new Process();
        process.StartInfo.FileName = "pdftotext";
        process.StartInfo.Arguments = $"-layout -nopgbrk -eol unix {path} -";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            throw new Exception($"Error starting pdftotext process: {e.Message}");
        }

        string? stdout = await process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"pdftotext failed with exit code {process.ExitCode}");
        }

        // Remove all Form Feed characters and trim whitespace
        stdout = stdout!.Replace("\f", "").Trim();

        // If the output is empty, return null
        if (stdout.Length == 0)
        {
            return null;
        }
        return stdout;
    }
}