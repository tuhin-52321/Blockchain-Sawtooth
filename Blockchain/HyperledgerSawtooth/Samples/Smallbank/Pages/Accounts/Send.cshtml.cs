using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Smallbank.Data;
using Smallbank.Models;

namespace Smallbank.Pages.Accounts
{
    public class SendModel : PageModel
    {
        private readonly Smallbank.Data.SmallbankContext _context;

        public SendModel(Smallbank.Data.SmallbankContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Transaction Send { get; set; } = default!;

        private async Task SetModel(uint? id)
        {
            var account = await _context.Account.FirstOrDefaultAsync(m => m.CustomerId == id);
            if (account == null)
            {
                Send = new Transaction
                {
                    CustomerId = null
                }; 
                return;
            }

            Send = new Transaction
            {
                CustomerId = id,
                CustomerName = account.CustomerName,
                CheckingBalance = account.CheckingBalance
            };

        }
        public async Task<IActionResult> OnGetAsync(uint? id)
        {
            if (id == null || _context.Account == null)
            {
                return NotFound();
            }

            await SetModel(id);

            if(Send.CustomerId == null)
            {
                return NotFound();
            }

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["StatusMessage"] = string.Empty;


            if (!ModelState.IsValid || _context.Account == null || Send == null)
            {
                return Page();
            }

            string? m = await _context.Account.SendMoney(Send);

            if (m != null)
            {
                ViewData["StatusMessage"] = m;

            }

            await SetModel(Send.CustomerId);
            
            if (Send.CustomerId == null)
            {
                return NotFound();
            }

            return Page();
        }

    }
}
