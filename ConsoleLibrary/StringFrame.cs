﻿namespace ConsoleLibrary
{
    public class StringFrame : FrameBase
    {
        public ConsoleRow[] Rows { get { return _rows; } }
        protected ConsoleRow[] _rows;
        protected int head = 0;

        public StringFrame(int x, int y, int width, int height, bool border = false) : base(x, y, width, height, border)
        {
            _rows = new ConsoleRow[Height];
            for (int i = 0; i < Height; i++)
                _rows[i] = new ConsoleRow(Width);
        }

        public ConsoleRow this[int index]
        {
            get { return Rows[index]; }
            set { head = index; _rows[index] = value; }
        }

        public void Push(ConsoleRow row)
        {
            _rows[head] = row;
            head++;
            head = head % _rows.Length;
        }
        public void PushCenter(ConsoleString consoleString) { Push(ConsoleRow.Center(Width, consoleString)); }
        public void PushCenter(ConsoleString[] consoleStrings) { Push(ConsoleRow.Center(Width, consoleStrings)); }
        public void PushEmpty(int times = 1) {for(int i = 0; i < times; i++) Push(new(Width)); }
        public override void Render(FrameMask? mask)
        {
            if (Border) DrawBorder(mask);

            for (int y = 0; y < Height; y++)
                if (Y + y >= 0 && Y + y < ConsoleManager.Instance._displaySize.Item2)
                    _rows[y].Render((X, Y + y), mask);
        }

        public override StringFrame DeepCopy()
        {
            StringFrame newFrame = new(X, Y, Width, Height, Border);

            for (int y = 0; y < Height; y++)
                if (_rows[y].Length > 0) { newFrame.head = y; newFrame.Push(_rows[y].DeepCopy()); }

            return newFrame;
        }
    }
}
