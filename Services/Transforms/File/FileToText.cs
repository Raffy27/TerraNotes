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

        switch (file.Type)
        {
            case "pdf":
                {
                    result = await ExtractTextFromPdf(file.Path, cancellationToken);
                    if (result != null)
                    {
                        return result;
                    }

                    // Fallback: convert to images and transcribe
                    var images = await ConvertPdfToImages(file.Path, cancellationToken);
                    try
                    {
                        foreach (var image in images)
                        {
                            result = await PerformOcr(image, cancellationToken);
                            if (result == null)
                            {
                                throw new Exception($"Error extracting text from PDF {file.Path}");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        // Delete the images
                        foreach (var image in images)
                        {
                            File.Delete(image);
                        }
                    }
                }
                break;
            case "png":
            case "jpg":
                {
                    result = await PerformOcr(file.Path, cancellationToken);
                    if (result == null)
                    {
                        throw new Exception($"Error transcribing image {file.Path}");
                    }
                }
                break;
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
        await process.WaitForExitAsync();

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

    private async Task<string[]> ConvertPdfToImages(string path, CancellationToken cancellationToken)
    {
        // Create a process and read the output: pdftoppm -jpeg <path> <basepath>/<basenameNoExt>
        var process = new Process();
        process.StartInfo.FileName = "pdftoppm";
        process.StartInfo.Arguments = $"-jpeg {path} {Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path))}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            throw new Exception($"Error starting pdftoppm process: {e.Message}");
        }

        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new Exception($"pdftoppm failed with exit code {process.ExitCode}");
        }

        // Get the list of files
        var files = Directory.GetFiles(Path.GetDirectoryName(path)!, $"{Path.GetFileNameWithoutExtension(path)}*.jpg");
        return files;
    }

    private async Task<string?> PerformOcr(string path, CancellationToken cancellationToken)
    {
        var baseName = Path.GetFileName(path);
        await _rocketVision.UploadFile(path);
        return await _rocketVision.TranscribeFile(baseName);
    }
}