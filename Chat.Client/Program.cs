using System.Net.Sockets;
using System.Text.Json;
using Chat.Shared;

Console.Title = "TCP Chat - Client";

DrawHeader();

using var client = new TcpClient();
try
{
    PrintSystem("Connecting to the server (127.0.0.1:5000)...");
    await client.ConnectAsync("127.0.0.1", 5000);
    PrintSuccess("Connected successfully!\n");

    using var stream = client.GetStream();
    using var writer = new StreamWriter(stream) { AutoFlush = true };
    using var reader = new StreamReader(stream);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Enter your username: ");
    Console.ResetColor();

    var userName = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(userName)) userName = $"User_{new Random().Next(1000, 9999)}";

    Console.Clear();
    DrawHeader();
    PrintSystem($"Welcome, {userName}! You can start typing messages.");
    PrintSystem("Type 'exit' to leave the chat.");
    Console.WriteLine(new string('-', 50));

    var joinPacket = new MessagePacket
    {
        Sender = userName,
        Content = $"{userName} joined the chat.",
        Type = MessageType.Join
    };
    await writer.WriteLineAsync(JsonSerializer.Serialize(joinPacket));

    _ = Task.Run(() => ReceiveMessagesAsync(reader));

    while (true)
    {
        var message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message)) continue;
        if (message.ToLower() == "exit") break;

        var packet = new MessagePacket
        {
            Sender = userName,
            Content = message,
            Type = MessageType.Chat
        };

        var json = JsonSerializer.Serialize(packet);
        await writer.WriteLineAsync(json);
    }
}
catch (Exception ex)
{
    PrintError($"Connection failed: {ex.Message}");
}

async Task ReceiveMessagesAsync(StreamReader reader)
{
    try
    {
        while (true)
        {
            var jsonLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(jsonLine))
            {
                PrintSystem("Disconnected from server.");
                break;
            }

            var packet = JsonSerializer.Deserialize<MessagePacket>(jsonLine);
            if (packet != null)
            {
                DisplayIncomingMessage(packet);
            }
        }
    }
    catch
    {
        PrintSystem("Connection closed.");
    }
}

void DisplayIncomingMessage(MessagePacket packet)
{

    var time = packet.Timestamp.ToLocalTime().ToString("HH:mm");

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($"\r[{time}] ");

    if (packet.Type == MessageType.Join || packet.Type == MessageType.Leave)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{packet.Content}");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{packet.Sender}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(packet.Content);
    }
    Console.ResetColor();
}

void DrawHeader()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==================================================");
    Console.WriteLine("                  T C P   C H A T                 ");
    Console.WriteLine("==================================================");
    Console.ResetColor();
}

void PrintSystem(string text)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(text);
    Console.ResetColor();
}

void PrintSuccess(string text)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(text);
    Console.ResetColor();
}

void PrintError(string text)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(text);
    Console.ResetColor();
}