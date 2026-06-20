using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Chat.Server.Data;
using Chat.Server.Helpers;
using Chat.Server.Services;
using Chat.Shared;

Console.Title = "TCP Chat - Server";

const int port = 5000;

var listener = new TcpListener(IPAddress.Any, port);

var clientManager = new ClientManager();
var broadcastService = new BroadcastService(clientManager);
var historyService = new HistoryService();
var commandService = new CommandService(clientManager);

using (var db = new ChatDbContext())
{
    ConsoleLogger.Info("Connecting to database...");
    await db.Database.EnsureCreatedAsync();
    ConsoleLogger.Info("Database ready.");
}

listener.Start();

ConsoleLogger.Info($"Server started on port {port}");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();

    _ = Task.Run(() =>
        HandleClientAsync(
            client,
            clientManager,
            broadcastService,
            historyService,
            commandService));
}

static async Task HandleClientAsync(
    TcpClient client,
    ClientManager clientManager,
    BroadcastService broadcastService,
    HistoryService historyService,
    CommandService commandService)
{
    using var stream = client.GetStream();
    using var reader = new StreamReader(stream);
    using var writer = new StreamWriter(stream)
    {
        AutoFlush = true
    };

    string currentUser = "";

    try
    {
        while (true)
        {
            string? json = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(json))
                break;

            var packet =
                JsonSerializer.Deserialize<MessagePacket>(json);

            if (packet == null)
                continue;

            if (packet.Type == MessageType.Join)
            {
                currentUser = packet.Sender;

                clientManager.AddClient(currentUser, writer);

                ConsoleLogger.Success($"{currentUser} joined");

                var history =
                    await historyService.GetHistoryAsync(currentUser);

                foreach (var msg in history)
                {
                    await writer.WriteLineAsync(
                        JsonSerializer.Serialize(msg));
                }

                await broadcastService.BroadcastAsync(packet);

                continue;
            }

            if (packet.Type == MessageType.Command)
            {
                await commandService.ExecuteAsync(packet, writer);

                continue;
            }

            if (packet.Type == MessageType.Chat)
            {
                await historyService.SaveMessageAsync(packet);

                ConsoleLogger.Message(packet);

                if (!string.IsNullOrEmpty(packet.Receiver))
                {
                    if (clientManager.TryGetClient(
                        packet.Receiver,
                        out var targetWriter))
                    {
                        var privatePacket = new MessagePacket
                        {
                            Sender = packet.Sender,
                            Receiver = packet.Receiver,
                            Content = packet.Content,
                            Timestamp = DateTime.UtcNow,
                            Type = MessageType.Private
                        };

                        string privateJson =
                            JsonSerializer.Serialize(privatePacket);

                        await targetWriter.WriteLineAsync(privateJson);

                        await writer.WriteLineAsync(privateJson);
                    }
                    else
                    {
                        await writer.WriteLineAsync(
                            JsonSerializer.Serialize(
                                new MessagePacket
                                {
                                    Sender = "Server",
                                    Content = "User is offline.",
                                    Type = MessageType.System
                                }));
                    }
                }
                else
                {
                    await broadcastService.BroadcastAsync(packet);
                }
            }
        }
    }
    catch (Exception ex)
    {
        ConsoleLogger.Error(ex.Message);
    }
    finally
    {
        if (!string.IsNullOrEmpty(currentUser))
        {
            clientManager.RemoveClient(currentUser);

            await broadcastService.BroadcastAsync(
                new MessagePacket
                {
                    Sender = "Server",
                    Content = $"{currentUser} left the chat",
                    Type = MessageType.Leave
                });

            ConsoleLogger.Warning($"{currentUser} disconnected");
        }

        client.Close();
    }
}
