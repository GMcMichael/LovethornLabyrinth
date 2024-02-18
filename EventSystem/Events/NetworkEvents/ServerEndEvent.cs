using System.Net;
using System.Text.Json.Serialization;

namespace EventSystem.Events.NetworkEvents
{
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
        public ServerEndEvent(EndPoint? endPoint, string username) : base(EventType.HostEnd, username)
        {
            if(endPoint is not null)
            {
                string str = endPoint.ToString();
                Host = str[..str.IndexOf(':')];
                Port = int.Parse(str[(str.IndexOf(':') + 1)..]);
            } else
            {
                Host = "127.0.0.1";
                Port = 48888;
            }
        }
    }
}
