using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSystem
{
    public interface ISerializable
    {
        public string Serialize() => JsonSerializer.Serialize(this, GetType());
        public bool Test()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, GetType());
                object? result = JsonSerializer.Deserialize(json, GetType());
                return result is not null;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
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
    public abstract class BaseEventArgs : ISerializable
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
    }
}
