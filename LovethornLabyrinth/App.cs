using NetworkingLibrary;
using ConsoleLibrary;
using EventSystem;
using EventSystem.Events;
using EventSystem.Events.NetworkEvents;

namespace LovethornLabyrinth
{
    internal class App
    {
        private const string APP_NAME = "NetworkingTest";
        private string AppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
        
        private bool SaveLogs = false;

        private int[] ConsoleDimensions = { 120, 30 }; //TODO: this should be gotten at runtime dynamically to resize the console text to the actual window

        ConsoleManager consoleManager = ConsoleManager.Instance;
        NetworkManager networkManager = NetworkManager.Instance;
        // Game should be a simple maze (with enemies and gold), 5 levels (4x4, 8x8, 12x12, 16x16, 20x20). Each level has a shop, stone door down, and a key (can also buy a key).
        //GameManager gameManager = GameManager.Instance;

        public App() { Directory.CreateDirectory(AppPath); }

        #region Startup Functions
        public void Start()
        {
            InitEvents();
            consoleManager.Init(ConsoleDimensions, AppPath, SaveLogs);
            networkManager.Init(AppPath);

            consoleManager.CoverScreen();
            InitStartFrame();

            consoleManager.Start();
        }

        public void InitEvents()
        {
            NetworkEvents.Instance.OnLog += SendLogToConsole;
            NetworkEvents.Instance.OnMessageRecieved.OnEvent += OnMessageRecieved;
            NetworkEvents.Instance.OnServerStart.OnEvent += OnServerStart;
            NetworkEvents.Instance.OnClientJoin.OnEvent += OnClientJoin;

            ConsoleEvents.Instance.OnMessageSend.OnEvent += OnSendMessage;
            ConsoleEvents.Instance.OnCommandSend.OnEvent += OnSendCommand;

            NetworkEvents.Instance.OnServerEnd.OnEvent += OnServerEnd;
            NetworkEvents.Instance.OnClientLeave.OnEvent += OnClientLeave;
        }
        #endregion

        #region Helper Functions
        private void Log(string message) { ConsoleManager.Log(message); }
        #endregion

        #region Frames

        private void InitStartFrame()
        {
            PriorityFrame priorityFrame = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);

            StringFrame WelcomeText = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);
            WelcomeText.PushEmpty(3);
            WelcomeText.PushCenter(new ConsoleString[]
            {
                new("Chatroom:"),
            });

            int menuWidth = 50;
            int menuHeightOffset = 5;
            int menuX = (ConsoleDimensions[0] - menuWidth) / 2;
            int menuY = ConsoleDimensions[1] - (ConsoleDimensions[1] / 3) - menuHeightOffset * 2;
            MenuFrame menuFrame = new([
                new(new("Start"), frame =>
                {
                    consoleManager.PushMenu(new([
                        new(new("Join"), frame =>
                        {
                            string host = "";
                            string port = NetworkManager._defaultPort.ToString();
                            consoleManager.PushMenu(new([
                                new MenuString("Enter IP here", host, (str, field) => host = str),
                                new MenuString("Port", port, 5, MenuString.Mode.Numbers, (str, field) => port = str),
                                new(new("Join"), frame => ConsoleEvents.Instance.SendCommand(new(CommandType.Connect, [string.IsNullOrEmpty(host) ? NetworkManager.GetLocalIP() : host, port]))),
                                MenuOption.Back
                                ], true, false, menuX, menuY + menuHeightOffset * 2, menuWidth));
                        }),
                        new(new("Host"), frame =>
                        {
                            string port = NetworkManager._defaultPort.ToString();
                            consoleManager.PushMenu(new([
                                new MenuString("Port", port, 5, MenuString.Mode.Numbers, (str, field) => port = str),
                                new(new("Host"), frame => ConsoleEvents.Instance.SendCommand(new(CommandType.Host, [NetworkManager.GetLocalIP(), port]))),
                                MenuOption.Back
                                ], true, false, menuX, menuY + menuHeightOffset * 2, menuWidth));
                        }),
                        MenuOption.Back
                    ], true, true, menuX, menuY + menuHeightOffset, menuWidth));
                }),
                new(new("Change Name"), frame =>
                {
                    consoleManager.PushMenu(new([
                        new MenuString("Username", networkManager._clientConnection._user.Username, (str, field) => { ConsoleEvents.Instance.SendCommand(new(CommandType.User, [str])); }),
                        MenuOption.Back
                    ], true, false, menuX, menuY + menuHeightOffset, menuWidth));
                }),
                new(new("Quit"), frame => { consoleManager._running = false; ConsoleEvents.Instance.SendCommand(new(CommandType.Quit)); }),
            ], true, true, menuX, menuY, menuWidth);

            priorityFrame.Push(WelcomeText);
            consoleManager.SetFrame(priorityFrame);

            consoleManager.PushMenu(menuFrame);
        }
        private void InitConsoleFrame()
        {//TODO: make the chatroom here
            PriorityFrame mainFrame = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);

            int footerHeight = 5;
            int usersWidth = 20;
            //TODO This should be a scrollable menu frame
            StringFrame users = new(1, 1, usersWidth, ConsoleDimensions[1] - 5 - footerHeight, true);
            users.PushCenter([
                new("User 1"),
                new("User 2"),
                new("User 3"),
                new("User 4"),
                new("User 5"),
            ]);
            
            //should be a scrollable frame
            StringFrame chatLog = new(usersWidth + 3, 1, ConsoleDimensions[0] - (usersWidth + 5), ConsoleDimensions[1] - (footerHeight + 5), true);
            chatLog.PushEmpty(3);
            chatLog.Push(new("User 1: Hey!"));
            chatLog.Push(new("User 1: Hello!"));

            MenuFrame chatUI = new([
                new(new("Options"), frame =>
                {
                    consoleManager.PushMenu(new([
                        new(new("Leave"), frame => { ConsoleEvents.Instance.SendCommand(CommandType.Leave, []); }),
                        new(new("Users"), frame => { /*consoleManager.PushMenu(users);*/ }),
                        MenuOption.Back
                    ], true, false, 0, ConsoleDimensions[1] - (footerHeight + 2), ConsoleDimensions[0]));
                }),
                new MenuString("Message", "", (str, field) => { field.ClearText(); NetworkEvents.Instance.SendMessage(new(str, networkManager._clientConnection._user.Username)); })
                ], true, false, 0, ConsoleDimensions[1] - (footerHeight - 2), ConsoleDimensions[0]);


            mainFrame.Push(users);
            mainFrame.Push(chatLog);

            consoleManager.SetFrame(mainFrame);

            consoleManager.PushMenu(chatUI);
        }
        private void InitLovethornFrame()
        {
            PriorityFrame priorityFrame = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);

            StringFrame WelcomeFrame = new(0, 0, ConsoleDimensions[0] / 2, ConsoleDimensions[1], true);

            WelcomeFrame.PushEmpty();
            WelcomeFrame.PushCenter(new ConsoleString[]
            {
                new ConsoleString("Welcome to "),
                new ConsoleString("Lovethorn ", ConsoleColor.Magenta),
                new ConsoleString("Labyrinth", ConsoleColor.DarkYellow),
            });

            priorityFrame.Push(WelcomeFrame);
            StringFrame? ColorInfo = consoleManager.ColorInfo();
            if (ColorInfo != null) priorityFrame.Push(ColorInfo);

            consoleManager.SetFrame(priorityFrame);
        }
        #endregion

        #region Event Links
        private void SendLogToConsole(object? sender, string message) { Log(message); }
        private void OnMessageRecieved(object? sender, BaseEventArgs e)
        {
            MessageEvent messageEvent = (MessageEvent)e;
            Log($"{messageEvent.Username}: {messageEvent.Message}");
        }
        private void OnServerStart(object? sender, BaseEventArgs e) { InitConsoleFrame();}
        private void OnClientJoin(object? sender, BaseEventArgs e) { InitConsoleFrame(); }
        private void OnSendCommand(object? sender, BaseEventArgs e) { NetworkEvents.Instance.CommandRecieved((CommandEvent)e); }
        private void OnClientLeave(object? sender, BaseEventArgs e) { InitStartFrame(); }
        private void OnServerEnd(object? sender, BaseEventArgs e) { InitStartFrame(); }
        private void OnSendMessage(object? sender, BaseEventArgs e)
        {
            e.Username = NetworkManager.Instance._clientConnection._user.Username;
            NetworkManager.Instance.AddDataToSend(new SendDataEvent(((ISerializable) e).Serialize(), e.Username));
        }
        #endregion
    }
}
