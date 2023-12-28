public class PascalCase : ITextToTextTransform
{
    public string Name => "PascalCase";

    public PascalCase()
    {

    }

    public async Task<string> Transform(string text, CancellationToken cancellationToken)
    { 
        var completion = ChatCompletionFactory.Instance.Create("gpt-4", "Bing")
            .WithMessage(
                ChatCompletion.Role.System,
                "You are a text transformer. " +
                "You repeat the same messages that are given to you, but with all the words transformed to PascalCase. " +
                "You don't need to introduce or explain your responses, just provide the result of the transformation."
            )
            .WithMessage(ChatCompletion.Role.User, text);
        
        return await completion.Complete(cancellationToken);
    }
}