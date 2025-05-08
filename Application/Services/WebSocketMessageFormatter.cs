using System.Text.Json;

public static class WebSocketMessageFormatter
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Converts properties to camel case
        WriteIndented = false // Optional: Set to true for pretty-printed JSON
    };
    public static string Serialize(WebSocketMessage message)
    {
        return JsonSerializer.Serialize(message, _jsonOptions);
    }

    public static WebSocketMessage? Deserialize(string message)
    {
        return JsonSerializer.Deserialize<WebSocketMessage>(message, _jsonOptions);
    }
}
