using EventSystem;
using EventSystem.Events;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHandler = EventSystem.EventHandler;

namespace NetworkingLibrary
{

    #region Events
    public class SendDataEvent : BaseEventArgs
    {
        public string Data {  get; set; }
        public SendDataEvent(string data, string username) : base(EventType.SendData, username)
        {
            Data = data;
        }

        //public override string Serialize() { return JsonSerializer.Serialize(this); }

        public static new bool Test(bool log = false)
        {
            if(log) NetworkManager.Instance.Log("Testing SendDataEvent");
            try
            {
                string json = JsonSerializer.Serialize(new SendDataEvent("Test Data", "Test_User"));
                SendDataEvent? testSendDataEvent = JsonSerializer.Deserialize<SendDataEvent>(json);
                if (testSendDataEvent == null)
                {
                    if(log) NetworkManager.Instance.Log("SendDataEvent Failed\n");
                    return false;
                }
                if(log) NetworkManager.Instance.Log("SendDataEvent Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if(log) NetworkManager.Instance.Log($"SendDataEvent Failed\n");
                return false;
            }
        }
    }
    public class ReceiveDataEvent : BaseEventArgs
    {
        public string? Data { get; set; }
        public EventType? DataType {  get; set; }

        public ReceiveDataEvent(string data) : base(EventType.ReceiveData)
        {
            Data = data;
            try
            {
                DataType = (EventType)int.Parse(Data[(Data.IndexOf(':') + 1)..Data.IndexOf(',')]);

                int startIndex = Data.IndexOf(':', Data.IndexOf(':') + 1) + 2;
                int endIndex = Data.IndexOf(',', Data.IndexOf(',') + 1) - 1;
                string newUsername = Data[startIndex..endIndex];
                if (Username == User.defaultName && !string.IsNullOrEmpty(newUsername))
                    Username = newUsername;
            }
            catch (Exception e)
            {
                Username = User.defaultName;
                DataType = EventType.Error;
                NetworkManager.Instance.Log($"Error while receiving data:\nData: {Data}\nError: {e}");
            }
        }
        public ReceiveDataEvent() : base(EventType.ReceiveData) { }
        public ReceiveDataEvent(SendDataEvent _sendDataEvent) : this(_sendDataEvent.Data) { }

        //public override string Serialize() { return JsonSerializer.Serialize(this); }
        public static new bool Test(bool log = false)
        {
            if(log) NetworkManager.Instance.Log("Testing ReceiveDataEvent");
            try
            {
                string json = JsonSerializer.Serialize(new ReceiveDataEvent(new SendDataEvent(new MessageEvent("Test Data", "Test_User").Serialize(), "Test_User")));
                ReceiveDataEvent? testReceiveDataEvent = JsonSerializer.Deserialize<ReceiveDataEvent>(json);
                if (testReceiveDataEvent == null)
                {
                    if(log) NetworkManager.Instance.Log("ReceiveDataEvent Failed\n");
                    return false;
                }
                if(log) NetworkManager.Instance.Log("ReceiveDataEvent Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if(log) NetworkManager.Instance.Log($"ReceiveDataEvent Failed\n");
                return false;
            }
        }
    }
    public class ServerStartEvent : BaseEventArgs
    {
        public string Host {  get; set; }
        public int Port {  get; set; }

        public ServerStartEvent(string host, int port, string username) : base(EventType.HostStart, username)
        {
            Host = host;
            Port = port;
        }

        //public override string Serialize() { return JsonSerializer.Serialize(this); }
        public static new bool Test(bool log = false)
        {
            if(log) NetworkManager.Instance.Log("Testing ServerStartEvent");
            try
            {
                string json = JsonSerializer.Serialize(new ServerStartEvent(NetworkManager.GetLocalIP(), NetworkManager._defaultPort, "Test_User"));
                ServerStartEvent? testServerStartEvent = JsonSerializer.Deserialize<ServerStartEvent>(json);
                if (testServerStartEvent == null)
                {
                    if(log) NetworkManager.Instance.Log("ServerStartEvent Failed\n");
                    return false;
                }
                if(log) NetworkManager.Instance.Log("ServerStartEvent Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if(log) NetworkManager.Instance.Log($"ServerStartEvent Failed\n");
                return false;
            }
        }
    }
    public class ServerEndEvent : BaseEventArgs
    {
        public string Host { get; set; }
        public int Port { get; set; }
        [JsonConstructor]
        public ServerEndEvent(string host, int port, string username) : base(EventType.HostEnd, username)
        {
            Host = host;
            Port = port;
        }
        public ServerEndEvent(EndPoint endPoint, User user) : base(EventType.HostEnd, user.Username)
        {
            string? host = endPoint.ToString();
        }

        //public override string Serialize() { return JsonSerializer.Serialize(this); }
        public static new bool Test(bool log = false)
        {
            if(log) NetworkManager.Instance.Log("Testing ServerEndEvent");
            try
            {
                string json = JsonSerializer.Serialize(new ServerEndEvent(NetworkManager._localHost, NetworkManager._defaultPort, "Test_User"));
                ServerEndEvent? testServerEndEvent = JsonSerializer.Deserialize<ServerEndEvent>(json);
                if (testServerEndEvent == null)
                {
                   if(log)  NetworkManager.Instance.Log("ServerEndEvent Failed\n");
                    return false;
                }
                if(log) NetworkManager.Instance.Log("ServerEndEvent Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if(log) NetworkManager.Instance.Log($"ServerEndEvent Failed\n");
                return false;
            }
        }
    }
    public class ClientJoinEvent : BaseEventArgs
    {
        public string Host {  get; set; }
        public int Port { get; set; }
        public ClientJoinEvent(string host, int port, string username) : base(EventType.ClientJoin, username)
        {
            Host = host;
            Port = port;
        }

        //public override string Serialize() { return JsonSerializer.Serialize(this); }
        public static new bool Test(bool log = false)
        {
            if (log) NetworkManager.Instance.Log("Testing ClientJoinEvent");
            try
            {
                string json = JsonSerializer.Serialize(new ClientJoinEvent(NetworkManager._localHost, NetworkManager._defaultPort, "Test_User"));
                ClientJoinEvent? testClientJoinEvent = JsonSerializer.Deserialize<ClientJoinEvent>(json);
                if (testClientJoinEvent == null)
                {
                    if (log) NetworkManager.Instance.Log("ClientJoinEvent Failed\n");
                    return false;
                }
                if (log) NetworkManager.Instance.Log("ClientJoinEvent Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if (log) NetworkManager.Instance.Log($"ClientJoinEvent Failed\n");
                return false;
            }
        }
    }
    public class ClientLeaveEvent : BaseEventArgs
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ClientLeaveEvent() : base(EventType.ClientLeave) { }
        public ClientLeaveEvent(string host, int port, string username) : base(EventType.ClientLeave, username)
        {
            Host = host;
            Port = port;
        }
        public ClientLeaveEvent(EndPoint endPoint, User user) : base(EventType.ClientLeave, user.Username)
        {
            string? host = endPoint.ToString();
        }

        //public override string Serialize() { return JsonSerializer.Serialize(this); }
        public static new bool Test(bool log = false)
        {
            if (log) NetworkManager.Instance.Log("Testing ClientLeaveEvent");
            try
            {
                string json = JsonSerializer.Serialize(new ClientLeaveEvent(NetworkManager._localHost, NetworkManager._defaultPort, "Test_User"));
                ClientLeaveEvent? testClientLeaveEvent = JsonSerializer.Deserialize<ClientLeaveEvent>(json);
                if (testClientLeaveEvent == null)
                {
                    if (log) NetworkManager.Instance.Log("ClientLeaveEvent Failed\n");
                    return false;
                }
                if (log) NetworkManager.Instance.Log("ClientLeaveEvent Passed\n");
                return true;
            }
            catch (Exception e)
            {
                if (log) NetworkManager.Instance.Log($"ClientLeaveEvent Failed\n");
                return false;
            }
        }
    }
    #endregion

    public class NetworkEvents
    {
        public static NetworkEvents Instance = new();
        public NetworkEvents() { }

        #region Event Handlers
        public event EventHandler<string>? OnLog;

        public EventHandler OnDataReceived = new();
        public EventHandler OnSendData = new();

        public EventHandler OnCommandRecieved = new();
        public EventHandler OnMessageRecieved = new();

        public EventHandler OnClientJoin = new();
        public EventHandler OnClientLeave = new();

        public EventHandler OnServerStart = new();
        public EventHandler OnServerEnd = new();
        #endregion

        #region Event Raise
        public void PassLog(string message) { OnLog?.Invoke(this, message); }
        public void DataReceived(ReceiveDataEvent dataEvent) { OnDataReceived.RaiseEvent(dataEvent); }
        public void SendData(SendDataEvent dataEvent) { OnSendData.RaiseEvent(dataEvent); }
        public void MessageReceived(MessageEvent message) { OnMessageRecieved.RaiseEvent(message); }
        public void CommandRecieved(CommandEvent command) { OnCommandRecieved.RaiseEvent(command); }
        public void ServerStarted(ServerStartEvent hostStart) { OnServerStart.RaiseEvent(hostStart); }
        public void ServerEnded(ServerEndEvent hostEnd) { OnServerEnd.RaiseEvent(hostEnd); }
        public void ClientJoined(ClientJoinEvent clientJoinEvent) { OnClientJoin.RaiseEvent(clientJoinEvent); }
        public void ClientLeft(ClientLeaveEvent clientLeaveEvent) { OnClientLeave.RaiseEvent(clientLeaveEvent); }
        #endregion
    }
}
