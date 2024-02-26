namespace ConsoleLibrary
{
    public class PriorityFrame : FrameBase
    {
        public PriorityQueue<FrameBase, int> PriorityQueue { get; set; } = new();
        private Dictionary<FrameBase, int> keyValuePairs = new();
        private int head = 0;

        public PriorityFrame(int x, int y, int width, int height, bool border = false) : base(x, y, width, height, border) { }

        public void Push(FrameBase frame, int priority = 0) { PriorityQueue.Enqueue(frame, priority); head = Math.Min(head, priority); }
        public void PushTop(FrameBase frame) { PriorityQueue.Enqueue(frame, --head); }
        public void Pop(FrameBase frame) { PriorityQueue = new(PriorityQueue.UnorderedItems.Where(keyValue => { return !keyValue.Element.Equals(frame); })); }

        public override void Render(FrameMask? mask)
        {
            if (Border) DrawBorder(mask);

            PriorityQueue<FrameBase, int> temp = new PriorityQueue<FrameBase, int>(PriorityQueue.UnorderedItems);
            while(temp.Count > 0 && (mask is null ? true : !mask.HasAllSet()))
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
