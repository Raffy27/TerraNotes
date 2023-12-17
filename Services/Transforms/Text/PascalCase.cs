public class PascalCase : ITextToTextTransform
{
    public string Name => "PascalCase";

    public PascalCase()
    {

    }

    public async Task<string> Transform(string text, CancellationToken cancellationToken)
    {
        const string instr = "You are a text transformer. You are given input text and you must capitalize the first letter of every word, then write it to the output. Write nothing else to the output but the resulting text.";
        var output = await LLama.Instance.Prompt(instr, text);
        Console.WriteLine("LLama output: " + output);
        return output;
    }
}