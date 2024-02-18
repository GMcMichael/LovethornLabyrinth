using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;
using EventSystem;
using EventSystem.Events;

namespace NetworkingLibrary
{
    //TODO: should I set up pooling for host/server https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socketasynceventargs.setbuffer?view=net-8.0&redirectedfrom=MSDN#System_Net_Sockets_SocketAsyncEventArgs_SetBuffer_System_Byte___System_Int32_System_Int32_
    public class NetworkManager
    {
        public static NetworkManager Instance { get; private set; } = new();

        public NetworkManager() { }

        #region Const Variables
        public const string _localHost = "127.0.0.1";
        public const int _defaultPort = 48888;
        public const int _bufferByteLength = 1024;
        public const int _connectionBacklog = 5;
        public const int _timeoutLength = 10000;
        public const string _logFileName = "NetworkLog.txt";
        public const string _userFileName = "Users.json";
        public const string _lastConnectionFile = "LastConnection.json";
        public const string _endOfData = "|EOD|";
        #endregion

        #region Variables
        private string? APP_PATH;
        private bool _hosting = false;
        private bool _connected = false;
        private bool _sending = false;
        private bool _isBusy = false;
        public bool IsHosting { get { return _hosting; } }
        public bool IsConnected { get { return _connected; } }
        public bool IsBusy { get { return _isBusy; } }

        public List<SendDataEvent> dataToSend = new List<SendDataEvent>();
        public ClientConnection _clientConnection = new();
        private List<ClientConnection> _hostClientConnections = new List<ClientConnection>();

        //save last connection for quick rejoin
        private Connection _lastConnection = new();
        #endregion

        #region Event Callbacks
        public void InitEventCallbacks()
        {
            NetworkEvents.Instance.OnSendData.OnEvent += OnSendData;
            NetworkEvents.Instance.OnDataReceived.OnEvent += OnDataReceived;
            NetworkEvents.Instance.OnCommandRecieved.OnEvent += OnCommandRecieved;
            NetworkEvents.Instance.OnClientJoin.OnEvent += OnClientJoin;
        }

        private void OnSendData(object? sender, BaseEventArgs e)
        {
            try
            {
                SendDataEvent dataEvent = (SendDataEvent)e;
                if (_hosting) CheckClientList();
                Task.Run(async () =>
                {
                    string message = ((ISerializable)dataEvent).Serialize() + _endOfData;
                    //Log($"Sending data: {message}");
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    if (_connected)
                        await _clientConnection._socket.SendAsync(messageBytes);
                    else if (_hosting)
                        _hostClientConnections.ForEach(async conn => { await conn._socket.SendAsync(messageBytes); });
                });
            } catch (SocketException ex)
            {
                Log($"Error sending message: {ex}");
            }
        }

        private void OnDataReceived(object? sender, BaseEventArgs e)
        {
            ReceiveDataEvent dataEvent = (ReceiveDataEvent)e;
            switch (dataEvent.DataType)
            {
                case EventType.Message:
                    MessageEvent? messageEvent = null;
                    try { messageEvent = JsonSerializer.Deserialize<MessageEvent>(dataEvent.Data); }
                    catch (Exception ex) { Log($"{ex}"); }
                    if (messageEvent == null) break;
                    NetworkEvents.Instance.MessageReceived(messageEvent);
                    if (_hosting)
                        _hostClientConnections.ForEach(client =>
                        {
                            if (!client._user.Equals(messageEvent.Username))
                            {
                                SendDataEvent dataEvent = new SendDataEvent(((ISerializable) messageEvent).Serialize(), messageEvent.Username);
                                NetworkEvents.Instance.SendData(dataEvent);
                            }
                        });
                    break;
                default:
                    Log($"Data of type {dataEvent.DataType} Received: {dataEvent.Data}");
                    break;
            }
        }
        private void OnCommandRecieved(object? sender, BaseEventArgs e)
        {
            CommandEvent commandEvent = (CommandEvent)e;
            string[] args = commandEvent.Args;
            switch(commandEvent.Command)
            {
                case CommandType.Host:
                    HostServer(args.Length > 0 ? args[0] : null, args.Length > 1 ? int.Parse(args[1]) : null);
                    break;
                case CommandType.Connect:
                    ClientConnect(args.Length > 0 ? args[0] : null, args.Length > 1 ? int.Parse(args[1]) : null);
                    break;
                case CommandType.Leave:
                    if(_hosting)
                    {
                        _hosting = false;// check to see if all this is done in the event, should it be?
                        foreach (var conn in _hostClientConnections)
                        {
                            conn.CloseConnection();
                        }
                        _hostClientConnections.Clear();
                        NetworkEvents.Instance.ServerEnded(new ServerEndEvent(_clientConnection._socket.LocalEndPoint, _clientConnection._user)); ;
                    }
                    if (_connected)
                    {
                        _connected = false;// check to see if this is set after the event
                        NetworkEvents.Instance.ClientLeft(new ClientLeaveEvent(_clientConnection._socket.RemoteEndPoint, _clientConnection._user));
                    }
                    _clientConnection.Reset();
                    break;
                case CommandType.User:
                    if (args.Length == 0)
                        Log($"User: {_clientConnection._user.Username}");
                    else
                        ChangeUser(args[0]);
                    break;
                case CommandType.Quit:
                    Log("Closing Socket(s)...");
                    _clientConnection.CloseConnection();

                    foreach (var conn in _hostClientConnections)
                        conn.CloseConnection();
                    _hostClientConnections.Clear();
                    break;
                case CommandType.Test:
                    Log("Test Chosen");
                    break;
                default:
                    Log($"Unknown Command Type: {commandEvent.Command}");
                    break;
            }
        }

        private void OnClientJoin(object? sender, BaseEventArgs e)
        {
            ClientJoinEvent clientJoinEvent = (ClientJoinEvent)e;
            SaveMostRecentConnection(new Connection(clientJoinEvent.Host, clientJoinEvent.Port));
        }
        #endregion

        #region Helper Functions
        public void Init(string? AppPath = null)
        {
            APP_PATH = AppPath;

            TestSerialization(false);

            _lastConnection = ReadMostRecentConnection();
            User? u = GetMostRecentUser();
            if (u != null) _clientConnection._user = u;
            Log($"User - {u}");

            InitEventCallbacks();
        }
        public void Log(string message)
        {
            //SaveLog(message); //Console already does this
            NetworkEvents.Instance.PassLog("NetworkManager: " + message);
        }
        public void SaveLog(string message)
        {
            if (string.IsNullOrEmpty(APP_PATH)) return;
            //save to log file
            using (StreamWriter sw = new StreamWriter(Path.Combine(APP_PATH, _logFileName), true))
            {
                sw.WriteLine(message);
            }
        }
        public void AddDataToSend(SendDataEvent dataEvent) { if(_connected || _hosting) dataToSend.Add(dataEvent); }
        public void SendData()
        {
            if (_sending) return;
            _sending = true;
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!_connected && !_hosting) break;
            
                    await Task.Run(async () =>
                    {
                        while (dataToSend.Count <= 0) { await Task.Yield(); }
                    });
            
                    if (_clientConnection == null) continue;

                    //copy current messages
                    SendDataEvent[] dataEvents = dataToSend.ToArray();
                    dataToSend.Clear();
            
                    foreach (SendDataEvent dE in dataEvents)
                        NetworkEvents.Instance.SendData(dE);
                }
                _sending = false;
            });
        }
        public static string GetLocalIP()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("No IP address detected");
        }
        public void ChangeUser(string? newName = null)
        {
            if (_connected || _hosting || _isBusy || _clientConnection == null) return;
            if(newName == null)
            {
                Log($"Username: {_clientConnection._user.Username}");
                return;
            }
            _isBusy = true;

            _clientConnection._user.Username = newName;
            SaveUser();
            Log($"Username changed to \"{_clientConnection._user.Username}\"");

            _isBusy = false;
        }
        public void SaveUser()
        {
            if (APP_PATH == null) return;
            HashSet<User> savingUsers = _clientConnection == null ? new() : [_clientConnection._user];
            savingUsers.UnionWith(ReadUsers().ToHashSet());

            using (StreamWriter sw = new StreamWriter(Path.Combine(APP_PATH, _userFileName), false))
            {
                foreach (User user in savingUsers)
                    sw.WriteLine(((ISerializable) user).Serialize());
            }
        }
        public User[] ReadUsers()
        {
            if (APP_PATH == null) return new User[0];

            List<User> users = new List<User>();
            string path = Path.Combine(APP_PATH, _userFileName);
            if (Path.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string? line = sr.ReadLine();
                    while (line != null)
                    {
                        User? user = JsonSerializer.Deserialize<User>(line);
                        if(user != null) users.Add(user);
                        line = sr.ReadLine();
                    }
                }
            }
            return users.ToArray();
        }
        public User? GetMostRecentUser()
        {
            User[] users = ReadUsers();
            return users.Length > 0 ? users[0] : null;
        }
        public void SaveMostRecentConnection(Connection connection)
        {
            if (APP_PATH == null) return;

            _lastConnection = connection;
            using (StreamWriter sw = new StreamWriter(Path.Combine(APP_PATH, _lastConnectionFile), false))
            {
                sw.WriteLine(((ISerializable)_lastConnection).Serialize());
            }
        }
        public Connection ReadMostRecentConnection()
        {
            if (APP_PATH == null) return new();

            Connection mostRecentConnection = new();
            string path = Path.Combine(APP_PATH, _lastConnectionFile);
            if (Path.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string? line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        mostRecentConnection = JsonSerializer.Deserialize<Connection>(line);
                }
            }
            if(mostRecentConnection != null) Log($"Last Connection - {mostRecentConnection}");
            return mostRecentConnection ?? new();

        }
        private void RunTest(ref bool result, ISerializable serializable) { result = result && serializable.Test(); }
        public bool TestSerialization(bool log, bool result = true)
        {
            try
            {
                bool allPassed = true;
                string testUsername = "Test_User";
                string testData = "Test Data";
                foreach (ISerializable testObj in new ISerializable[]
                {
                    new User(testUsername),
                    new Connection(),
                    new CommandEvent(CommandType.Test, Array.Empty<string>(), testUsername),
                    new MessageEvent(testData, testUsername),
                    new SendDataEvent(testData, testUsername),
                    new ReceiveDataEvent(new SendDataEvent((new MessageEvent(testData, testUsername) as ISerializable).Serialize(), testUsername)),
                    new ServerStartEvent(GetLocalIP(), _defaultPort, testUsername),
                    new ServerEndEvent(_localHost, _defaultPort, testUsername),
                    new ClientJoinEvent(_localHost, _defaultPort, testUsername),
                    new ClientLeaveEvent(_localHost, _defaultPort, testUsername),
                })
                {
                    RunTest(ref allPassed, testObj);
                }

                if (result) Log($"All Event Serializations Passed: {allPassed}\n");
                return allPassed;
                
            } catch (Exception ex)
            {
                Log($"Serialization testing failed: {ex}");
                return false;
            }
        }
        public void AddHostClientConnection(ClientConnection connection) { _hostClientConnections.Add(connection); }
        public void CheckClientList()
        {
            if (!_hosting) { CloseHostConnections(); return; }
            new List<ClientConnection>(_hostClientConnections).ForEach(conn =>
            {
                if(!conn._listening) _hostClientConnections.Remove(conn);
            });
        }
        public void CloseHostConnections()
        {
            _hostClientConnections.ForEach(conn => conn.CloseConnection());
            _hostClientConnections.Clear();
        }
        #endregion

        #region Client Functions
        //Client entry point
        public async void ClientConnect(string? host = null, int? port = null)
        {
            if (_connected || _hosting || _isBusy) return;

            _isBusy = true;
            _clientConnection.Reset();

            host ??= GetLocalIP();
            port ??= _defaultPort;

            Log("Connecting to [" + host + "] at socket [" + port + "]"); ;

            //_lastHost = host;
            //_lastPort = port;

            IPEndPoint hostIP = new IPEndPoint(IPAddress.Parse(host), (int)port);
            await _clientConnection._socket.ConnectAsync(hostIP);
            _connected = true;

            // send username
            await _clientConnection._socket.SendAsync(Encoding.UTF8.GetBytes(_clientConnection._user.Username));

            _clientConnection.ListenForData();
            SendData();

            NetworkEvents.Instance.ClientJoined(new ClientJoinEvent(host, (int)port, _clientConnection._user.Username));

            _isBusy = false;
        }
        #endregion

        #region Host Functions
        //Host entry point
        public void HostServer(string? host = null, int? port = null)
        {
            if (_connected || _hosting || _isBusy) return;

            _isBusy = true;

            host ??= GetLocalIP();
            port ??= _defaultPort;

            Log("Hosting at [" + host + "] on socket [" + port + "]");

            IPEndPoint hostIP = new IPEndPoint(IPAddress.Parse(host), (int)port);

            _clientConnection._socket = new Socket(hostIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _clientConnection._socket.Bind(hostIP);
            _clientConnection._socket.Listen(_connectionBacklog);
            _hosting = true;

            _clientConnection.WaitForNewConnections();
            SendData();

            NetworkEvents.Instance.ServerStarted(new ServerStartEvent(host, (int)port, _clientConnection._user.Username));

            _isBusy = false;
        }
        #endregion
    }
}
