using EventSystem.Events;

namespace EventSystem
{
    public class BaseEventHandler
    {
        public event EventHandler<BaseEventArgs>? OnEvent;

        public void RaiseEvent(BaseEventArgs e) { OnEvent?.Invoke(this, e); }
    }
}
