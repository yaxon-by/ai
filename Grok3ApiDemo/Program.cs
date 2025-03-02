using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Grok3Client", client =>
{
    client.BaseAddress = new Uri("https://api.x.ai/v1/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Grok3ApiKey"]}");
});

var app = builder.Build();

// Define a simple endpoint to test the Grok 3 API
app.MapGet("/ask-grok", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Grok3Client");

    // Example request payload (adjust based on actual API requirements)
    var requestPayload = new
    {
        model = "grok-beta", // Using grok-beta as per public beta info; update to "grok-3" when available
        messages = new[]
        {
            new { role = "user", content = "What is the meaning of life?" }
        }
    };

    try
    {
        var response = await client.PostAsJsonAsync("chat/completions", requestPayload);
        
        var responseString = await response.Content.ReadAsStringAsync();
        
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var answer = result.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return Results.Ok(new { Answer = answer });
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem($"Error calling Grok 3 API: {ex.Message}");
    }
});

app.Run();