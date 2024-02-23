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

        private int[] ConsoleDimensions = { 120, 30 };

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

            InitStartFrame();

            consoleManager.Start();
        }

        public void InitEvents()
        {
            NetworkEvents.Instance.OnLog += SendLogToConsole;
            NetworkEvents.Instance.OnMessageRecieved.OnEvent += OnMessageRecieved;
            NetworkEvents.Instance.OnServerStart.OnEvent += OnServerStart;
            NetworkEvents.Instance.OnServerEnd.OnEvent += OnServerEnd;

            ConsoleEvents.Instance.OnMessageSend.OnEvent += OnSendMessage;
            ConsoleEvents.Instance.OnCommandSend.OnEvent += OnSendCommand;
        }
        #endregion

        #region Helper Functions
        private void Log(string message) { ConsoleManager.Log(message); }
        private void InitStartFrame()
        {
            consoleManager.CoverScreen();

            PriorityFrame priorityFrame = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);

            StringFrame WelcomeText = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);
            WelcomeText.PushEmpty(3);
            WelcomeText.PushCenter(new ConsoleString[]
            {
                new("Select an option:"),
            });

            int menuWidth = 50;
            int menuX = (ConsoleDimensions[0] - menuWidth) / 2;
            int menuY = ConsoleDimensions[1] - (ConsoleDimensions[1] / 3);
            MenuFrame menuFrame = new(new MenuOption[]
            {
                new(new("Start"), MenuOption.EmptyAction), //TODO: go though join / host menu, then end at screen showing chatroom
                new(new("Change Name"), () =>
                {
                    MenuFrame nameChange = new(new MenuOption[]
                    {
                        new(new("Cancel"), () =>
                        {
                            consoleManager.PopMenu();
                        }),
                        new MenuString("Enter new name...", 20),//pass current username instead
                        new(new("Enter"), () =>
                        {
                            //TODO: update name from the menu string text
                            consoleManager.PopMenu();
                        })
                    }, true, false, menuX, menuY - 5, menuWidth, 1);
                    consoleManager.PushMenu(nameChange);
                }),
                new(new("Quit"), () => { consoleManager._running = false; ConsoleEvents.Instance.SendCommand(new(CommandType.Quit)); }),
            }, true, true, menuX, menuY, menuWidth, 1);

            priorityFrame.Push(WelcomeText);
            consoleManager.SetFrame(priorityFrame);

            consoleManager.PushMenu(menuFrame);
        }
        private void InitLovethornFrame()
        {
            consoleManager.CoverScreen();

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
        private void OnServerStart(object? sender, BaseEventArgs e)
        {
            ServerStartEvent serverEvent = (ServerStartEvent)e;
            Log("Server Started");
        }
        private void OnServerEnd(object? sender, BaseEventArgs e)
        {
            ServerEndEvent serverEvent = (ServerEndEvent)e;
            Log("Server Ended");
        }
        private void OnSendCommand(object? sender, BaseEventArgs e)
        {
            NetworkEvents.Instance.CommandRecieved((CommandEvent)e);
        }

        private void OnSendMessage(object? sender, BaseEventArgs e)
        {
            e.Username = NetworkManager.Instance._clientConnection._user.Username;
            NetworkManager.Instance.AddDataToSend(new SendDataEvent(((ISerializable) e).Serialize(), e.Username));
        }
        #endregion
    }
}
