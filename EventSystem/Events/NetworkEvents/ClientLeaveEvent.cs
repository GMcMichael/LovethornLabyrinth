using System.Net;
using System.Text.Json.Serialization;

namespace EventSystem.Events.NetworkEvents
{
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
        public ClientLeaveEvent(EndPoint? endPoint, string username) : base(EventType.ClientLeave, username)
        {
            if (endPoint is not null)
            {
                string str = endPoint.ToString();
                Host = str[..str.IndexOf(':')];
                Port = int.Parse(str[(str.IndexOf(':') + 1)..]);
            }
            else
            {
                Host = "127.0.0.1";
                Port = 48888;
            }
        }
    }
}
