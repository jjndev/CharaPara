using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages
{
    public class EditProfileTagsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IProfileAuthorizationService _profileAuthorizationService;

        public List<SearchTag> ChosenSearchTags { get; set; }

        public EditProfileTagsModel(ApplicationDbContext context, IProfileAuthorizationService profileAuthorizationService)
        {
            _context = context;
            _profileAuthorizationService = profileAuthorizationService;
        }

        public async Task<IActionResult> OnGetAsync(int? p)
        {
            var id = p;

            if (id == null) return NotFound();
            var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == id);
            if (profile == null) return NotFound();

            if (!await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(profile, this))
                return Forbid();

            //for each search tag in the profile, search for a matching search tag in the database
            //ChosenSearchTags = new List<SearchTag>();
            //foreach (var profileSearchTagId in profile.SearchTagIds)
            //{
            //    var searchTag = await _context.SearchTags.FirstOrDefaultAsync(s => s.Id == profileSearchTagId);
            //    if (searchTag != null)
            //    {
            //        ChosenSearchTags.Add(searchTag);
            //    }
            //}
            
            return Page();
        }

        public JsonResult OnGetSearchForTag(string q)
        {
            var searchResults = _context.SearchTags
                .Where(e => e.Name.Contains(q) || e.SearchAliases.Contains(q)) 
                .ToList();

            return new JsonResult(searchResults);
        }
    }
}
