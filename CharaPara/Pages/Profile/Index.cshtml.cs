using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly CharaPara.Data.ApplicationDbContext _context;

        public IndexModel(CharaPara.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Data.Model.Profile> Profile { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Profile = await _context.Profiles
                .Include(p => p.AppUser).ToListAsync();
        }
    }
}
