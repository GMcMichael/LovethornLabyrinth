using System.Data;
using System.Text.Json.Serialization;

namespace ConsoleLibrary
{
    #region Event System
    public enum EventType
    {
        Message,
        Command
    }
    public enum CommandType
    {
        Host,
        Connect,
        Leave,
        User,
        Quit,
        Test
    }
    public class EventHandler
    {
        public event EventHandler<BaseEventArgs>? OnEvent;

        public void RaiseEvent(BaseEventArgs e) { OnEvent?.Invoke(this, e); }
    }

    public abstract class BaseEventArgs
    {
        [JsonPropertyOrder(-2)]
        [JsonInclude]
        public EventType _eventType;
        [JsonPropertyOrder(-1)]
        [JsonInclude]
        public string _user;

        public BaseEventArgs(EventType eventType, string user = "Console")
        {
            _eventType = eventType;
            _user = user;
        }
    }
    #endregion

    #region Events
    public class MessageEvent : BaseEventArgs
    {
        [JsonInclude]
        public string message;

        public MessageEvent(string _message) : base(EventType.Message) { message = _message; }
    }
    public class CommandEvent : BaseEventArgs
    {
        [JsonInclude]
        public CommandType command;
        [JsonInclude]
        public string[] args;

        public CommandEvent(CommandType _command, string[] _args) : base(EventType.Command)
        {
            command = _command;
            args = _args;
        }
    }
    #endregion

    public class ConsoleEvents
    {
        public static ConsoleEvents Instance = new();
        public ConsoleEvents() { }

        #region Event Handlers
        public EventHandler OnSendMessage = new();
        public EventHandler OnSendCommand = new();
        #endregion

        #region Event Raise
        public void SendMessage(string _message) { SendMessage(new MessageEvent(_message)); }
        public void SendMessage(MessageEvent messageEvent) { OnSendMessage.RaiseEvent(messageEvent); }
        public void SendCommand(CommandType commandType, string[] args) { SendCommand(new CommandEvent(commandType, args)); }
        public void SendCommand(CommandEvent commandEvent) { OnSendCommand.RaiseEvent(commandEvent); }
        #endregion
    }
}
