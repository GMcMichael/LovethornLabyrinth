namespace EventSystem.Events.NetworkEvents
{
    public class ClientJoinEvent : BaseEventArgs
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ClientJoinEvent(string host, int port, string username) : base(EventType.ClientJoin, username)
        {
            Host = host;
            Port = port;
        }
    }
}
