using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSystem
{
    public enum EventType
    {
        Error,
        Message,
        Command,
        SendData,
        ReceiveData,
        HostStart,
        HostEnd,
        ClientJoin,
        ClientLeave
    }
    public enum CommandType
    {
        Host,
        Connect,
        Leave,
        User,
        Quit,
        Test
    }
    public class EventHandler
    {
        public event EventHandler<BaseEventArgs>? OnEvent;

        public void RaiseEvent(BaseEventArgs e) { OnEvent?.Invoke(this, e); }
    }
    public abstract class BaseEventArgs
    {
        [JsonPropertyOrder(-2)]
        public EventType EventType { get; set; }
        [JsonPropertyOrder(-1)]
        public string Username { get; set; }

        public BaseEventArgs(EventType eventType, string username = "Default_User")
        {
            EventType = eventType;
            Username = username;
        }

        public string Serialize() { return JsonSerializer.Serialize(this); }
        public static bool Test(bool log = false) { return true; }
    }
}
