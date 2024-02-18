namespace EventSystem
{
    public enum EventType
    {
        Error,
        Message,
        Command,
        SendData,
        ReceiveData,
        HostStart,
        HostEnd,
        ClientJoin,
        ClientLeave
    }
    public enum CommandType
    {
        Host,
        Connect,
        Leave,
        User,
        Quit,
        Test
    }
}
