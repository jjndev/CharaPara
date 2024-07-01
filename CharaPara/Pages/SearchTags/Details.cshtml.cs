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
    public class DetailsModel : PageModel
    {
        private readonly CharaPara.Data.ApplicationDbContext _context;

        public DetailsModel(CharaPara.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}
