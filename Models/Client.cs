using Fleck;
namespace Models
{
    public class Client
    {
        public Guid ID;
        public required string IP;
        public required IWebSocketConnection Socket;
    }
}

