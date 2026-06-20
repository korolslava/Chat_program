using Chat.Shared;

namespace Chat.Server.Helpers;

public static class ConsoleLogger
{
    public static void Info(string message)
    {
        Print(message, ConsoleColor.Cyan, "INFO");
    }

    public static void Success(string message)
    {
        Print(message, ConsoleColor.Green, "JOIN");
    }

    public static void Warning(string message)
    {
        Print(message, ConsoleColor.DarkYellow, "WARN");
    }

    public static void Error(string message)
    {
        Print(message, ConsoleColor.Red, "ERROR");
    }

    public static void Message(MessagePacket packet)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{packet.Timestamp.ToLocalTime():HH:mm:ss}] ");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($"{packet.Sender}: ");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(packet.Content);

        Console.ResetColor();
    }

    private static void Print(
        string message,
        ConsoleColor color,
        string prefix)
    {
        Console.ForegroundColor = color;

        Console.WriteLine($"[{prefix}] {message}");

        Console.ResetColor();
    }
}