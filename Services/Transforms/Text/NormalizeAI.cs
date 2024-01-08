public class NormalizeAI : ITextToTextTransform
{
    public string Name => "PascalCase";

    public NormalizeAI()
    {

    }

    public async Task<string> Transform(string text, CancellationToken cancellationToken)
    {
        // If the string starts with ```md or ```markdown, trim it off
        if (text.StartsWith("```md"))
        {
            text = text.Substring(5);
            // If the string ends with ```, trim it off
            if (text.EndsWith("```"))
            {
                text = text.Substring(0, text.Length - 3);
            }
        }
        else if (text.StartsWith("```markdown"))
        {
            text = text.Substring(11);
            // If the string ends with ```, trim it off
            if (text.EndsWith("```"))
            {
                text = text.Substring(0, text.Length - 3);
            }
        }

        // If the text contains **System:** on the first line and **User:** somewhere later, then trim until after the **User:** line
        if (text.Contains("**System:**"))
        {
            var lines = text.Split("\n");
            var userIndex = Array.IndexOf(lines, "**User:**");
            if (userIndex != -1)
            {
                text = string.Join("\n", lines.Skip(userIndex + 1));
            }
        }

        // Remove --- from the beginning of the string
        if (text.StartsWith("---"))
        {
            text = text.Substring(3);
        }
        // Remove --- from the end of the string
        if (text.EndsWith("---"))
        {
            text = text.Substring(0, text.Length - 3);
        }

        // Trim whitespace from the beginning and end of the string
        text = text.Trim();

        return text;
    }
}