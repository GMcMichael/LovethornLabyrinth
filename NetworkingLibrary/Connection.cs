namespace NetworkingLibrary
{
    public class Connection : EventSystem.ISerializable
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public Connection(string host = NetworkManager._localHost, int port = NetworkManager._defaultPort)
        {
            Host = host;
            Port = port;
        }
        public override string ToString() { return $"Host: {Host}, Port: {Port}"; }
    }
}
