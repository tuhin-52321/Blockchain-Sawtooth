using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Smallbank.Data;
using Smallbank.Models;

namespace Smallbank.Pages.Accounts
{
    public class CreateModel : PageModel
    {
        private readonly Smallbank.Data.SmallbankContext _context;

        public CreateModel(Smallbank.Data.SmallbankContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["StatusMessage"] = string.Empty;
            return Page();
        }

        [BindProperty]
        public Account Account { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["StatusMessage"] = string.Empty;


            if (!ModelState.IsValid || _context.Account == null || Account == null)
            {
                return Page();
            }

            string? m = await _context.Account.Add(Account);

            if(m != null)
            {
                ViewData["StatusMessage"] = m;

            }

            return Page();
        }
    }
}
