using System;

namespace ChatApp.Models
{
    public class Message
    {
        public string Text { get; set; } = string.Empty;
        public bool IsFromMe { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string FormattedTime => Timestamp.ToString("h:mm tt");
    }
}
