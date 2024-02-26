namespace ConsoleLibrary
{
    public class MenuFrame : FrameBase
    {
        private bool isFocused = false;
        private int currOption = 0;
        private MenuOption[] Options;
        private bool RowMode;
        private bool Wraps;

        private ConsoleColor hoveredTextColor = ConsoleColor.Black;
        private ConsoleColor hoveredBackColor = ConsoleColor.White;

        private const int PadLength = 3;
        private ConsoleString rowPadding => ConsoleString.Pad(PadLength);
        private ConsoleString leftSelector => new("<", hoveredTextColor, hoveredBackColor);
        private ConsoleString rightSelector => new(">", hoveredTextColor, hoveredBackColor);
        public MenuFrame(MenuOption[] options, bool rowMode, bool wraps, int x, int y, int width, int height = 1, bool border = false) : base(x, y, width, height, border)
        {
            Options = options;
            RowMode = rowMode;
            Wraps = wraps;
        }
        public bool Aligns(bool horizontal) => RowMode == horizontal;
        public void SetFocus() { isFocused = true; }
        public void SetSelection(int selection)
        {
            currOption = Math.Min(selection, Options.Length - 1);
        }
        public void MoveSelection(int moves)
        {
            currOption += moves;
            if (currOption < 0) currOption = Options.Length - 1;
            else currOption %= Options.Length;
        }
        public void Select() => Options[currOption].Select(this);
        private void DrawBuffer(ref int tail, FrameMask? mask, List<ConsoleString> buffer) {
            int localTail = tail;
            buffer.ForEach(str => {
                str.Render((X + localTail, Y), mask);
                localTail += str.Length;
            });
            buffer.Clear();
            tail = localTail;
        }
        public override void Render(FrameMask? mask)
        {
            if(RowMode)
            {
                //go to center
                int head = (Width - Options[currOption].Length - (PadLength * 2)) / 2;
                int tail;
                int optionTail;
                int optionHead = optionTail = currOption;
                List<ConsoleString> buffer = new List<ConsoleString>();

                //until at left side or first option
                while (head > 0 && optionHead > 0)
                    buffer.AddRange(Options[--optionHead].AttemptBack(ref head, rowPadding));

                buffer.Reverse();
                tail = head;

                //render buffer
                if(buffer.Count > 0) DrawBuffer(ref tail, mask, buffer);

                //try draw hovered
                if (isFocused) DrawBuffer(ref tail, mask, Options[currOption].AttemptForward(tail, Width, rightSelector, leftSelector, 1, hoveredTextColor, hoveredBackColor));
                else DrawBuffer(ref tail, mask, Options[currOption].AttemptForward(tail, Width, rowPadding, rowPadding));
                
                //until at width or last option
                while(optionTail < Options.Length - 1)
                {
                    DrawBuffer(ref tail, mask, Options[++optionTail].AttemptForward(tail, Width, rowPadding));
                    
                    if (tail >= Width) break;
                }

                if (Wraps)
                {
                    int tempTail = tail;
                    //until no options, draw back from head to 0
                    if (head > 0)
                    {
                        int fillOption = Options.Length - 1;
                        while (head > 0 && fillOption > optionTail)
                            buffer.AddRange(Options[fillOption--].AttemptBack(ref head, rowPadding));

                        if (head > 0 && fillOption == optionTail && Options[fillOption].LastDepth > 0)
                            buffer.Add(Options[fillOption].GetRemaining());

                        buffer.Reverse();
                        if (head > 0) buffer.Add(ConsoleString.Pad(head));
                        tempTail = tail;
                        head = tail = 0;

                        if (buffer.Count > 0) DrawBuffer(ref tail, mask, buffer);
                        buffer.Clear();
                        tail = tempTail;
                    }

                    optionTail = ++optionTail % Options.Length;
                    //until tail is at width or no options, draw from start
                    while (tempTail < Width && optionTail < optionHead)
                        buffer.AddRange(Options[optionTail++].AttemptForward(ref tempTail, Width, rowPadding));

                    if (tempTail < Width) buffer.Insert(0, ConsoleString.Pad(Width - tempTail));

                    DrawBuffer(ref tail, mask, buffer);
                }
            }
            else // multiple rows one column
            {
                //should be easier than above, each row has 1 option, just go to start of row and attempt forward
            }
            isFocused = false;
        }

        public override MenuFrame DeepCopy()
        {
            return new(Options, RowMode, Wraps, X, Y, Width, Height, Border);
        }

        public void Clear()
        {
            int tail = 0;
            DrawBuffer(ref tail, null, new([ConsoleString.Pad(Width)]));
        }
    }
}
