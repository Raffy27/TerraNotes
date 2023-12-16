using System.Net;
using System.Text;
using System.Text.Json;

public class RocketVision
{
    private string user;
    private string email;
    private string name;
    private string userId;
    private string sessionToken;
    private string firebaseJWT;
    private string secureJWT;
    private string refreshToken;

    public async Task Login(string email, string password)
    {
        var loginData = new
        {
            Username = email,
            Password = password,
            Method = "GET"
        };
        var body = JsonSerializer.Serialize(loginData);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api-parse.getrocketbook.com/parse/login")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("User-Agent", "Parse Android SDK API Level 33");
        request.Headers.Add("X-Parse-Application-Id", "b2BIMBU8WOK3zmbogXDAGRGZL6m05K0DFMWTgGbA");
        request.Headers.Add("X-Parse-App-Build-Version", "3040019");
        request.Headers.Add("X-Parse-App-Display-Version", "3.4.0");
        request.Headers.Add("X-Parse-Installation-Id", "e9e385aa-4fd0-4a19-8249-acc73c6c33f4");
        request.Headers.Add("X-Parse-Client-Key", "fyTOFGKpYYBRXvHCEabnJ8FmVli5QxicUD0Rn7fU");

        var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Login failed with status code {response.StatusCode}");
        }

        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
        if (data == null)
        {
            throw new Exception("Failed to parse login response");
        }

        user = (data["username"] as string)!;
        name = (data["name"] as string)!;
        userId = (data["objectId"] as string)!;
        sessionToken = (data["sessionToken"] as string)!;
    }

    public async Task CastToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.getrocketbook.com/cast/" + userId + "/token");
        request.Headers.Add("Authorization", "Bearer " + sessionToken);
        request.Headers.Add("User-Agent", "Google-HTTP-Java-Client/1.42.3 (gzip)");

        var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Cast token failed with status code {response.StatusCode}");
        }

        // Get response as string
        var body = await response.Content.ReadAsStringAsync();
        // Remove quotes from string
        firebaseJWT = body.Trim('"');
    }

    public async Task GetSecureJWT()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken?key=" + apiKey);
        request.Headers.Add("Content-Type", "application/json");
        request.Headers.Add("X-Android-Package", "com.rb.rocketbook");
        request.Headers.Add("X-Android-Cert", "5D08264B44E0E53FBCCC70B4F016474CC6C5AB5C");
        request.Headers.Add("Accept-Language", "en-US");
        request.Headers.Add("X-Client-Version", "Android/Fallback/X21001000/FirebaseCore-Android");
        request.Headers.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 13; 21081111RG Build/TP1A.220624.014)");

        var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Get secure JWT failed with status code {response.StatusCode}");
        }

        // Parse JSON response
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
        if (data == null)
        {
            throw new Exception("Failed to parse secure JWT response");
        }

        // Save secure token and other useful data
        secureJWT = (data["idToken"] as string)!;
        refreshToken = (data["refreshToken"] as string)!;
    }

    private async Task<string> GetUploadUrl(string fileName) {
        var urlencodedFilename = WebUtility.UrlEncode(userId + "/" + fileName);

        var payload = new StringContent("{}", Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "https://firebasestorage.googleapis.com/v0/b/rocket-vision/o?" +
            "name=" + urlencodedFilename + "&" +
            "uploadType=resumable")
        {
            Content = payload
        };
        request.Headers.Add("Authorization", "Firebase " + secureJWT);
        request.Headers.Add("X-Firebase-Storage-Version", "Android/23.11.15 (190400-520109968)");
        request.Headers.Add("x-firebase-gmpid", "1:807535467202:android:71029d5975dd1c05");
        request.Headers.Add("X-Goog-Upload-Command", "start");
        request.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        request.Headers.Add("X-Goog-Upload-Header-Content-Type", "application/octet-stream");
        request.Headers.Add("Content-Type", "application/json");
        request.Headers.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 13; 21081111RG Build/TP1A.220624.014)");
        request.Content.Headers.ContentLength = 2;

        var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Get upload URL failed with status code {response.StatusCode}");
        }

        var uploadUrl = response.Headers.GetValues("X-Goog-Upload-URL").FirstOrDefault();
        if (uploadUrl == null)
        {
            throw new Exception("Upload URL is empty");
        }

        return uploadUrl;
    }

    
}