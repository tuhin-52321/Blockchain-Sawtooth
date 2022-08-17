using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Smallbank.Data;
using Smallbank.Models;

namespace Smallbank.Pages.Accounts
{
    public class DeleteModel : PageModel
    {
        private readonly Smallbank.Data.SmallbankContext _context;

        public DeleteModel(Smallbank.Data.SmallbankContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(uint? id)
        {
            if (id == null || _context.Account == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FirstOrDefaultAsync(m => m.ID == id);

            if (account == null)
            {
                return NotFound();
            }
            else 
            {
                Account = account;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(uint? id)
        {
            if (id == null || _context.Account == null)
            {
                return NotFound();
            }
            var account = await _context.Account.FindAsync(id);

            if (account != null)
            {
                Account = account;
                //Not implemented
            }

            return RedirectToPage("./Index");
        }
    }
}
