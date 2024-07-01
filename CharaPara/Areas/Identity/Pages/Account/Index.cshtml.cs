using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;
using System.Security.Claims;

namespace CharaPara.Areas.Identity.Pages.Account
{
    public class IndexModel : PageModel
    {
        //private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        public string StatusMessage { get; set; }
        public string Username { get; set; }
        public ICollection<Profile> Profiles { get; set; }


        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not { } userId) return RedirectToPage("./Login");

            var user = await _db.Users
            .Where(u => u.Id == userId)
            //.Select(u => new AppUserSummaryAndProfilesDto
            //{
            //    Name = u.UserName,
            //    Profiles = u.Profiles
            //})
            .FirstOrDefaultAsync();


            Username = User.Identity.Name; 
            Profiles = await _db.Profiles.Where(p => p.AppUserId == userId).ToListAsync();
            return Page();
        }
    }
}
