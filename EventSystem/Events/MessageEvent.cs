namespace EventSystem.Events
{
    public class MessageEvent : BaseEventArgs
    {
        public string Message { get; set; }
        public MessageEvent(string message, string username) : base(EventType.Message, username)
        {
            Message = message;
        }
    }
}
