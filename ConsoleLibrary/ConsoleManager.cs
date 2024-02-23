using EventSystem;
using System.Runtime.InteropServices;

namespace ConsoleLibrary
{
    public class ConsoleManager
    {
        public static ConsoleManager Instance { get; private set; } = new();
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
        private StringFrame? ColorInfoFrame;
        private StringFrame? CoverFrame;
        public PriorityFrame? MainFrame { get; set; }
        public List<MenuFrame> MenuFrames { get; private set; } = new();
        #endregion

        #region Log Functions
        public static void Log(string message, bool save = true)
        {
            if (save) Instance.SaveLog(message);
            //if(IsConsole) Console.WriteLine(GetStamp() + message);
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
        public void Init(int[] size, string AppPath, bool saveLogs = false)
        {
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = true;

            InitLog(AppPath, saveLogs);
            SetSize(size);
            InitColorFrame((size[0] / 2) + 4, 5, 16, 24, true);
            InitCoverFrame(size[0], size[1]);

            ConsoleEvents.Instance.OnKeyPressed += OnKeyPressed;
        }
        public void Start()
        {
            if (_running) return;
            _running = true;
            SetMouseInput(false);

            SetupInput();
            RunDisplay();

            SetMouseInput(true);
        }
        private void SetupInput()
        {
            Task.Run(async () =>
            {
                while (_running)
                {
                    if (!Console.KeyAvailable) { await Task.Yield(); continue; }
                    ConsoleEvents.Instance.KeyPressed(Console.ReadKey(true).Key);
                }
            });
        }
        private void OnKeyPressed(object? sender, ConsoleKey e)
        {
            bool horizontal = true;
            bool forward = true;
            MenuFrame? currMenu = MenuFrames.Count > 0 ? MenuFrames[^1] : null;
            switch (e)
            {
                case ConsoleKey.Enter:
                    if (currMenu != null)
                        currMenu.Select();
                    break;
                case ConsoleKey.DownArrow:
                    horizontal = false;
                    goto case ConsoleKey.RightArrow;
                case ConsoleKey.UpArrow:
                    horizontal = false;
                    goto case ConsoleKey.LeftArrow;
                case ConsoleKey.LeftArrow:
                    forward = false;
                    goto case ConsoleKey.RightArrow;
                case ConsoleKey.RightArrow:
                    if(currMenu != null)
                    {
                        if (!TryMoveFrames(currMenu, horizontal, forward))
                            currMenu.MoveSelection(forward ? 1 : -1);
                    } else// do other
                    {

                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Helper Functions
        private bool TryMoveFrames(MenuFrame? menu, bool horizontal, bool forward)
        {
            if(menu?.Aligns(horizontal) ?? false) return false;
            // try and move menus somehow
            // maybe have a sorted list of all menus on screen
            return true;
        }
        public string GetFileId() { return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); }
        public void SetSize(int width, int height)
        {
            Console.SetWindowSize(width, height);
            _displaySize = (width, height);
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
                CoverFrame.Push(ConsoleRow.Fill(width, '░', ConsoleColor.Red));
        }
        public void CoverScreen() { CoverFrame?.Render(null); }
        #endregion

        #region Display Functions
        private void RunDisplay()
        {
            while (_running)
            {
                RenderLoop();
                ConsoleEvents.Instance.TickMenu();
            }
        }
        private void RenderLoop()
        {
            if (MainFrame == null) return;

            if (MenuFrames.Count > 0) MenuFrames[^1].SetFocus();
            MainFrame.Render(MainFrame.EmptyMask);
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
        public void PushMenu(MenuFrame frame) { MainFrame?.PushTop(frame); MenuFrames.Add(frame); }
        public void PopMenu(MenuFrame? frame = null)
        {
            if (frame != null)
            {
                MainFrame?.Pop(frame);
                MenuFrames.Remove(frame);
            }
            else if (MenuFrames.Count > 0)
            {
                MainFrame?.Pop(MenuFrames[^1]);
                MenuFrames.RemoveAt(MenuFrames.Count - 1);
            }
        }
        public void SetFrame(PriorityFrame frame) { MainFrame = frame; }
        public StringFrame? ColorInfo() { return ColorInfoFrame; }
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
