using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Chat.Shared;
using Chat.Server.Data;

Console.Title = "TCP Chat - Server";
int port = 5000;
var tcpListener = new TcpListener(IPAddress.Any, port);
var connectedClients = new ConcurrentDictionary<string, StreamWriter>();

using (var db = new ChatDbContext())
{
    LogSystem("Connecting to PostgreSQL and verifying database...");
    await db.Database.EnsureCreatedAsync();
    LogSystem("Database is ready.");
}

try
{
    tcpListener.Start();
    LogSystem($"Server started successfully on port {port}. Waiting for connections...");

    while (true)
    {
        var client = await tcpListener.AcceptTcpClientAsync();
        _ = HandleClientAsync(client);
    }
}
catch (Exception ex)
{
    LogError($"Fatal server error: {ex.Message}");
}

async Task HandleClientAsync(TcpClient client)
{
    var endPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
    using var stream = client.GetStream();
    using var reader = new StreamReader(stream);
    using var writer = new StreamWriter(stream) { AutoFlush = true };

    string currentUserName = string.Empty;

    try
    {
        while (true)
        {
            var jsonLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(jsonLine)) break;

            var packet = JsonSerializer.Deserialize<MessagePacket>(jsonLine);
            if (packet == null) continue;

            if (packet.Type == MessageType.Join && string.IsNullOrEmpty(currentUserName))
            {
                currentUserName = packet.Sender;
                connectedClients.TryAdd(currentUserName, writer);

                LogSuccess($"{currentUserName} joined the chat. ({endPoint})");

                await SendHistoryToClientAsync(writer);

                await BroadcastMessageAsync(packet);
                continue;
            }

            if (packet.Type == MessageType.Chat)
            {
                using (var db = new ChatDbContext())
                {
                    db.Messages.Add(new ChatMessage
                    {
                        Sender = packet.Sender,
                        Content = packet.Content,
                        Timestamp = packet.Timestamp
                    });
                    await db.SaveChangesAsync();
                }

                LogMessage(packet);
                await BroadcastMessageAsync(packet);
            }
        }
    }
    catch (Exception)
    {
    }
    finally
    {
        if (!string.IsNullOrEmpty(currentUserName))
        {
            connectedClients.TryRemove(currentUserName, out _);
            LogWarning($"{currentUserName} disconnected.");

            var leavePacket = new MessagePacket
            {
                Sender = "Server",
                Content = $"{currentUserName} has left the chat.",
                Type = MessageType.Leave
            };
            await BroadcastMessageAsync(leavePacket);
        }
        client.Close();
    }
}

async Task SendHistoryToClientAsync(StreamWriter writer)
{
    using var db = new ChatDbContext();

    var history = await db.Messages
        .OrderByDescending(m => m.Timestamp)
        .Take(20)
        .ToListAsync();

    history.Reverse();

    foreach (var msg in history)
    {
        var historyPacket = new MessagePacket
        {
            Sender = msg.Sender,
            Content = msg.Content,
            Timestamp = msg.Timestamp,
            Type = MessageType.Chat
        };
        await writer.WriteLineAsync(JsonSerializer.Serialize(historyPacket));
    }
}

async Task BroadcastMessageAsync(MessagePacket packet)
{
    var json = JsonSerializer.Serialize(packet);

    foreach (var clientWriter in connectedClients.Values)
    {
        try
        {
            await clientWriter.WriteLineAsync(json);
        }
        catch
        {
        }
    }
}

void LogSystem(string message) => PrintColored($"[INFO] {message}", ConsoleColor.Cyan);
void LogSuccess(string message) => PrintColored($"[JOIN] {message}", ConsoleColor.Green);
void LogWarning(string message) => PrintColored($"[LEAVE] {message}", ConsoleColor.DarkYellow);
void LogError(string message) => PrintColored($"[ERROR] {message}", ConsoleColor.Red);

void LogMessage(MessagePacket packet)
{
    var time = packet.Timestamp.ToLocalTime().ToString("HH:mm:ss");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($"[{time}] ");
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.Write($"{packet.Sender}: ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(packet.Content);
    Console.ResetColor();
}

void PrintColored(string text, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ResetColor();
}