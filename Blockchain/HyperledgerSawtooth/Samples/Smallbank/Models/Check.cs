using System.ComponentModel.DataAnnotations;

namespace Smallbank.Models
{
    public class Check
    {
        public uint? CustomerId { get; set; }

        [Display(Name = "Customer Name", Description = "Name of the customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Checking Balance", Description = "Initial checking balance")]
        public uint CheckingBalance { get; set; }

        [Display(Name = "Check Amount", Prompt = "Enter check amount", Description = "Check Amount")]
        public uint Amount{ get; set; }

    }
}
