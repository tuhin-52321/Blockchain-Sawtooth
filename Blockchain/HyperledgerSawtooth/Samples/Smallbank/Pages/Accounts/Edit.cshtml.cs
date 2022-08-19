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
    public class EditModel : PageModel
    {
        private readonly Smallbank.Data.SmallbankContext _context;

        public EditModel(Smallbank.Data.SmallbankContext context)
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

            var account =  await _context.Account.FirstOrDefaultAsync(m => m.CustomerId == id);
            if (account == null)
            {
                return NotFound();
            }
            Account = account;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            //Not implemented
            return RedirectToPage("./Index");
        }

    }
}
