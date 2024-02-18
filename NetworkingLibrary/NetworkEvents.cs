using EventSystem;
using EventSystem.Events;
using System.Net;
using System.Text.Json.Serialization;
using BaseEventHandler = EventSystem.BaseEventHandler;

namespace NetworkingLibrary
{
    #region Events
    public class SendDataEvent : BaseEventArgs
    {
        public string Data { get; set; }
        public SendDataEvent(string data, string username) : base(EventType.SendData, username)
        {
            Data = data;
        }
    }
    public class ReceiveDataEvent : BaseEventArgs
    {
        public string? Data { get; set; }
        public EventType? DataType { get; set; }
        [JsonConstructor]
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
        public ReceiveDataEvent(SendDataEvent _sendDataEvent) : this(_sendDataEvent.Data) { }
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
    }
    public class ClientLeaveEvent : BaseEventArgs
    {
        public string Host { get; set; }
        public int Port { get; set; }
        [JsonConstructor]
        public ClientLeaveEvent(string host, int port, string username) : base(EventType.ClientLeave, username)
        {
            Host = host;
            Port = port;
        }
        public ClientLeaveEvent(EndPoint endPoint, User user) : base(EventType.ClientLeave, user.Username)
        {
            string? host = endPoint.ToString();
        }
    }
    #endregion

    public class NetworkEvents : BaseEventSystem
    {
        public static NetworkEvents Instance { get; private set; } = new();
        public NetworkEvents() { }

        #region Event Handlers
        public event EventHandler<string>? OnLog;

        public BaseEventHandler OnDataReceived = new();
        public BaseEventHandler OnSendData = new();

        public BaseEventHandler OnClientJoin = new();
        public BaseEventHandler OnClientLeave = new();

        public BaseEventHandler OnServerStart = new();
        public BaseEventHandler OnServerEnd = new();
        #endregion

        #region Event Raise
        public void PassLog(string message) { OnLog?.Invoke(this, message); }
        public void DataReceived(ReceiveDataEvent dataEvent) { OnDataReceived.RaiseEvent(dataEvent); }
        public void SendData(SendDataEvent dataEvent) { OnSendData.RaiseEvent(dataEvent); }
        public void ServerStarted(ServerStartEvent hostStart) { OnServerStart.RaiseEvent(hostStart); }
        public void ServerEnded(ServerEndEvent hostEnd) { OnServerEnd.RaiseEvent(hostEnd); }
        public void ClientJoined(ClientJoinEvent clientJoinEvent) { OnClientJoin.RaiseEvent(clientJoinEvent); }
        public void ClientLeft(ClientLeaveEvent clientLeaveEvent) { OnClientLeave.RaiseEvent(clientLeaveEvent); }
        #endregion
    }
}
