using System;
using System.Text.Json;
using Chat.Shared;
using Chat.Server.Helpers;
namespace Chat.Server.Services;

public class BroadcastService
{
    private readonly ClientManager _clientManager;
    public BroadcastService(ClientManager clientManager)
    {
        _clientManager = clientManager;
    }
    public async Task BroadcastAsync(MessagePacket packet)
    {
        string json = JsonSerializer.Serialize(packet);
        foreach (var writer in _clientManager.GetAllWriters())
        {
            try
            { 
                await writer.WriteLineAsync(json); 
        }
            catch (Exception ex) 
            { 
                ConsoleLogger.Error(ex.Message); 
            }
        }
    }
}