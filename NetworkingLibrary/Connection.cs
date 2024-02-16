using System.Text.Json;

namespace NetworkingLibrary
{
    public class Connection
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public Connection(string host = NetworkManager._localHost, int port = NetworkManager._defaultPort)
        {
            Host = host;
            Port = port;
        }
        public override string ToString() { return $"Host: {Host}, Port: {Port}"; }
        public string Serialize() { return JsonSerializer.Serialize(this); }
        public static bool Test(bool log = false)
        {
            if (log) NetworkManager.Instance.Log("Testing Connection");
            try
            {
                string json = JsonSerializer.Serialize(new Connection());
                Connection? testConnection = JsonSerializer.Deserialize<Connection>(json);
                if (testConnection == null)
                {
                    if (log) NetworkManager.Instance.Log($"Connection Failed\n");
                    return false;
                }
                if (log) NetworkManager.Instance.Log("Connection Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if (log) NetworkManager.Instance.Log($"Connection Failed\n");
                return false;
            }
        }
    }
}
