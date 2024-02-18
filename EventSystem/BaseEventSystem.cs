using EventSystem.Events;

namespace EventSystem
{
    public abstract class BaseEventSystem
    {
        public const string DEFAULT_USERNAME = "Default_User";
        #region Event Handlers
        public BaseEventHandler OnCommandRecieved = new();
        public BaseEventHandler OnCommandSend = new();
        public BaseEventHandler OnMessageRecieved = new();
        public BaseEventHandler OnMessageSend = new();
        #endregion

        #region Event Raising
        public void MessageReceived(MessageEvent message) { OnMessageRecieved.RaiseEvent(message); }
        public void SendMessage(MessageEvent messageEvent) { OnMessageSend.RaiseEvent(messageEvent); }
        public void CommandRecieved(CommandEvent command) { OnCommandRecieved.RaiseEvent(command); }
        public void SendCommand(CommandEvent commandEvent) { OnCommandSend.RaiseEvent(commandEvent); }
        #endregion
    }
}
