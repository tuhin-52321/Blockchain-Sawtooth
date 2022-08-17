namespace Smallbank.Models
{
    public class Account
    {
        public uint ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint SavingsBalance{ get; set; }
        public uint CheckingBalance { get; set; }
    }
}
