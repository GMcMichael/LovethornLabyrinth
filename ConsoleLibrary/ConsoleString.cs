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
        public void Render(FrameMask? mask)
        {
            Console.ForegroundColor = TextColor;
            Console.BackgroundColor = BackColor;
            if(mask == null)
                Console.Write(Data);
            else
                for(int i = 0; i < Data.Length; i++)
                    if (!mask[(i,0)])
                        Console.Write(Data[i]);
                    else
                        Console.CursorLeft += 1;
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

    public class LineBreak : ConsoleString
    {
        public new string Data { get { return Break(); } }
        public LineBreak() : base("") { }
        public static string Break() { return "".PadRight(ConsoleManager.Instance._displaySize.Item1 - Console.GetCursorPosition().Left); }
        public override string ToString() { return Data; }
    }
}
