namespace ConsoleLibrary
{
    public abstract class FrameBase
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BorderX { get { return X - (Border ? 1 : 0); } }
        public int BorderY { get { return Y - (Border ? 1 : 0); } }
        public int BorderWidth { get { return Width + (Border ? 2 : 0); } }
        public int BorderHeight { get { return Height + (Border ? 2 : 0); } }
        public bool Border { get; set; }
        public ConsoleColor BorderTextColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BorderBackColor { get; set; } = ConsoleColor.Black;
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
            if (origin.Item1 >= ConsoleManager.Instance._displaySize.Item1 ||
                origin.Item2 < 0 || origin.Item2 >= ConsoleManager.Instance._displaySize.Item2) return;
            for (int x = 0; x < row.Length; x++)
            {
                if (CheckRender((origin.Item1 + x, origin.Item2), mask))
                {
                    Console.SetCursorPosition(origin.Item1 + x, origin.Item2);
                    Console.Write(row[x]);
                }
            }
        }
    }
}
