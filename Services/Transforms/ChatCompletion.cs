using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ChatCompletion
{
    public enum Role
    {
        System,
        Assistant,
        User
    }
    private class Message 
    {
        public Role? role;

        [JsonPropertyName("content")]
        public string? Text { get; set; }

        [JsonPropertyName("role")]
        public string? RoleAsString => role?.ToString().ToLower();
    }

    private readonly string apiUrl;
    private readonly string model;
    private readonly string? provider;
    private readonly float temperature;
    private readonly List<Message> messages = new();

    public ChatCompletion(string apiUrl, string model, string? provider, float temperature)
    {
        this.apiUrl = apiUrl;
        this.model = model;
        this.provider = provider;
        this.temperature = temperature;
    }

    public ChatCompletion WithMessage(Role role, string text)
    {
        messages.Add(new Message() { role = role, Text = text });
        return this;
    }

    public async Task<string> Complete(CancellationToken cancellationToken)
    {
        dynamic requestParams = new {
            model,
            messages,
            temperature
        };
        if (provider != null)
        {
            requestParams.provider = provider;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/chat/completions") {
            Content = new StringContent(JsonSerializer.Serialize(requestParams), Encoding.UTF8, "application/json")
        };
        Console.WriteLine(JsonSerializer.Serialize(requestParams));

        var client = new HttpClient() {
            Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite)
        };
        var response = await client.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine(responseContent);

        // Deserialize the response
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)!;
        var choices = responseJson["choices"] as JsonElement?;
        var choice = choices!.Value.EnumerateArray().First();
        var text = choice.GetProperty("message").GetProperty("content").GetString()!;

        return text;
    }
}