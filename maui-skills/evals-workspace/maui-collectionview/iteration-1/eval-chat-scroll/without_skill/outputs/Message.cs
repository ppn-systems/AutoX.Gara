namespace ChatApp.Models;

public class Message
{
    public string Text { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsMe { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string FormattedTime => Timestamp.ToString("h:mm tt");
}
