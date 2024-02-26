namespace ConsoleLibrary
{
    public class MenuOption
    {
        public static MenuOption Back = new(new("Back"), (frame) => { ConsoleManager.Instance.PopMenu()?.Clear(); });
        public static Action<MenuFrame> EmptyAction { get; } = frame => {};
        public ConsoleString Text;
        protected Action<MenuFrame> _action;
        private int lastDepth = 0;
        public int LastDepth { get { return lastDepth; } }
        public int Length => Text.Length;
        public int Remaning => Length - lastDepth;
        public MenuOption(ConsoleString text, Action<MenuFrame>? action = null)
        {
            Text = text;
            _action = action ?? EmptyAction;
        }
        public void Select(MenuFrame frame) { _action(frame); }
        public ConsoleString GetRemaining() => Text.SubString(lastDepth, Remaning);

        private int BackAdd(ref int head, List<ConsoleString> list, ConsoleString consoleString) {
            if (head <= 0) return 0;
            int len = consoleString.Length;
            int dist = Math.Min(head, len);
            int startDepth = len - dist;
            head -= dist;

            list.Add(startDepth > 0 ? consoleString.SubString(startDepth, dist) : consoleString);
            return startDepth + dist;
        }
        private int FrontAdd(ref int tail, int width, List<ConsoleString> list, ConsoleString consoleString)
        {
            if (tail >= width) return 0;
            int len = consoleString.Length;

            int dist = Math.Min(width - tail, len);
            tail += dist;

            list.Add(dist < len ? consoleString.SubString(0, dist) : consoleString);
            return dist;
        }

        public List<ConsoleString> AttemptBack(ref int head, ConsoleString padding)
        {
            List<ConsoleString> results = new List<ConsoleString>();

            lastDepth = BackAdd(ref head, results, Text) % Length;

            BackAdd(ref head, results, padding);

            return results;
        }
        public List<ConsoleString> AttemptForward(int tail, int width, ConsoleString backPad, ConsoleString? frontPad = null, int additonalPad = 0, ConsoleColor? textColor = null, ConsoleColor? backColor = null)
        {
            return AttemptForward(ref tail, width, backPad, frontPad, additonalPad, textColor, backColor);
        }
        public List<ConsoleString> AttemptForward(ref int tail, int width, ConsoleString backPad, ConsoleString? frontPad = null, int additonalPad = 0, ConsoleColor? textColor = null, ConsoleColor? backColor = null)
        {
            List<ConsoleString> results = new List<ConsoleString>();
            if(additonalPad > 0)
                FrontAdd(ref tail, width, results, ConsoleString.Pad(additonalPad));
            if (frontPad != null)
                FrontAdd(ref tail, width, results, frontPad);
            if (additonalPad > 0)
                FrontAdd(ref tail, width, results, ConsoleString.Pad(additonalPad));

            ConsoleString text = Text;
            if(textColor != null || backColor != null) text = new(text.Data, textColor ?? text.TextColor, backColor ?? text.BackColor);
            lastDepth = FrontAdd(ref tail, width, results, text) % Length;

            if (additonalPad > 0)
                FrontAdd(ref tail, width, results, ConsoleString.Pad(additonalPad));
            FrontAdd(ref tail, width, results, backPad);
            if (additonalPad > 0)
                FrontAdd(ref tail, width, results, ConsoleString.Pad(additonalPad));

            return results;
        }
    }
}
