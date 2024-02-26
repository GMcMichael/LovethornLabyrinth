
namespace ConsoleLibrary
{
    public class MenuString : MenuOption
    {
        public enum Mode
        {
            AlphaNumeric,
            Letters,
            Numbers
        }
        private Mode _mode;
        private int _minWidth;
        private string _label;
        private string _text;
        public bool IsWriting { get; private set; } = false;

        public MenuString(string label, string text, Action<string, MenuString> action) : this(label, text, 10, Mode.AlphaNumeric, action) { }
        public MenuString(string label, string text, int minWidth, Action<string, MenuString> action) : this(label, text, minWidth, Mode.AlphaNumeric, action) { }
        public MenuString(string label, string text, Mode mode, Action<string, MenuString> action) : this(label, text, 10, mode, action) { }
        public MenuString(string label, string text, int minWidth, Mode mode, Action<string, MenuString> action) : base(new((label + ": " + text).PadRight(minWidth)), frame => { })
        {
            _label = label;
            _text = text;
            _minWidth = minWidth;
            _mode = mode;
            ConsoleEvents.Instance.OnAlphaNumericKeyPressed += OnAlphaNumericPressed;
            ConsoleEvents.Instance.OnControlKeyPressed += OnControlKeyPressed; ;
            ConsoleEvents.Instance.OnMenuTick += OnMenuTick;
            _action = frame =>
            {
                IsWriting = !IsWriting;
                if (IsWriting) ConsoleManager.Instance.Reserve(this);
                else { ConsoleManager.Instance.Free(this); action(_text, this); }
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

            switch(_mode)
            {
                case Mode.Letters:
                    if (!char.IsAsciiLetter(e.KeyChar)) return;
                    break;
                 case Mode.Numbers:
                    if (!char.IsAsciiDigit(e.KeyChar)) return;
                    break;
                default:
                    break;
            }

            _text += e.KeyChar;
        }

        private void OnMenuTick(object? sender, EventArgs e)
        {
            if (!IsWriting) return;

            Text.Data = (_label + ": " + _text).PadRight(_minWidth);
        }

        public void ClearText() { _text = ""; }
    }
}
