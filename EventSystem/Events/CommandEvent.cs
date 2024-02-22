namespace EventSystem.Events
{
    public class CommandEvent : BaseEventArgs
    {
        public CommandType Command {  get; set; }
        public string[] Args { get; set; }

        public CommandEvent(CommandType command, string[]? args = null, string username = BaseEventSystem.DEFAULT_USERNAME) : base(EventType.Command, username)
        {
            Command = command;
            Args = args ?? new string[0];
        }
    }
}
