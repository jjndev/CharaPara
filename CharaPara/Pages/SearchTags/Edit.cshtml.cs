using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages.SearchTags
{
    public class EditModel : PageModel
    {
        private readonly CharaPara.Data.ApplicationDbContext _context;

        public EditModel(CharaPara.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SearchTag SearchTag { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var searchtag =  await _context.SearchTags.FirstOrDefaultAsync(m => m.Id == id);
            if (searchtag == null)
            {
                return NotFound();
            }
            SearchTag = searchtag;
           ViewData["ParentSearchTagId"] = new SelectList(_context.SearchTags, "Id", "Id");
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

            _context.Attach(SearchTag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SearchTagExists(SearchTag.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool SearchTagExists(short id)
        {
            return _context.SearchTags.Any(e => e.Id == id);
        }
    }
}
