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
    public class WriteCheckModel : PageModel
    {
        private readonly Smallbank.Data.SmallbankContext _context;

        public WriteCheckModel(Smallbank.Data.SmallbankContext context)
        {
            _context = context;
        }

        [BindProperty]
        public VMTransaction WriteCheck { get; set; } = default!;

        private async Task SetModel(uint id)
        {
            var account = await _context.Account.FirstOrDefaultAsync(m => m.CustomerId == id);
            if (account == null)
            {
                WriteCheck = new VMTransaction();
                return;
            }

            WriteCheck = new VMTransaction
            {
                CustomerId = id,
                CustomerName = account.CustomerName,
                CheckingBalance = account.CheckingBalance
            };

        }
        public async Task<IActionResult> OnGetAsync(uint id)
        {
            if (id == 0 || _context.Account == null)
            {
                return NotFound();
            }

            await SetModel(id);

            if (WriteCheck.CustomerId == 0)
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


            if (!ModelState.IsValid || _context.Account == null || WriteCheck == null)
            {
                return Page();
            }

            string? m = await _context.Account.WriteCheck(WriteCheck);

            if (m != null)
            {
                ViewData["StatusMessage"] = m;

            }

            await SetModel(WriteCheck.CustomerId);

            if (WriteCheck.CustomerId == 0)
            {
                return NotFound();
            }


            return Page();
        }

    }
}
