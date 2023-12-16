using TerraNotes.Models;

public interface ITransform
{
    string Name { get; }
}

public interface IFileToTextTransform : ITransform
{
    Task<string> Transform(TypedFile file, CancellationToken cancellationToken);
}

public interface ITextToTextTransform : ITransform
{
    Task<string> Transform(string text, CancellationToken cancellationToken);
}