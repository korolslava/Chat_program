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

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Enter your username: ");
    Console.ResetColor();

    var userName = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(userName)) userName = "Anonymous";

    Console.Clear();
    DrawHeader();
    PrintSystem($"Welcome, {userName}! You can start typing messages.");
    PrintSystem("Type 'exit' to leave the chat.");
    Console.WriteLine(new string('-', 50));

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write("You > ");
        Console.ResetColor();

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

void DrawHeader()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==================================================");
    Console.WriteLine("                      C H A T                     ");
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