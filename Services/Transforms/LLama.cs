using System.Text;
using System.Text.Json;

public sealed class LLama
{
    public static string API_URL = "http://localhost:11434/api";
    private static readonly LLama instance = new LLama();
    static LLama()
    {
    }
    private LLama() {
        // Check if the model is downloaded:
        // Do aweb request to /tags, parse json
        var client = new HttpClient();
        var request = client.GetAsync($"{API_URL}/tags").Result;
        var response = request.Content.ReadAsStringAsync().Result;

        var responseJson = JsonSerializer.Deserialize<Dictionary<string, object>>(response)!;
        var models = responseJson["models"] as JsonElement?;
        foreach (var model in models!.Value.EnumerateArray())
        {
            if (model.GetProperty("name").GetString()!.StartsWith("llama2")) {
                return;
            }
        }
        
        throw new Exception("LLama model not found. Please download it first!");
    }

    public static LLama Instance
    {
        get
        {
            return instance;
        }
    }

    public async Task<string> Prompt(string inst, string prompt) {
        var requestParams = new {
            model = "llama2",
            stream = false,
            prompt = $"[INST]{inst}[/INST]\n{prompt}"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{API_URL}/generate") {
            Content = new StringContent(JsonSerializer.Serialize(requestParams), Encoding.UTF8, "application/json")
        };

        var client = new HttpClient() {
            Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite)
        };
        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Deserialize the response
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)!;
        return responseJson["response"].ToString()!;
    }
}