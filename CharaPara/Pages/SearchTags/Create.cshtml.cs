using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages.SearchTags
{
    public class CreateModel : PageModel
    {
        private readonly CharaPara.Data.ApplicationDbContext _context;

        public CreateModel(CharaPara.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["ParentSearchTagId"] = new SelectList(_context.SearchTags, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public SearchTag SearchTag { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.SearchTags.Add(SearchTag);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
