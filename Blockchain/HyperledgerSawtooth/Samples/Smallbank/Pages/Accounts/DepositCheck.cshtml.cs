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
    public class DepositCheckModel : PageModel
    {
        private readonly Smallbank.Data.SmallbankContext _context;

        public DepositCheckModel(Smallbank.Data.SmallbankContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DepositCheck DepositCheck { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(uint? id)
        {
            if (id == null || _context.Account == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FirstOrDefaultAsync(m => m.CustomerId == id);
            if (account == null )
            {
                return NotFound();
            }

            var check = new DepositCheck 
            { 
                CustomerId = id,
                CustomerName = account.CustomerName,
                CheckingBalance = account.CheckingBalance
            };

            DepositCheck = check;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["StatusMessage"] = string.Empty;


            if (!ModelState.IsValid || _context.Account == null || DepositCheck == null)
            {
                return Page();
            }

            string? m = await _context.Account.DepositCheck(DepositCheck);

            if (m != null)
            {
                ViewData["StatusMessage"] = m;

            }

            return Page();
        }

    }
}
