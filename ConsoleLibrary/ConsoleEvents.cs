using EventSystem;
using EventSystem.Events;
using System.Data;
using System.Text.Json.Serialization;
using CommandType = EventSystem.CommandType;
using EventHandler = EventSystem.EventHandler;

namespace ConsoleLibrary
{
    public class ConsoleEvents
    {
        public static ConsoleEvents Instance = new();
        public ConsoleEvents() { }

        #region Event Handlers
        public EventHandler OnSendMessage = new();
        public EventHandler OnSendCommand = new();
        #endregion

        #region Event Raise
        public void SendMessage(string _message) { SendMessage(new MessageEvent(_message, "Console")); }
        public void SendMessage(MessageEvent messageEvent) { OnSendMessage.RaiseEvent(messageEvent); }
        public void SendCommand(CommandType commandType, string[] args) { SendCommand(new CommandEvent(commandType, args, "Console")); }
        public void SendCommand(CommandEvent commandEvent) { OnSendCommand.RaiseEvent(commandEvent); }
        #endregion
    }
}
