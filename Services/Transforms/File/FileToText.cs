using TerraNotes.Models;

public class FileToText : IFileToTextTransform
{
    public string Name => "FileToText";

    public async Task<string> Transform(TypedFile file, CancellationToken cancellationToken)
    {
        string result = "";

        switch (file.Type) {
            case "pdf":
                break;
        }

        return result;
    }
}