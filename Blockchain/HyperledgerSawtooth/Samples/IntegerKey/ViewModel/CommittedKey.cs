
namespace IntegerKeys.ViewModel
{

    public class CommittedKey
    {

        public string Name{get;private set;}
        public int Value { get; set; }

        public CommittedKey IntKey => this;

        public CommittedKey(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }
}
