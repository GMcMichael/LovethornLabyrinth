namespace EventSystem.Events.NetworkEvents
{
    public class ServerStartEvent : BaseEventArgs
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public ServerStartEvent(string host, int port, string username) : base(EventType.HostStart, username)
        {
            Host = host;
            Port = port;
        }
    }
}
