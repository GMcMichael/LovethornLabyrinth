using System.Text.Json;

namespace EventSystem.Events
{
    public class MessageEvent : BaseEventArgs
    {
        public string Message { get; set; }
        public MessageEvent(string message, string username) : base(EventType.Message, username)
        {
            Message = message;
        }
        public static new bool Test(bool log = false)
        {
            try
            {
                string json = new MessageEvent("Test Message", "Test_User").Serialize();
                MessageEvent? testMessageEvent = JsonSerializer.Deserialize<MessageEvent>(json);
                return testMessageEvent != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
