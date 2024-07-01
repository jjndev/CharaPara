using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages.SearchTags
{
    public class DeleteModel : PageModel
    {
        private readonly CharaPara.Data.ApplicationDbContext _context;

        public DeleteModel(CharaPara.Data.ApplicationDbContext context)
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

            var searchtag = await _context.SearchTags.FirstOrDefaultAsync(m => m.Id == id);

            if (searchtag == null)
            {
                return NotFound();
            }
            else
            {
                SearchTag = searchtag;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var searchtag = await _context.SearchTags.FindAsync(id);
            if (searchtag != null)
            {
                SearchTag = searchtag;
                _context.SearchTags.Remove(SearchTag);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
