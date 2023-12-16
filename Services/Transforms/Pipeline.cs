using TerraNotes.Models;

public class Pipeline
{
    private readonly List<ITransform> transforms;

    public Pipeline(string preset)
    {
        transforms = new List<ITransform>();

        switch (preset) {
            default:
                throw new Exception($"Unknown pipeline preset {preset}");
        }
    }

    public void AddTransform(ITransform transform)
    {
        transforms.Add(transform);
    }

    public async Task<string> Run(TypedFile input, CancellationToken cancellationToken)
    {
        string? payload = null;

        foreach (var transform in transforms)
        {
            if (transform is IFileToTextTransform fileToTextTransform)
            {
                payload = await fileToTextTransform.Transform(input, cancellationToken);
            }
            else if (transform is ITextToTextTransform textToTextTransform)
            {
                payload = await textToTextTransform.Transform(payload!, cancellationToken);
            }
            else
            {
                throw new Exception($"Unknown transform type {transform.GetType()}");
            }
        }

        return payload!;
    }
}