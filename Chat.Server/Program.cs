using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Chat.Shared;

Console.Title = "TCP Chat - Server";
int port = 5000;
var tcpListener = new TcpListener(IPAddress.Any, port);

try
{
    tcpListener.Start();
    LogSystem($"Server started successfully on port {port}. Waiting for connections...");

    while (true)
    {
        var client = await tcpListener.AcceptTcpClientAsync();
        LogSuccess($"New connection established: {client.Client.RemoteEndPoint}");

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

    try
    {
        while (true)
        {
            var jsonLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(jsonLine)) break; // Client disconnected gracefully

            var packet = JsonSerializer.Deserialize<MessagePacket>(jsonLine);
            if (packet != null)
            {
                LogMessage(packet);
            }
        }
    }
    catch (Exception)
    {
        LogWarning($"Connection lost with client: {endPoint}");
    }
    finally
    {
        client.Close();
        LogWarning($"Client disconnected: {endPoint}");
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