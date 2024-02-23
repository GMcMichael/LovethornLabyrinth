using EventSystem;
using EventSystem.Events;

namespace ConsoleLibrary
{
    public class ConsoleEvents : BaseEventSystem
    {
        public static ConsoleEvents Instance { get; private set; } = new();
        public const string username = "Console";
        public ConsoleEvents() { }

        public event EventHandler<ConsoleKeyInfo>? OnAlphaNumericKeyPressed;
        public event EventHandler<ConsoleKeyInfo>? OnControlKeyPressed;
        public event EventHandler? OnMenuTick;

        public void SendMessage(string _message) { SendMessage(new MessageEvent(_message, username)); }
        public void SendCommand(CommandType commandType, string[] args) { SendCommand(new CommandEvent(commandType, args, username)); }
        public void AlphaNumericKeyPressed(ConsoleKeyInfo consoleKeyInfo) { OnAlphaNumericKeyPressed?.Invoke(this, consoleKeyInfo); }
        public void ControlKeyPressed(ConsoleKeyInfo consoleKeyInfo) { OnControlKeyPressed?.Invoke(this, consoleKeyInfo); }
        public void TickMenu() { OnMenuTick?.Invoke(this, new()); }
    }
}
