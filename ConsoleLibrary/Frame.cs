using System.Collections;

namespace ConsoleLibrary
{
    public class FrameMask
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public BitArray Mask { get; set; }
        public int Length { get { return Mask.Count; } }
        public FrameMask(int width, int height) { Mask = new(width * height); Width = width; Height = height; }
        public FrameMask(BitArray mask) { Mask = mask; Width = mask.Count; Height = 1; }
        public bool Peek(int x = 0, int y = 0) { return Mask[x + y * Width]; }
        public bool Peek((int, int) pos) { return Peek(pos.Item1, pos.Item2); }
        public void Set(int index, bool value) { Mask[index] = value; }
        public void Set(int x, int y, bool value) { Mask[x + y * Width] = value; }
        public void SetRange(int index, bool[] values) { for (int i = 0; i < values.Length; i++) Set(index + i, values[i]); }
        public bool this[(int, int) pos]
        {
            get { return this[pos.Item1, pos.Item2]; }
            set { this[pos.Item1, pos.Item2] = value; }
        }
        public bool this[int x, int y]
        {
            get
            {
                int index = x + (y * Width);
                bool b = index >= 0 && index < Mask.Count && Mask[index];
                if (index >= 0 && index < Mask.Count)
                    Mask[index] = true;
                return b;
            }
            set
            {
                int index = x + (y * Width);
                if(index >= 0 && index < Mask.Count)
                    Mask[index] = value;
            }
        }

        public FrameMask this[(int, int) origin, int width]
        {
            get { return SubArray(origin, width, 1); }
        }
        public FrameMask this[(int, int) origin, (int, int) dims]
        {
            get { return SubArray(origin, dims.Item1, dims.Item2); }
        }
        public bool HasAllSet() { return Mask.HasAllSet(); }
        public void Clear() { Mask.SetAll(false); }
        public FrameMask SubArray((int, int) origin, int width, int height)
        {
            FrameMask subMask = new(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    subMask.Set(x, y, this[x + origin.Item1, y + origin.Item2]);//Peek(x, y));
            return subMask;
        }
    }
    public abstract class FrameBase
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BorderX { get { return X - (Border ? 1 : 0); } }
        public int BorderY { get { return Y - (Border ? 1 : 0); } }
        public int BorderWidth { get { return Width + (Border ? 2 : 0); } }
        public int BorderHeight { get { return Height + (Border ? 2: 0); } }
        public bool Border {  get; set; }
        public ConsoleColor BorderTextColor {  get; set; } = ConsoleColor.White;
        public ConsoleColor BorderBackColor {  get; set; } = ConsoleColor.Black;
        public FrameBase(int x, int y, int width, int height, bool border)
        {
            Width = width;
            Height = height;
            Border = border;
            Move(x, y);
        }
        public abstract void Render(FrameMask? mask);
        public abstract FrameBase DeepCopy();
        public void Move(int? _x = null, int? _y = null)
        {
            X = _x ?? X;
            Y = _y ?? Y;
        }
        public FrameMask EmptyMask { get { return new(Width, Height); } }
        public void DrawBorder(FrameMask? mask = null)
        {
            Console.ForegroundColor = BorderTextColor;
            Console.BackgroundColor = BorderBackColor;

            string top = " ", bot = "\\";
            for (int i = 0; i < Width; i++) { top += "_"; bot += "_"; }
            top += ' '; bot += '/';

            //draw top
            AttemptRender((BorderX, BorderY), top, mask);

            //draw top connector
            AttemptRender((BorderX, Y), '/', mask);
            AttemptRender((Width + X, Y), '\\', mask);

            //draw middle
            for (int y = 1; y < Height; y++)
            {
                AttemptRender((BorderX, Y + y), '|', mask);
                AttemptRender((Width + X, Y + y), '|', mask);
            }

            //draw bottom with connector
            AttemptRender((BorderX, Y + Height), bot, mask);
        }
        protected bool CheckBounds((int, int) pos)
        {
            (int, int) displaySize = ConsoleManager.Instance._displaySize;
            return pos.Item1 >= 0 && pos.Item1 < displaySize.Item1 &&
                   pos.Item2 >= 0 && pos.Item2 < displaySize.Item2;
        }
        private bool CheckRender((int, int) pos, FrameMask? mask = null)
        {
            bool b = CheckBounds(pos);
            if (b &&
                mask != null &&
                pos.Item1 < mask.Width &&
                pos.Item2 < mask.Height)
            {
                b = !mask[pos];
            }
            return b;
        }
        public void AttemptRender((int, int) origin, char msg, FrameMask? mask = null)
        {
            if (CheckRender(origin, mask))
            {
                Console.SetCursorPosition(origin.Item1, origin.Item2);
                Console.Write(msg);
            }
        }
        public void AttemptRender((int, int) origin, string row, FrameMask? mask = null)
        {
            if (origin.Item1 < 0 || origin.Item1 >= ConsoleManager.Instance._displaySize.Item1 ||
                origin.Item2 < 0 || origin.Item2 >= ConsoleManager.Instance._displaySize.Item2) return;
            for (int x = 0; x < row.Length; x++)
            {
                if(CheckRender((origin.Item1 + x, origin.Item2), mask))
                {
                    Console.SetCursorPosition(origin.Item1 + x, origin.Item2);
                    Console.Write(row[x]);
                }
            }
        }
    }
    public class StringFrame : FrameBase
    {
        public ConsoleRow[] Rows { get { return _rows; } }
        private ConsoleRow[] _rows;
        private int head = 0;

        public StringFrame(int x, int y, int width, int height, bool border = false) : base(x, y, width, height, border)
        {
            _rows = new ConsoleRow[Height];
            for (int i = 0; i < Height; i++)
                _rows[i] = new ConsoleRow(Width);
        }

        public ConsoleRow this[int index]
        {
            get { return Rows[index]; }
            set { head = index;  _rows[index] = value; }
        }

        public void Push(ConsoleRow row)
        {
            _rows[head] = row;
            head++;
            head = head % _rows.Length;
        }
        public void PushCenter(ConsoleString consoleString) { Push(ConsoleRow.Center(Width, consoleString)); }
        public void PushCenter(ConsoleString[] consoleStrings) { Push(ConsoleRow.Center(Width, consoleStrings)); }
        public void PushEmpty() { Push(new(Width)); }
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

    public class PriorityFrame : FrameBase
    {
        public PriorityQueue<FrameBase, int> PriorityQueue { get; set; } = new();

        public PriorityFrame(int x, int y, int width, int height, bool border = false) : base(x, y, width, height, border) { }

        public void Push(FrameBase frame, int priority = 0) { PriorityQueue.Enqueue(frame, priority); }

        public override void Render(FrameMask? mask)
        {
            if (Border) DrawBorder(mask);

            PriorityQueue<FrameBase, int> temp = new PriorityQueue<FrameBase, int>(PriorityQueue.UnorderedItems);
            while(temp.Count > 0 && (mask is null ? false : !mask.HasAllSet()))
                temp.Dequeue().Render(mask);
        }

        public override PriorityFrame DeepCopy()
        {
            PriorityFrame newFrame = new(X, Y, Width, Height, Border);

            foreach (var frame in PriorityQueue.UnorderedItems)
                newFrame.Push(frame.Element.DeepCopy(), frame.Priority);

            return newFrame;
        }
    }
}
