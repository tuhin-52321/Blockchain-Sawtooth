using System.ComponentModel.DataAnnotations;

namespace Smallbank.Models
{
    public class VMTransaction
    {
        public uint CustomerId { get; set; }

        [Display(Name = "Customer Name", Description = "Name of the customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Checking Balance", Description = "Checking balance")]
        public uint CheckingBalance { get; set; }

        [Display(Name = "Savings Balance", Description = "Savings balance")]
        public uint SavingsBalance { get; set; }

        [Display(Name = "Check Amount", Prompt = "Enter check amount", Description = "Check amount")]
        public uint CheckAmount{ get; set; }

        [Display(Name = "Cash Amount", Prompt = "Enter cash amount", Description = "Cash amount")]
        public int CashAmount { get; set; }

        [Display(Name = "Destination customer Id", Prompt = "Enter Destination customer id", Description = "Destination Customer")]
        public uint DestCustomerId { get; set; }

    }
}
