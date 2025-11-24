using Models;
using Newtonsoft.Json;

namespace Handlers
{
    public class PosHandler
    {
        public static void HandlePing(Client client, string rawJson)
        {
            var response = new { type = "pong", id = client.ID };
            client.Socket.Send(JsonConvert.SerializeObject(response));
        }
    }
}

