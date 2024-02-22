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
                if (index >= 0 && index < Mask.Count)
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
}
