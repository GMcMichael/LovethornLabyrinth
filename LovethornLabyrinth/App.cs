using NetworkingLibrary;
using ConsoleLibrary;
using EventSystem;
using EventSystem.Events;

namespace LovethornLabyrinth
{
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
            CommandEvent commandEvent = (CommandEvent)e;
            NetworkEvents.Instance.CommandRecieved(new CommandEvent(commandEvent.Command, commandEvent.Args, commandEvent.Username));
        }

        private void OnSendMessage(object? sender, BaseEventArgs e)
        {
            MessageEvent conMessageEvent = (MessageEvent)e;
            MessageEvent netMessageEvent = new MessageEvent(conMessageEvent.Message, NetworkManager.Instance._clientConnection._user.Username);
            NetworkManager.Instance.AddDataToSend(new SendDataEvent(((ISerializable) netMessageEvent).Serialize(), netMessageEvent.Username));
        }
        #endregion
    }
}
