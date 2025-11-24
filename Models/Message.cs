public enum EventMessage
{
    Ping
}

namespace Models
{
    public class Message<T>
    {
        public EventMessage Event { get; set; }
        public T Data { get; set; }
    }
}

