public class Markdown : ITextToTextTransform
{
    public string Name => "Markdown";

    public Markdown()
    {

    }

    public async Task<string> Transform(string text, CancellationToken cancellationToken)
    {
        var completion = ChatCompletionFactory.Instance.Create("mixtral-8x7b", null)
            .WithMessage(
                ChatCompletion.Role.System,
                "You are a text transformer. " +
                "You repeat the messages given to you, but with contextual markdown formatting applied to them. " +
                "You don't need to introduce or explain your responses, just provide the result of the transformation."
            )
            .WithMessage(ChatCompletion.Role.User, text);
        
        return await completion.Complete(cancellationToken);
    }
}