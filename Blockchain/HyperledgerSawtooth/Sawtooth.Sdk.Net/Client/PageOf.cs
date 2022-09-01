namespace Sawtooth.Sdk.Net.Client
{
    public class PageOf<T>
    {
        public List<T> List { get; } = new List<T>();

        public string HeadId { get; private set; }

        public string Next { get; private set; }

        public PageOf(string headId, string next)
        {
            HeadId = headId;
            Next = next;
        }

        internal void Add(T block)
        {
            List.Add(block);
        }
    }
}