namespace ChatApp.Models;

public class Message
{
    public string Text { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsFromMe { get; set; }
}
