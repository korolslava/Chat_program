using System.Collections.Concurrent;
using System.IO;

namespace Chat.Server.Services;

public class ClientManager
{
    private readonly ConcurrentDictionary<string, StreamWriter> _clients = new();

    public bool AddClient(string userName, StreamWriter writer)
    {
        return _clients.TryAdd(userName, writer);
    }

    public bool RemoveClient(string userName)
    {
        return _clients.TryRemove(userName, out _);
    }

    public bool TryGetClient(string userName, out StreamWriter writer)
    {
        return _clients.TryGetValue(userName, out writer!);
    }

    public ICollection<string> GetOnlineUsers()
    {
        return _clients.Keys;
    }

    public ICollection<StreamWriter> GetAllWriters()
    {
        return _clients.Values;
    }
}
