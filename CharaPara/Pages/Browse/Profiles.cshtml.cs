using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages.Browse
{
    public class ProfilesModel : PageModel
    {
        private readonly ILogger<ProfilesModel> _logger;
        private readonly ApplicationDbContext _context;

        public ProfilesModel(ILogger<ProfilesModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
            RecentProfileList = new List<Data.Model.Profile>();
        }

        public List<Data.Model.Profile> RecentProfileList { get; set; }


        public async Task<IActionResult> OnGet()
        {
            //get a list of profiles from the database.

            RecentProfileList = await _context.Profiles
                .Where(x => x.DateTimeDeleted == null && x.VisibleStatus == VisibleStatus.Public)
                .OrderByDescending(x => x.DateTimeCreated.ToString())
                .Take(12)
                .ToListAsync();

            return Page();
        }
    }
}
