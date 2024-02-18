using System.Text.Json;

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

        public static new bool Test(bool log = false)
        {
            try
            {
                string json = new CommandEvent(CommandType.Test, Array.Empty<string>(), "Test_User").Serialize();
                CommandEvent? testCommandEvent = JsonSerializer.Deserialize<CommandEvent>(json);
                return testCommandEvent != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
