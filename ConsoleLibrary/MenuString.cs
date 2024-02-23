
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
            ConsoleEvents.Instance.OnAlphaNumericKeyPressed += OnAlphaNumericPressed;
            ConsoleEvents.Instance.OnControlKeyPressed += OnControlKeyPressed; ;
            ConsoleEvents.Instance.OnMenuTick += OnMenuTick;
            _action = () =>
            {
                IsWriting = !IsWriting;
                if (IsWriting) { _text = ""; ConsoleManager.Instance.Reserve(this); }
                else ConsoleManager.Instance.Free(this);
            };
        }

        private void OnControlKeyPressed(object? sender, ConsoleKeyInfo e)
        {
            if (!IsWriting || e.Key != ConsoleKey.Backspace) return;

            if (_text.Length > 0) _text = _text[..^1];
        }

        private void OnAlphaNumericPressed(object? sender, ConsoleKeyInfo e)
        {
            if (!IsWriting) return;

            _text += e.KeyChar;
        }

        private void OnMenuTick(object? sender, EventArgs e)
        {
            if (!IsWriting) return;

            Text.Data = _text.PadRight(_width).Substring(0, _width);
        }
    }
}
