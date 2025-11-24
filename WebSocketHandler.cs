using Fleck;
using Models;
using Newtonsoft.Json;
using Handlers;

public class SocketServer
{
    private readonly WebSocketServer _server;

    private readonly Dictionary<Guid, Client> _clients = new();

    private readonly Dictionary<EventMessage, Action<Client, string>> _handlers =
        new()
        {
            { EventMessage.Ping, PosHandler.HandlePing }
        };

    public SocketServer(string url)
    {
        _server = new WebSocketServer(url);
    }

    public void Run()
    {
        _server.Start(ConfigureSocket);
        Task.Delay(-1).Wait(); // keep server alive
    }

    private void ConfigureSocket(IWebSocketConnection socket)
    {
        socket.OnOpen = () => HandleOpen(socket);
        socket.OnClose = () => HandleClose(socket);
        socket.OnMessage = (json) => HandleMessage(socket, json);
    }

    // -----------------------
    // CONNECTION HANDLERS
    // -----------------------

    private void HandleOpen(IWebSocketConnection socket)
    {
        var id = socket.ConnectionInfo.Id;
        var ip = socket.ConnectionInfo.ClientIpAddress;

        _clients[id] = new Client
        {
            ID = id,
            IP = ip,
            Socket = socket
        };

        Console.WriteLine($"Client connected: {id} ({ip})");
    }

    private void HandleClose(IWebSocketConnection socket)
    {
        var id = socket.ConnectionInfo.Id;

        if (_clients.Remove(id))
            Console.WriteLine($"Client disconnected: {id}");
    }

    // -----------------------
    // MESSAGE PROCESSING
    // -----------------------

    private void HandleMessage(IWebSocketConnection socket, string json)
    {
        var id = socket.ConnectionInfo.Id;

        if (!_clients.TryGetValue(id, out var client))
            return;

        var msg = DeserializeMessage(json);
        if (msg == null)
        {
            socket.Send("Invalid json data format");
            return;
        }

        if (_handlers.TryGetValue(msg.Event, out var handler))
        {
            handler(client, json);
        }
        else
        {
            socket.Send($"No handler for: {msg.Event}");
        }
    }

    // -----------------------
    // JSON
    // -----------------------

    private Message<dynamic>? DeserializeMessage(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<Message<dynamic>>(json);
        }
        catch
        {
            Console.WriteLine("Invalid json data format");
            return null;
        }
    }
}

