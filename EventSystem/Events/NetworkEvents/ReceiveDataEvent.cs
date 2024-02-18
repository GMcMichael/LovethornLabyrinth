using System.Text.Json.Serialization;

namespace EventSystem.Events.NetworkEvents
{
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
                if (Username == BaseEventSystem.DEFAULT_USERNAME && !string.IsNullOrEmpty(newUsername))
                    Username = newUsername;
            }
            catch (Exception e)
            {
                Username = BaseEventSystem.DEFAULT_USERNAME;
                DataType = EventType.Error;
            }
        }
        public ReceiveDataEvent(SendDataEvent _sendDataEvent) : this(_sendDataEvent.Data) { }
    }
}
