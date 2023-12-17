public class Markdown : ITextToTextTransform
{
    public string Name => "Markdown";

    public Markdown()
    {

    }

    public async Task<string> Transform(string text, CancellationToken cancellationToken)
    {
        const string instr = "You are a text transformer. You are given input text and you must convert it to markdown using the necessary formatting. Then you must write the result to the output, and only the result.";
        var output = await LLama.Instance.Prompt(instr, text);
        Console.WriteLine("LLama output: " + output);
        return output;
    }
}