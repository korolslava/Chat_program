using System.Net.Sockets;
using System.Text.Json;
using Chat.Shared;

Console.Title = "TCP Chat - Client";

DrawHeader();

using var client = new TcpClient();

try
{
    PrintInfo("Connecting to server...");

    await client.ConnectAsync("127.0.0.1", 5000);

    PrintSuccess("Connected!");

    using var stream = client.GetStream();
    using var writer = new StreamWriter(stream)
    {
        AutoFlush = true
    };

    using var reader = new StreamReader(stream);

    Console.Write("Username: ");

    string userName =
        Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(userName))
    {
        userName =
            $"User_{Random.Shared.Next(1000, 9999)}";
    }

    await writer.WriteLineAsync(
        JsonSerializer.Serialize(
            new MessagePacket
            {
                Sender = userName,
                Content = $"{userName} joined",
                Type = MessageType.Join
            }));

    _ = Task.Run(() =>
        ReceiveMessagesAsync(reader));

    while (true)
    {
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            continue;

        if (input.ToLower() == "exit")
            break;

        if (input.StartsWith("/"))
        {
            await writer.WriteLineAsync(
                JsonSerializer.Serialize(
                    new MessagePacket
                    {
                        Sender = userName,
                        Content = input,
                        Type = MessageType.Command
                    }));

            continue;
        }

        string receiver = "";

        string content = input;

        if (input.StartsWith("/msg "))
        {
            var parts = input.Split(' ', 3);

            if (parts.Length < 3)
            {
                PrintError(
                    "Use: /msg [User] [Text]");

                continue;
            }

            receiver = parts[1];

            content = parts[2];
        }

        var packet = new MessagePacket
        {
            Sender = userName,
            Receiver = receiver,
            Content = content,
            Timestamp = DateTime.UtcNow,
            Type = MessageType.Chat
        };

        await writer.WriteLineAsync(
            JsonSerializer.Serialize(packet));
    }
}
catch (Exception ex)
{
    PrintError(ex.Message);
}

static async Task ReceiveMessagesAsync(
    StreamReader reader)
{
    try
    {
        while (true)
        {
            string? json =
                await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(json))
                break;

            var packet =
                JsonSerializer.Deserialize<MessagePacket>(json);

            if (packet == null)
                continue;

            if (packet.Type == MessageType.Command
                && packet.Content == "/clear")
            {
                Console.Clear();

                DrawHeader();

                continue;
            }

            ShowPacket(packet);
        }
    }
    catch
    {
        PrintError("Disconnected.");
    }
}

static void ShowPacket(
    MessagePacket packet)
{
    Console.ForegroundColor = ConsoleColor.DarkGray;

    Console.Write(
        $"[{packet.Timestamp.ToLocalTime():HH:mm}] ");

    switch (packet.Type)
    {
        case MessageType.System:
        case MessageType.Join:
        case MessageType.Leave:

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(packet.Content);

            break;

        case MessageType.Private:

            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.Write("[PRIVATE] ");

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.Write($"{packet.Sender}: ");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(packet.Content);

            break;

        default:

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.Write($"{packet.Sender}: ");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(packet.Content);

            break;
    }

    Console.ResetColor();
}

static void DrawHeader()
{
    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine("=====================================");
    Console.WriteLine("            TCP CHAT");
    Console.WriteLine("=====================================");

    Console.ResetColor();
}

static void PrintInfo(string text)
{
    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine(text);

    Console.ResetColor();
}

static void PrintSuccess(string text)
{
    Console.ForegroundColor = ConsoleColor.Green;

    Console.WriteLine(text);

    Console.ResetColor();
}

static void PrintError(string text)
{
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine(text);

    Console.ResetColor();
}
