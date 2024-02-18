using EventSystem;
using System.Runtime.InteropServices;

namespace ConsoleLibrary
{
    public class ConsoleManager
    {
        public static ConsoleManager Instance { get; private set; } = new();
        public static bool IsConsole = true;
        public ConsoleManager() { }

        #region Const Variables
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
        private const uint ENABLE_QUICK_EDIT = 0x0040;

        public const string _logFileName = "ConsoleLog_";
        public const string _logFileExt = ".txt";
        public readonly int[] _defaultSize = [120, 30];
        #endregion

        #region Variables
        private string? APP_PATH;
        private string _currLogFile = "";
        private bool _saveLogs = true;

        public (int, int) _displaySize = (0,0);
        private bool _debugging = false;
        public bool _running = false;
        public bool testing = false;
        #endregion

        #region Console Variables
        public char _commandChar = '/';
        public int _delta = 16;
        public int _displayDelta = (int)DefaultDisplayUpdateTimes.Fast;
        private int _displayTimer = 0;
        public UpdateType _updateType = UpdateType.Interval;
        #endregion

        #region Display Variables
        private ConsoleColor TextColor = ConsoleColor.White;
        private ConsoleColor BackgroundColor = ConsoleColor.Black;
        private FrameBase? _frame;
        private StringFrame? ColorInfoFrame;
        private StringFrame? CoverFrame;
        #endregion

        #region Log Functions
        public static void Log(string message, bool save = true)
        {
            if (save) Instance.SaveLog(message);
            if(IsConsole) Console.WriteLine(GetStamp() + message);
        }
        public void InitLog(string AppPath, bool saveLogs = false) { APP_PATH = AppPath; _saveLogs = saveLogs; _currLogFile = GetFileId(); }
        public void SaveLog(string message)
        {
            if (!_saveLogs || string.IsNullOrEmpty(APP_PATH)) return;
            //save to log file
            using (StreamWriter sw = new StreamWriter(Path.Combine(APP_PATH, _logFileName + _currLogFile + _logFileExt), true))
            {
                sw.WriteLine(message);
            }
        }
        #endregion

        #region Main Functions
        public void Init(int[] size, string AppPath, bool saveLogs = false, bool isConsole = true)
        {
            IsConsole = isConsole;
            InitLog(AppPath, saveLogs);
            SetSize(size);
            InitColorFrame((size[0] / 2) + 4, 5, 16, 24, true);
            InitCoverFrame(size[0], size[1]);
        }
        public void Start()
        {
            if (_running) return;
            _running = true;
            SetMouseInput(false);

            if(IsConsole)
                StartConsole();
            else
                StartDisplay();

            SetMouseInput(true);
        }
        #endregion

        #region Helper Functions
        public string HelpInfo()
        {
            return "Command Prefix: '" + _commandChar + @"'
             Commands:
                 help - show this text
                 [u]ser (name) - sets display username
                 [h]ost (ip) (socket) - host on the specified ip and socket
                 [c]onnect (ip) (socket) - connect to specified ip and socket
                 [q]uit - exit the console
                 ";
        }
        public static void Clear() { Console.Clear(); }
        public void Wait()
        {
            Console.WriteLine("Press Any Key to Continue...");
            Console.ReadKey();
        }
        public static string GetStamp() { return "[" + DateTime.Now.ToString("hh:mm:ss") + "] - "; }
        private bool CheckTimer()
        {
            bool res = _displayTimer >= _displayDelta;
            if (res) { _displayTimer = 0; }
            return res;
        }
        public string GetFileId() { return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); }
        public void SetSize(int width, int height)
        {
            Console.SetWindowSize(width, height);
            _displaySize = (width, height);
            if (!IsConsole)
                Console.SetBufferSize(width, height);

        }
        public void SetSize(int[] size) { if(size.Length >= 2) SetSize(size[0], size[1]); }
        private void InitColorFrame(int x, int y, int width, int height, bool border)
        {
            if (width < 16) width = 16;
            if(height < 24) height = 24;
            ColorInfoFrame = new(x, y, width, height, border);

            ColorInfoFrame.BorderTextColor = ConsoleColor.Green;

            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Black", ConsoleColor.Black, ConsoleColor.White));
            ColorInfoFrame.PushCenter(new ConsoleString("White", ConsoleColor.White));
            ColorInfoFrame.PushCenter(new ConsoleString("Gray", ConsoleColor.Gray));
            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Red", ConsoleColor.Red));
            ColorInfoFrame.PushCenter(new ConsoleString("Dark Red", ConsoleColor.DarkRed));
            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Yellow", ConsoleColor.Yellow));
            ColorInfoFrame.PushCenter(new ConsoleString("Dark Yellow", ConsoleColor.DarkYellow));
            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Blue", ConsoleColor.Blue));
            ColorInfoFrame.PushCenter(new ConsoleString("Dark Blue", ConsoleColor.DarkBlue));
            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Green", ConsoleColor.Green));
            ColorInfoFrame.PushCenter(new ConsoleString("Dark Green", ConsoleColor.DarkGreen));
            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Cyan", ConsoleColor.Cyan));
            ColorInfoFrame.PushCenter(new ConsoleString("Dark Cyan", ConsoleColor.DarkCyan));
            ColorInfoFrame.PushEmpty();
            ColorInfoFrame.PushCenter(new ConsoleString("Magenta", ConsoleColor.Magenta));
            ColorInfoFrame.PushCenter(new ConsoleString("Dark Magenta", ConsoleColor.DarkMagenta));
        }
        private void InitCoverFrame(int width, int height)
        {
            CoverFrame = new(0, 0, width, height);

            for (int i = 0; i < height; i++)
                CoverFrame.Push(ConsoleRow.Fill(width, '!', ConsoleColor.Red));
        }
        public void CoverScreen() { CoverFrame?.Render(null); }
        #endregion

        #region Display Functions
        private void StartDisplay()
        {
            Console.CursorVisible = false;
            while(_running)
            {
                RenderLoop();
                Thread.Sleep(_delta);
            }
            Console.CursorVisible = true;
        }
        private void RenderLoop()
        {
            if (_frame == null) return;
            _frame.Render(_frame.EmptyMask);
        }
        public void ClearDisplay()
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = TextColor;
            Console.BackgroundColor = BackgroundColor;

            string row = "".PadLeft(_displaySize.Item1);
            for (int i = 0; i < _displaySize.Item2; i++)
                Console.Write(row);
        }
        public void SetFrame(FrameBase frame) { _frame = frame; }
        public StringFrame? ColorInfo() { return ColorInfoFrame; }
        #endregion

        #region Console Functions
        private void StartConsole()
        {
            while (_running)
            {
                if (!HandleInput()) Thread.Sleep(_delta);
            }
        }
        public bool HandleInput()
        {
            _displayTimer += (_updateType == UpdateType.Interval) ? _delta : 0;

            if (!Console.KeyAvailable)
            {
                if (CheckTimer())
                    Log("", false);
                return false;
            }

            Console.Write(GetStamp());
            string? msg = Console.ReadLine();
            if (string.IsNullOrEmpty(msg)) return false;

            if (msg[0] == _commandChar)
                ProcessCommand(msg.ToLower().Substring(1));

            else if (msg == "help" || msg == "h")
                Log(HelpInfo(), false);

            else
                ConsoleEvents.Instance.SendMessage(msg);

            return true;
        }

        public void ProcessCommand(string cmd)
        {
            string[] args = cmd.Split(' ');
            if (_debugging) Log("Handling Command: " + cmd, false);
            if (args[0] == "help")
            {
                Log(HelpInfo(), false);
                return;
            }
            CommandType type = CommandType.Host;
            switch (args[0])
            {
                case "t":
                case "test":
                    type = CommandType.Test;
                    break;
                case "l":
                case "leave":
                    type = CommandType.Leave;
                    break;
                case "c":
                case "connect":
                    type = CommandType.Connect;
                    break;
                case "h":
                case "host":
                    type = CommandType.Host;
                    break;
                case "u":
                case "user":
                    type = CommandType.User;
                    break;
                default:
                case "q":
                case "quit":
                    _running = false;
                    Log("Exiting...");
                    type = CommandType.Quit;
                    break;
            }
            ConsoleEvents.Instance.SendCommand(type, args[1..]);
        }
        #endregion

        #region Stop Mouse Click
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
        #endregion
    }
}
