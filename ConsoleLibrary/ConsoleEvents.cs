using EventSystem;
using EventSystem.Events;

namespace ConsoleLibrary
{
    public class ConsoleEvents : BaseEventSystem
    {
        public static ConsoleEvents Instance { get; private set; } = new();
        public const string username = "Console";
        public ConsoleEvents() { }

         public void SendMessage(string _message) { SendMessage(new MessageEvent(_message, username)); }
         public void SendCommand(CommandType commandType, string[] args) { SendCommand(new CommandEvent(commandType, args, username)); }
    }
}
