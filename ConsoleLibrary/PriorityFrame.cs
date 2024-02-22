namespace ConsoleLibrary
{
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
