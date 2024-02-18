namespace EventSystem.Events.NetworkEvents
{
    public class SendDataEvent : BaseEventArgs
    {
        public string Data { get; set; }
        public SendDataEvent(string data, string username) : base(EventType.SendData, username)
        {
            Data = data;
        }
    }
}
