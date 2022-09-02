
namespace IntegerKeys.ViewModel
{

    public class CommittedKey
    {

        public string Name{get;private set;}
        public uint Value { get; set; }

        public CommittedKey IntKey => this;

        public CommittedKey(string name, uint value)
        {
            Name = name;
            Value = value;
        }
    }
}
