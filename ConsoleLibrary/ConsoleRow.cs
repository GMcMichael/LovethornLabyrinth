namespace ConsoleLibrary
{
    public class ConsoleRow
    {
        public int Width { get; set; }
        public List<ConsoleString> Row { get { return _row; } }
        private List<ConsoleString> _row = new();
        public int Length {  get { int len = 0; _row.ForEach(str => len += str.Length); return len; } }
        public ConsoleRow(int width) { Width = width; }
        public ConsoleRow(int width, List<ConsoleString> row) { Width = width; _row = new(row); }
        public ConsoleRow(List<ConsoleString> row) { int len = 0; row.ForEach(str => { len += str.Length; }); Width = len; _row = new(row); }
        public ConsoleRow(ConsoleString str) { Width = str.Length; _row.Add(str); }
        public ConsoleRow DeepCopy()
        {
            ConsoleRow newRow = new(Width);

            _row.ForEach(str => newRow.Push(str.DeepCopy()));

            return newRow;
        }
        public static ConsoleRow Center(int width, ConsoleString msg)
        {
            ConsoleRow row = new(width);
            int diff = width - msg.Length;
            int mid = diff / 2;
            row.Push(ConsoleString.Pad(mid));
            row.Push(msg);
            row.Push(ConsoleString.Pad(mid + (diff % 2)));

            return row;
        }
        public static ConsoleRow Center(int width, ConsoleString[] msg)
        {
            ConsoleRow row = new(width);
            int diff = width;

            foreach (ConsoleString str in msg)
                diff -= str.Length;

            int mid = diff / 2;
            row.Push(ConsoleString.Pad(mid));
            foreach (ConsoleString str in msg)
                row.Push(str);
            row.Push(ConsoleString.Pad(mid + (diff % 2)));

            return row;
        }
        public static ConsoleRow Fill(int width, char toFill, ConsoleColor TextColor = ConsoleColor.White, ConsoleColor BackColor = ConsoleColor.Black)
        {
            string str = "";
            for (int i = 0; i < width; i++) str += toFill;
            return new(new ConsoleString(str, TextColor, BackColor));
        }
        public void Render(FrameMask? mask)
        {
            if (_row.Count <= 0)
                ConsoleString.Pad(Width).Render(mask?[(0,0), Width]);
            else
            {
                int head = 0;
                _row.ForEach(str =>
                {
                    str.Render(mask?[(head, 0), str.Length]);
                    head += str.Length;
                });
            }
        }

        private void Clamp()
        {
            List<ConsoleString> temp = new();
            int remaining = Width;
            foreach (ConsoleString str in _row)
            {
                if (remaining <= 0)
                    break;

                if (str.Length > remaining)
                    str.Clamp(remaining);

                temp.Add(str);
                remaining -= str.Length;
            }
            _row = new(temp);
        }

        public void Push(ConsoleString data) { if (_row.Count <= 0 || !_row[^1].Merge(data)) _row.Add(data); Clamp(); }
        public void PushRange(ConsoleString[] consoleStrings) { foreach (var cs in consoleStrings) Push(cs); }
    }
}
