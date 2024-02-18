using EventSystem.Events.NetworkEvents;

namespace EventSystem
{
    public abstract class NetworkEventSystem : BaseEventSystem
    {
        #region Event Handlers
        public BaseEventHandler OnDataReceived = new();
        public BaseEventHandler OnDataSend = new();

        public BaseEventHandler OnClientJoin = new();
        public BaseEventHandler OnClientLeave = new();

        public BaseEventHandler OnServerStart = new();
        public BaseEventHandler OnServerEnd = new();
        #endregion


        #region Event Raising
        public void DataReceived(ReceiveDataEvent dataEvent) { OnDataReceived.RaiseEvent(dataEvent); }
        public void SendData(SendDataEvent dataEvent) { OnDataSend.RaiseEvent(dataEvent); }

        public void ClientJoined(ClientJoinEvent clientJoinEvent) { OnClientJoin.RaiseEvent(clientJoinEvent); }
        public void ClientLeft(ClientLeaveEvent clientLeaveEvent) { OnClientLeave.RaiseEvent(clientLeaveEvent); }

        public void ServerStarted(ServerStartEvent hostStart) { OnServerStart.RaiseEvent(hostStart); }
        public void ServerEnded(ServerEndEvent hostEnd) { OnServerEnd.RaiseEvent(hostEnd); }
        #endregion
    }
}
