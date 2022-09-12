namespace Sawtooth.Sdk.Net.Client
{
    public class FullList<T>
    {
        public List<T> List { get; } = new List<T>();

        public string HeadId { get; private set; }


        public FullList(string headId)
        {
            HeadId = headId;
        }

        internal void Add(T block)
        {
            List.Add(block);
        }
    }
}