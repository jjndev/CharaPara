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
    public class IndexModel : PageModel
    {
        private readonly CharaPara.Data.ApplicationDbContext _context;

        public IndexModel(CharaPara.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<SearchTag> SearchTag { get;set; } = default!;

        public async Task OnGetAsync()
        {
            SearchTag = await _context.SearchTags
                .Include(s => s.ParentSearchTag).ToListAsync();
        }
    }
}
