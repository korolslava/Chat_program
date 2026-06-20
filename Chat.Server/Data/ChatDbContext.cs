using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Data;

public class ChatDbContext : DbContext
{
    public DbSet<ChatMessage> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=TcpChatDb;Username=postgres;Password=postgres");
    }
}