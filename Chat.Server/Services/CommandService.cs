using System.Text.Json;
using Chat.Shared;
using System.IO;

namespace Chat.Server.Services;

public class CommandService
{
    private readonly ClientManager _clientManager;

    public CommandService(ClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public async Task ExecuteAsync(
        MessagePacket packet,
        StreamWriter writer)
    {
        if (string.IsNullOrWhiteSpace(packet?.Content))
            return;

        string[] parts = packet.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        switch (parts[0].ToLower())
        {
            case "/who":
                var users = string.Join(", ", _clientManager.GetOnlineUsers());
                await SendSystemMessage(writer, $"Online users: {users}");
                break;

            case "/help":
                await SendSystemMessage(writer, "/who, /help, /clear, /msg [User] [Text]");
                break;

            case "/clear":
                await writer.WriteLineAsync(JsonSerializer.Serialize(
                    new MessagePacket
                    {
                        Type = MessageType.Command,
                        Content = "/clear"
                    }));
                break;
        }
    }

    private async Task SendSystemMessage(
        StreamWriter writer,
        string content)
    {
        await writer.WriteLineAsync(JsonSerializer.Serialize(
            new MessagePacket
            {
                Sender = "Server",
                Content = content,
                Type = MessageType.System
            }));
    }
}
