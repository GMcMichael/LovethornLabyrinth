
namespace ConsoleLibrary
{
    public class MenuString : MenuOption
    {
        private int _width;
        private string _text;
        public bool IsWriting { get; private set; } = false;
        public MenuString(string prompt, int width) : base(new(prompt.PadRight(width).Substring(0, width)), () => { })
        {
            _width = width;
            _text = prompt.PadRight(width).Substring(0, width);
            ConsoleEvents.Instance.OnKeyPressed += OnKeyPressed;
            ConsoleEvents.Instance.OnMenuTick += OnMenuTick;
            _action = () =>
            {
                IsWriting = !IsWriting;
                if (IsWriting) _text = "";
            };
        }

        private void OnKeyPressed(object? sender, ConsoleKey e)//TODO: I need to get just character keys
        {
            if (!IsWriting) return;

            if (e == ConsoleKey.Escape)
            {
                IsWriting = false;
                return;
            }
            if(e != ConsoleKey.Enter) _text += e.ToString();// I need to pass keyinfo instead so I can check if shift was held
        }

        private void OnMenuTick(object? sender, EventArgs e)
        {
            if (!IsWriting) return;

            Text.Data = _text.PadRight(_width).Substring(0, _width);
        }
    }
}
