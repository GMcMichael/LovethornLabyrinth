namespace EventSystem.Events
{
    public class CommandEvent : BaseEventArgs
    {
        public CommandType Command {  get; set; }
        public string[] Args { get; set; }

        public CommandEvent(CommandType command, string[] args, string username) : base(EventType.Command, username)
        {
            Command = command;
            Args = args;
        }
    }
}
