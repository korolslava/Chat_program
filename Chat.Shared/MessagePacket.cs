namespace Chat.Shared;

public class MessagePacket
{
    public string Sender { get; set; } = string.Empty;

    public string Receiver { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}