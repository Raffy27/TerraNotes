using System.Net;
using System.Text;
using System.Text.Json;

public class RocketVision
{
    private string? user;
    private string? email;
    private string? name;
    private string? userId;
    private string? sessionToken;
    private string? firebaseJWT;
    private string? secureJWT;
    private string? refreshToken;

    private string firebaseApiKey;
    private readonly Logger<RocketVision> _logger;

    public RocketVision(string firebaseApiKey, Logger<RocketVision> logger)
    {
        this.firebaseApiKey = firebaseApiKey;
        _logger = logger;
    }

    private async Task Login(string email, string password)
    {
        var loginData = new
        {
            username = email,
            password,
            method = "GET"
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

        user = data["username"].ToString();
        name = data["name"].ToString();
        this.email = data["email"].ToString();
        userId = data["objectId"].ToString();
        sessionToken = data["sessionToken"].ToString();
    }

    private async Task CastToken()
    {
        if (sessionToken == null || sessionToken == "")
        {
            throw new Exception("Session token is empty");
        }
        if (userId == null || userId == "")
        {
            throw new Exception("User ID is empty");
        }

        var safeUserId = WebUtility.UrlEncode(userId);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.getrocketbook.com/cast/" + safeUserId + "/token");
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

    private async Task GetSecureJWT()
    {
        var payloadObject = new
        {
            token = firebaseJWT,
            returnSecureToken = true
        };
        var payload = JsonSerializer.Serialize(payloadObject);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken?key=" + firebaseApiKey)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
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
        secureJWT = data["idToken"].ToString();
        refreshToken = data["refreshToken"].ToString();
    }

    public async Task Authenticate(string email, string password)
    {
        await Login(email, password);
        await CastToken();
        await GetSecureJWT();
    }

    private async Task<string> GetUploadUrl(string fileName)
    {
        var urlencodedFilename = WebUtility.UrlEncode(userId + "/" + fileName);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://firebasestorage.googleapis.com/v0/b/rocket-vision/o?" +
            "name=" + urlencodedFilename + "&" +
            "uploadType=resumable")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", "Firebase " + secureJWT);
        request.Headers.Add("X-Firebase-Storage-Version", "Android/23.11.15 (190400-520109968)");
        request.Headers.Add("x-firebase-gmpid", "1:807535467202:android:71029d5975dd1c05");
        request.Headers.Add("X-Goog-Upload-Command", "start");
        request.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        request.Headers.Add("X-Goog-Upload-Header-Content-Type", "application/octet-stream");
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

    public async Task UploadFile(string fileName)
    {
        if (secureJWT == null || secureJWT == "")
        {
            throw new Exception("Secure JWT is empty");
        }

        var uploadUrl = await GetUploadUrl(fileName);

        // Get file size and a reader
        var fileInfo = new FileInfo(fileName);
        var fileSize = fileInfo.Length;
        var payload = new FileStream(fileName, FileMode.Open);

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
        {
            Content = new StreamContent(payload)
        };
        request.Headers.Add("Authorization", "Firebase " + secureJWT);
        request.Headers.Add("X-Firebase-Storage-Version", "Android/23.11.15 (190400-520109968)");
        request.Headers.Add("x-firebase-gmpid", "1:807535467202:android:71029d5975dd1c05");
        request.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
        request.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        request.Headers.Add("X-Goog-Upload-Offset", "0");
        request.Headers.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 13; 21081111RG Build/TP1A.220624.014)");
        // This bit is very important, otherwise Transfer-Encoding defaults to chunked
        request.Content.Headers.ContentLength = fileSize;

        var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Upload file failed with status code {response.StatusCode}");
        }

        // Parse JSON response
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
        if (data == null)
        {
            throw new Exception("Failed to parse upload file response");
        }
    }

    public async Task<string> TranscribeFile(string fileName)
    {
        var originalFilenames = new string[] { fileName };
        var originalFilenamesJsonUrlEncoded = WebUtility.UrlEncode(JsonSerializer.Serialize(originalFilenames));
        var urlencodedEmail = WebUtility.UrlEncode(email);
        var payload = new StringContent("userId=" + userId + "&" +
            "email=" + urlencodedEmail + "&" +
            "searchable=true&" +
            "os=android&" +
            "ocrPageTitle=true&" +
            "originalFilenames=" + originalFilenamesJsonUrlEncoded, Encoding.UTF8, "application/x-www-form-urlencoded");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.getrocketbook.com/scans/transcribe")
        {
            Content = payload
        };
        request.Headers.Add("Authorization", "Bearer " + sessionToken);
        request.Headers.Add("User-Agent", "Google-HTTP-Java-Client/1.42.3 (gzip)");

        var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Transcribe file failed with status code {response.StatusCode}");
        }

        // Parse JSON responseint a dynamic array
        var data = JsonSerializer.Deserialize<Dictionary<string, object>[]>(await response.Content.ReadAsStringAsync());
        if (data == null)
        {
            throw new Exception("Failed to parse transcribe file response");
        }

        string text = "";
        foreach (var page in data)
        {
            text += page["description"].ToString();
        }

        return text;
    }
}