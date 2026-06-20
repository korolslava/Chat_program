using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Data;

public class ChatDbContext : DbContext
{
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=localhost;Database=TcpChatDb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);
    }
}