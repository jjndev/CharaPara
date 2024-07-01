using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CharaPara.App;
using CharaPara.Data;
using CharaPara.Data.Model;
using System.Diagnostics;

namespace CharaPara.Pages.Profile
{
    public class EditAvatarModel : PageModel
    {
        [BindProperty]


        public VM_Profile_EditCharacterAvatar VMProfile { get; set; } = default!;

        private ApplicationDbContext _context { get; set; }
        private readonly UserManager<AppUser> _userManager;
        private IUserImageUploadHandler _uploadHandler { get; set; }
        private readonly IProfileAuthorizationService _profileAuthorizationService;

        public string OriginalAvatarUrl { get; set; }

        [BindProperty]
        public VM_Profile_EditCharacterAvatar ProfileVM { get; set; } = default!;

        public EditAvatarModel(IUserImageUploadHandler uploadHandler, ApplicationDbContext context, UserManager<AppUser> userManager, IProfileAuthorizationService profileAuthorizationService)
        {
            _uploadHandler = uploadHandler;
            _profileAuthorizationService = profileAuthorizationService;
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> OnGetAsync(int? p)
        {
            if (p == null)
            {
                return NotFound();
            }

            ProfileVM = new VM_Profile_EditCharacterAvatar(p.Value);

            //get the avatar url for this profile
            OriginalAvatarUrl = await _context.Profiles
                .Where(x => x.Id == p)
                .Select(x => x.ImageFileName_Avatar)
                .FirstOrDefaultAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (VMProfile.Avatar == null || VMProfile.Id == null)
            {
                return Page();
            }

            var profile = await _context.Profiles
                .Where(x => x.Id == VMProfile.Id)
                .FirstOrDefaultAsync();

            //authenticate the user
            var appUser = await _userManager.GetUserAsync(User);

            if (await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(profile, appUser) == false)
            {
                return Forbid();
            }

            //attempt to upload the supplied image

            var result = await _uploadHandler.UploadImageAsync(
                formFile: VMProfile.Avatar,
                appUser: appUser,
                relatedId: VMProfile.Id,
                uploadType: IUserImageUploadHandler.ImageCategory.ProfileAvatar
            );


            switch (result.ResultCode)
            {
                case IUserImageUploadHandler.ImageUploadResultCode.Success:
                    Console.WriteLine("upload success");
                    TempData["AvatarUploadSuccess"] = "Upload Successful!";
                    break;
                default:
                    Console.WriteLine("upload failed: " + result.ToString());
                    TempData["AvatarUploadFail"] = "Upload Failed: " + result.ToString();
                    break;
            }

            //update the profile's avatar url
            profile.ImageFileName_Avatar = result.UploadUrl;
            await _context.SaveChangesAsync();

            return Redirect($"./Profile/{profile.Id}");

        }
    }
}
