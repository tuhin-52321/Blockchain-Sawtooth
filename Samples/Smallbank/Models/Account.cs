using System.ComponentModel.DataAnnotations;

namespace Smallbank.Models
{
    public class Account
    {
        public uint CustomerId { get; set; }
        
        [Display(Name = "Customer Name", Prompt = "Enter name of the customer", Description = "Name of the customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Savings Balance", Prompt = "Enter initial savings balance", Description = "Initial savings balance")]
        public uint SavingsBalance{ get; set; }

        [Display(Name = "Checking Balance", Prompt = "Enter initial checking balance", Description = "Initial checking balance")]
        public uint CheckingBalance { get; set; }
    }
}
