using NetworkingLibrary;
using ConsoleLibrary;

namespace LovethornLabyrinth
{
    //TODO: Merge the event systems to antoher library that both librarys can use
    internal class App // Game should be a simple maze (with enemies and gold), 5 levels (4x4, 8x8, 12x12, 16x16, 20x20). Each level has a shop, stone door down, and a key (can also buy a key).
    {
        private const string APP_NAME = "NetworkingTest";
        private string AppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
        
        private bool SaveLogs = false;

        private int[] ConsoleDimensions = { 120, 30 };

        ConsoleManager consoleManager = ConsoleManager.Instance;
        NetworkManager networkManager = NetworkManager.Instance;
        //GameManager gameManager = GameManager.Instance;

        public App() { Directory.CreateDirectory(AppPath); }

        #region Startup Functions
        public void Start()
        {
            InitEvents();
            consoleManager.Init(ConsoleDimensions, AppPath, SaveLogs, false);
            networkManager.Init(AppPath);
            consoleManager.CoverScreen();

            PriorityFrame priorityFrame = new(0, 0, ConsoleDimensions[0], ConsoleDimensions[1]);

            StringFrame WelcomeFrame = new(5, 0, ConsoleDimensions[0] / 2, ConsoleDimensions[1], true);

            WelcomeFrame.PushEmpty();
            WelcomeFrame.PushCenter(new ConsoleString[]
            {
                new ConsoleString("Welcome to "),
                new ConsoleString("Lovethorn ", ConsoleColor.Magenta),
                new ConsoleString("Labyrinth", ConsoleColor.DarkYellow),
            });

            WelcomeFrame.Move(0);

            // StringFrame WelcomeFrame2 = WelcomeFrame.DeepCopy();
            // 
            // WelcomeFrame2.PushCenter(new ConsoleString("This is a test."));
            // WelcomeFrame2.PushCenter(new ConsoleString("This is a test."));
            // WelcomeFrame2.PushCenter(new ConsoleString("This is a test."));
            // WelcomeFrame2.PushCenter(new ConsoleString("This is a test."));
            // WelcomeFrame2.PushCenter(new ConsoleString("This is a test."));
            // WelcomeFrame2.PushCenter(new ConsoleString("This is a test."));

            StringFrame WelcomeFrame2 = new(59, 0, ConsoleDimensions[0] / 2, ConsoleDimensions[1], true);

            WelcomeFrame2.PushEmpty();
            WelcomeFrame2.PushCenter(new ConsoleString[]
            {
                new ConsoleString("Welcome to "),
                new ConsoleString("Lovethorn ", ConsoleColor.Magenta),
                new ConsoleString("Labyrinth", ConsoleColor.DarkYellow),
            });

            //WelcomeFrame2.Move(59);
            
            priorityFrame.Push(WelcomeFrame2, 2);

            StringFrame TestFrame = new(50, 0, 30, 20, true);
            for(int i = 0; i < 18; i++)
                TestFrame.PushCenter(new ConsoleString("This is a Test Frame", ConsoleColor.Yellow));

            priorityFrame.Push(TestFrame, 1);

            priorityFrame.Push(WelcomeFrame);
            StringFrame? ColorInfo = consoleManager.ColorInfo();
            if(ColorInfo != null) priorityFrame.Push(ColorInfo);

            consoleManager.SetFrame(priorityFrame);
            consoleManager.Start();
        }

        public void InitEvents()
        {
            NetworkEvents.Instance.OnLog += SendLogToConsole;
            NetworkEvents.Instance.OnMessageRecieved.OnEvent += OnMessageRecieved;
            NetworkEvents.Instance.OnServerStart.OnEvent += OnServerStart;
            NetworkEvents.Instance.OnServerEnd.OnEvent += OnServerEnd;

            ConsoleEvents.Instance.OnSendMessage.OnEvent += OnSendMessage;
            ConsoleEvents.Instance.OnSendCommand.OnEvent += OnSendCommand;
        }
        #endregion

        #region Helper Functions
        private void Log(string message) { ConsoleManager.Log(message); }
        #endregion

        #region Event Links
        private void SendLogToConsole(object? sender, string message) { Log(message); }
        private void OnMessageRecieved(object? sender, NetworkingLibrary.BaseEventArgs e)
        {
            NetworkingLibrary.MessageEvent messageEvent = (NetworkingLibrary.MessageEvent)e;
            Log($"{messageEvent.User}: {messageEvent.Message}");
        }
        private void OnServerStart(object? sender, NetworkingLibrary.BaseEventArgs e)
        {
            ServerStartEvent serverEvent = (ServerStartEvent)e;
            Log("Server Started");
        }
        private void OnServerEnd(object? sender, NetworkingLibrary.BaseEventArgs e)
        {
            ServerEndEvent serverEvent = (ServerEndEvent)e;
            Log("Server Ended");
        }
        private void OnSendCommand(object? sender, ConsoleLibrary.BaseEventArgs e)
        {
            ConsoleLibrary.CommandEvent commandEvent = (ConsoleLibrary.CommandEvent)e;
            NetworkEvents.Instance.CommandRecieved(new NetworkingLibrary.CommandEvent((NetworkingLibrary.CommandType)commandEvent.command, commandEvent.args, new User(commandEvent._user)));
        }

        private void OnSendMessage(object? sender, ConsoleLibrary.BaseEventArgs e)
        {
            ConsoleLibrary.MessageEvent conMessageEvent = (ConsoleLibrary.MessageEvent)e;
            NetworkingLibrary.MessageEvent netMessageEvent = new NetworkingLibrary.MessageEvent(conMessageEvent.message, NetworkManager.Instance._clientConnection._user);
            NetworkManager.Instance.AddDataToSend(new SendDataEvent(netMessageEvent.Serialize(), netMessageEvent.User));
        }
        #endregion
    }
}
