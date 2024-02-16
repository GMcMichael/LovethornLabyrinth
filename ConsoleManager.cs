using System.Runtime.InteropServices;

namespace NetworkingTest
{
    public class ConsoleManager
    {
        public enum UpdateType
        {
            Interval,
            Required
        }
        public enum DefaultDisplayUpdateTimes
        {
            Very_Fast = 1000,
            Fast = 5000,
            Normal = 10000,
            Slow = 30000,
            Very_Slow = 60000
        }
        private uint ENABLE_QUICK_EDIT = 0x0040;

        private bool _debugging = false;
        public bool _running = true;
        public char _commandChar = '/';
        public int _delta = 16;
        public int _displayDelta = (int)DefaultDisplayUpdateTimes.Fast;
        private int _displayTimer = 0;
        public UpdateType _updateType = UpdateType.Interval;

        public static ConsoleManager Instance = new();

        public ConsoleManager() { }

        public void Start()
        {
            if(_running) return;
            _running = true;
            SetMouseInput(false);

            Task.Run(async () =>
            {
                while (_running)
                {
                    if (!HandleInput()) await Task.Delay(_delta);
                }

                SetMouseInput(true);
            });
        }

        public string HelpInfo()
        {
            return "Command Prefix: '" + _commandChar + @"'
             Commands:
                 [h]elp - show this text
                 [u]ser (name) - sets display username
                 host (ip) (socket) - host on the specified ip and socket
                 [c]onnect (ip) (socket) - connect to specified ip and socket
                 [q]uit - exit the console
                 ";
        }

        public void Wait()
        {
            Console.WriteLine("Press Any Key to Continue...");
            Console.ReadKey();
        }

        public static string GetStamp()
        {
            return "[" + DateTime.Now.ToString("hh:mm:ss") + "] - ";
        }

        public static void Log(string _msg = "...")
        {
            Console.WriteLine(GetStamp() +  _msg);
        }
        
        private bool CheckTimer()
        {
            bool res = _displayTimer >= _displayDelta;
            if (res) { _displayTimer = 0; }
            return res;
        }

        public bool HandleInput()
        {
            _displayTimer += (_updateType == UpdateType.Interval) ? _delta : 0;

            if (!Console.KeyAvailable)
            {
                if(CheckTimer()) Log("");
                return false;
            }

            Console.Write(GetStamp());
            string? msg = Console.ReadLine();
            if (String.IsNullOrEmpty(msg)) return false;

            if (msg[0] == _commandChar) ProcessCommand(msg.ToLower().Substring(1));
            else if (msg == "help" || msg == "h") Log(HelpInfo());
            else
            {
                NetworkManager.Instance.AddMessage(msg);
            }

            return true;
        }

        public void ProcessCommand(string cmd)
        {
            string[] args = cmd.Split(' ');
            if (_debugging) Log("Handling Command: " + cmd);
            switch(args[0])
            {
                case "t":
                case "test":

                    break;
                case "l":
                case "leave":
                    NetworkManager.Instance._clientConnection?.CloseConnection();
                    break;
                case "c":
                case "connect":
                    NetworkManager.Instance.ClientConnect(args.Length > 1 ? args[1] : null, args.Length > 2 ? int.Parse(args[2]) : null);
                    break;
                case "host":
                    NetworkManager.Instance.HostServer(args.Length > 1 ? args[1] : null, args.Length > 2 ? int.Parse(args[2]) : null);
                    break;
                case "u":
                case "user":
                    if (args.Length == 1)
                        Log($"User: {NetworkManager.Instance._user.Username}");
                    else
                        NetworkManager.Instance.ChangeUser(args.Length > 1 ? args[1] : null);
                    break;
                case "h":
                case "help":
                    Log(HelpInfo());
                    break;
                case "q":
                case "quit":
                    _running = false;
                    Log("Exiting...");
                    NetworkManager.Instance.CloseSocket();
                    Log("Closing Socket...");
                    break;
                default:
                    Log("Unknown Command ( " + cmd + " ).");
                    Log(HelpInfo());
                    break;
            }
        }



        // below is to stop console freeze on mouse click --------------------------------------------------------------------------------------

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public void SetMouseInput(bool enable)
        {
            IntPtr consoleHandle = GetStdHandle(-10);// -10 is the standard input device.
            uint mode;
            GetConsoleMode(consoleHandle, out mode);

            mode &= enable ? ENABLE_QUICK_EDIT : ~ENABLE_QUICK_EDIT; // set quick edit to disabel mouse clicks

            SetConsoleMode(consoleHandle, mode);
        }
    }
}
