using System.Text.Json;

public sealed class ChatCompletionFactory
{
    public static string API_URL = "http://localhost:1337/v1";
    public static float BASE_TEMPERATURE = 0.7f;
    private static readonly ChatCompletionFactory instance = new ChatCompletionFactory();

    private HashSet<string> validModels = new HashSet<string>();

    static ChatCompletionFactory()
    {
    }
    private ChatCompletionFactory()
    {
        var client = new HttpClient();
        var request = client.GetAsync($"{API_URL}/models").Result;
        var response = request.Content.ReadAsStringAsync().Result;

        var responseJson = JsonSerializer.Deserialize<Dictionary<string, object>>(response)!;
        var models = responseJson["data"] as JsonElement?;
        foreach (var model in models!.Value.EnumerateArray())
        {
            validModels.Add(model.GetProperty("id").GetString()!);
        }
    }
    public static ChatCompletionFactory Instance
    {
        get
        {
            return instance;
        }
    }

    public bool ModelExists(string model)
    {
        return validModels.Contains(model);
    }

    public ChatCompletion Create(string model, string? provider)
    {
        if (!ModelExists(model))
        {
            throw new Exception("Model not found. Please ensure that it exists first");
        }

        return new ChatCompletion(API_URL, model, provider, BASE_TEMPERATURE);
    }
}