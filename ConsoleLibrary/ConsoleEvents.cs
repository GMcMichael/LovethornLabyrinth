using EventSystem;
using EventSystem.Events;
using EventHandler = EventSystem.EventHandler;

namespace ConsoleLibrary
{
    public class ConsoleEvents
    {
        public static ConsoleEvents Instance = new();
        public const string username = "Console";
        public ConsoleEvents() { }

        public EventHandler OnSendMessage = new();
        public EventHandler OnSendCommand = new();

        #region Event Raising
        public void SendMessage(string _message) { SendMessage(new MessageEvent(_message, username)); }
        public void SendMessage(MessageEvent messageEvent) { OnSendMessage.RaiseEvent(messageEvent); }
        public void SendCommand(CommandType commandType, string[] args) { SendCommand(new CommandEvent(commandType, args, username)); }
        public void SendCommand(CommandEvent commandEvent) { OnSendCommand.RaiseEvent(commandEvent); }
        #endregion
    }
}
