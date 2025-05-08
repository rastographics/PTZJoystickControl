public class WebSocketMessage
{
    public string Type { get; set; } = null!; // e.g., "event", "request", "response", "command"
    public string Action { get; set; } = null!; // e.g., "selectedCameraChanged", "previewCamera"
    public object? Payload { get; set; } // The data associated with the message
}
