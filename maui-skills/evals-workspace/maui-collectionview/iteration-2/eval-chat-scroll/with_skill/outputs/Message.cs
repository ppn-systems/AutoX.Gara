namespace ChatApp.Models;

public class Message
{
    public string Text { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsFromMe { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Convenience properties for binding layout alignment
    public Microsoft.Maui.Controls.LayoutOptions HorizontalAlignment =>
        IsFromMe
            ? Microsoft.Maui.Controls.LayoutOptions.End
            : Microsoft.Maui.Controls.LayoutOptions.Start;

    public Microsoft.Maui.Graphics.Color BubbleColor =>
        IsFromMe
            ? Microsoft.Maui.Graphics.Color.FromArgb("#0078D4")
            : Microsoft.Maui.Graphics.Color.FromArgb("#E1E1E1");

    public Microsoft.Maui.Graphics.Color TextColor =>
        IsFromMe
            ? Microsoft.Maui.Graphics.Colors.White
            : Microsoft.Maui.Graphics.Colors.Black;
}
