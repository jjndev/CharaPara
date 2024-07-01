using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;
using System.Security.Claims;


namespace CharaPara.Pages
{
    public class ProfileModel : PageModel
    {
        public Data.Model.Profile RequestedProfile { get; set; }
        public bool HasEditPermissions { get; set; }
        
        public string? WriterName {  get; set; }

        public int? WriterId { get; set; }

        public List<SearchTag> RequestedProfileSearchTags { get; set; }

        public List<Tab> ProfileTabs { get; set; }

        public List<string> SearchTags { get; set; }

        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly IProfileAuthorizationService _profileAuthorizationService;
        public string AvatarUrl { get; set; }



        public ProfileModel(ApplicationDbContext db, UserManager<AppUser> userManager, IProfileAuthorizationService profileAuthorizationService)
        {
            _db = db;
            _userManager = userManager;
            _profileAuthorizationService = profileAuthorizationService;
            HasEditPermissions = false;
            Console.WriteLine($"ProfileAuthorizationService: {_profileAuthorizationService}");
            AvatarUrl = "";
        }

        public async Task<IActionResult> OnGetAsync([FromRoute] string profileIdString = null)
        {
            //if (!int.TryParse(PageContext.RouteData.Values["id"]?.ToString(), out int profileId))
            if (profileIdString == null || !int.TryParse(profileIdString, out int profileId))
            {
                // Handle the case where the profile ID is not provided or is invalid
                return RedirectToPage("/Error");
            }

            var getProfile = await _db.Profiles.FirstOrDefaultAsync(m => m.Id == profileId);

            // Handle the case where the profile is not found
            if (getProfile == null)
            {
                return NotFound();
            }

            

            // Check if the user is authorized to access the requested profile
            //if (!await _profileAuthorizationService.CheckUserProfileAuthorization_View(getProfile, this))



            if (!await _profileAuthorizationService.CheckUserProfileAuthorization_View(getProfile, this))     
            {
                return Forbid();
            }

            // Check if the user has edit permissions
            //HasEditPermissions = await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(getProfile, this);
            HasEditPermissions = await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(getProfile, this);

            RequestedProfile = getProfile;

            AvatarUrl = RequestedProfile.GetAvatarFilePath();

            //get the profile's tabs
            ProfileTabs = await _db.Tabs.Where(tab => tab.ProfileId == getProfile.Id && tab.DateTimeDeleted == null).OrderBy(tab => tab.Order).ToListAsync();


            // get the profile's search tags

            //var profileSearchTagIds = RequestedProfile.SearchTagIds;
            //RequestedProfileSearchTags = await _db.SearchTags
            //    .Where(st => profileSearchTagIds.Contains(st.Id))
            //    .ToListAsync();



            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Page();
        }
    }
}
