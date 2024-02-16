using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleLibrary
{
    public class ConsoleString
    {
        public ConsoleColor TextColor { get; set; }
        public ConsoleColor BackColor { get; set; }
        public string Data { get; set; }
        [JsonIgnore]
        public int Length => Data.Length;
        private int NumColors = Enum.GetNames(typeof(ConsoleColor)).Length;
        public ConsoleString(string data, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black)
        {
            Data = data;
            TextColor = textColor;
            BackColor = backColor;
        }

        public ConsoleString DeepCopy() { return new(Data, TextColor, BackColor); }
        private bool CheckBounds((int, int) pos)
        {
            (int, int) displaySize = ConsoleManager.Instance._displaySize;
            return pos.Item1 >= 0 && pos.Item1 < displaySize.Item1 &&
                   pos.Item2 >= 0 && pos.Item2 < displaySize.Item2;
        }
        public void Render((int, int) origin, FrameMask? mask)
        {
            Console.ForegroundColor = TextColor;
            Console.BackgroundColor = BackColor;
            for (int x = 0; x < Data.Length; x++)
            {
                (int, int) pos = (origin.Item1 + x, origin.Item2);
                if (CheckBounds(pos) && (mask == null || !mask[pos]))
                {
                    Console.SetCursorPosition(pos.Item1, pos.Item2);
                    Console.Write(Data[x]);
                }
            }
        }
        public void CycleTextColor() { TextColor = (ConsoleColor)((int)(TextColor + 1) % NumColors); }
        public void CycleBackgroungColor() { BackColor = (ConsoleColor)((int)(BackColor + 1) % NumColors); }
        public string Serialize() { return JsonSerializer.Serialize(this); }
        public override string ToString() { return Data; }
        public bool Merge(ConsoleString other)
        {
            if (other.TextColor == TextColor &&
               other.BackColor == BackColor)
            {
                Data += other.Data;
                return true;
            }
            return false;
        }
        public void Clamp(int len) { Data = Data.Substring(0, len); }
        public static ConsoleString Pad(int len, string msg = "") { return new($"{msg}".PadLeft(len)); }
    }
}
