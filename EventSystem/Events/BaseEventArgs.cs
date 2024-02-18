using System.Text.Json.Serialization;

namespace EventSystem.Events
{
    public abstract class BaseEventArgs : ISerializable
    {
        [JsonPropertyOrder(-2)]
        public EventType EventType { get; set; }
        [JsonPropertyOrder(-1)]
        public string Username { get; set; }

        public BaseEventArgs(EventType eventType, string username = BaseEventSystem.DEFAULT_USERNAME)
        {
            EventType = eventType;
            Username = username;
        }
    }
}
