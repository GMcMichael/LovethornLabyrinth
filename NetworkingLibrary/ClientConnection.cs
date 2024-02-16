using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace NetworkingLibrary
{
    public class ClientConnection
    {
        public User _user = new();
        public CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public byte[] _buffer = new byte[NetworkManager._bufferByteLength];

        public bool _listening;

        public ClientConnection(User? user = null, Socket? socket = null)
        {
            _user = user ?? new User();
            _socket = socket ?? _socket;
        }

        public void ListenForData()
        {
            if (_listening) return;
            _listening = true;
            _tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _tokenSource.Token;

            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? data = null;
                    while (true)
                    {
                        int bytesReceived = await _socket.ReceiveAsync(_buffer, SocketFlags.None, cancellationToken);
                        if(cancellationToken.IsCancellationRequested)
                        {
                            _listening = false;
                            NetworkManager.Instance.Log($"Stopped listening for {_user.Username}");
                            return;
                        }
                        //Recieve data
                        data += Encoding.UTF8.GetString(_buffer, 0, bytesReceived);
                        int endIndex = data.IndexOf(NetworkManager._endOfData);
                        if (endIndex > -1)
                        {
                            data = data.Remove(endIndex);
                            break;
                        }
                    }

                    if (data == null) continue;
                    try
                    {
                        SendDataEvent? sendDataEvent = JsonSerializer.Deserialize<SendDataEvent>(data);
                        if (sendDataEvent != null)
                            NetworkEvents.Instance.DataReceived(new ReceiveDataEvent(sendDataEvent));
                    } catch (Exception e)
                    {
                        NetworkManager.Instance.Log($"Error Listening for data: {e}");
                    }
                }
                NetworkManager.Instance.Log($"Stopped listening for {_user.Username}");
                _listening = false;
            }, cancellationToken);
        }

        public void WaitForNewConnections()
        {
            if (_listening) return;
            _listening = true;
            _tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _tokenSource.Token;

            Task.Run(async () =>
            {
                NetworkManager.Instance.Log($"Waiting for connections at {_socket.LocalEndPoint}");
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var handler = await _socket.AcceptAsync(cancellationToken);
                    if(cancellationToken.IsCancellationRequested) break;

                    //get username
                    string? newUsername = null;
                    Task.WaitAny(new Task[] { Task.Run(async () =>
                    {
                        int bytesReceived = await handler.ReceiveAsync(_buffer, SocketFlags.None);
                        //Recieve data
                        newUsername = Encoding.UTF8.GetString(_buffer, 0, bytesReceived);
                    }, cancellationToken) }, NetworkManager._timeoutLength);
                    if (cancellationToken.IsCancellationRequested) break;

                    //check data
                    if (string.IsNullOrEmpty(newUsername))
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        handler = null;
                    }
                    else
                    {
                        ClientConnection connection = new ClientConnection(new User(newUsername), handler);
                        NetworkManager.Instance.AddHostClientConnection(connection);
                        NetworkManager.Instance.Log($"New connection from {connection._user}");
                        connection.ListenForData();//Listen for messages from client
                    }
                }
                NetworkManager.Instance.Log($"No longer waiting for connections at {_socket.LocalEndPoint}");
                _listening = false;
            }, cancellationToken);
        }

        public void Reset()
        {
            CloseConnection();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void CloseConnection()
        {
            try
            {
                if (_listening)
                    _tokenSource.Cancel();
                if(_socket.Connected)
                    _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            } catch (Exception e)
            {
                NetworkManager.Instance.Log("Error closing connection");
            }
        }
    }
}
