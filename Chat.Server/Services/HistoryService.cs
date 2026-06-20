using Microsoft.EntityFrameworkCore;
using Chat.Server.Data;
using Chat.Shared;

namespace Chat.Server.Services;

public class HistoryService
{
    public async Task SaveMessageAsync(MessagePacket packet)
    {
        using var db = new ChatDbContext();

        db.Messages.Add(new ChatMessage
        {
            Sender = packet.Sender,
            Receiver = packet.Receiver,
            Content = packet.Content,
            Timestamp = packet.Timestamp
        });

        await db.SaveChangesAsync();
    }

    public async Task<List<MessagePacket>> GetHistoryAsync(string userName)
    {
        using var db = new ChatDbContext();

        var messages = await db.Messages
            .Where(m =>
                string.IsNullOrEmpty(m.Receiver)
                || m.Receiver == userName
                || m.Sender == userName)
            .OrderByDescending(m => m.Timestamp)
            .Take(20)
            .ToListAsync();

        messages.Reverse();

        return messages.Select(m => new MessagePacket
        {
            Sender = m.Sender,
            Receiver = m.Receiver,
            Content = m.Content,
            Timestamp = m.Timestamp,
            Type = MessageType.Chat
        }).ToList();
    }
}
