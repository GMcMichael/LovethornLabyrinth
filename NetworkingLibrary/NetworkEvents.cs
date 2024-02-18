using EventSystem;

namespace NetworkingLibrary
{
    public class NetworkEvents : NetworkEventSystem
    {
        public static NetworkEvents Instance { get; private set; } = new();
        public NetworkEvents() { }

        public event EventHandler<string>? OnLog;

        public void PassLog(string message) { OnLog?.Invoke(this, message); }
    }
}
