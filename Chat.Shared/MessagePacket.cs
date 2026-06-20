using System;

namespace Chat.Shared;

public enum MessageType
{
    Chat,
    System,
    Join,
    Leave
}

public class MessagePacket
{
    public string Sender { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Chat;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}