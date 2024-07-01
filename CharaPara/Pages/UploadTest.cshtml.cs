using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CharaPara.App;
using CharaPara.Data;
using CharaPara.Data.Model;
using System.Diagnostics;

namespace CharaPara.Pages
{
    public class UploadTestModel : PageModel
    {
        [BindProperty]
        public IFormFile FormFile { get; set; } = default!;
        
        private readonly IUserImageUploadHandler _uploadHandler;
        private readonly UserManager<AppUser> _userManager;



        public UploadTestModel(IUserImageUploadHandler uploadHandler, UserManager<AppUser> userManager)
        {
            _uploadHandler = uploadHandler;
            _userManager = userManager;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (FormFile == null)
            {
                return Page();
            }

            //get the appuser

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return Page();
            }

            //upload formFile using the uploadservice

            var result = await _uploadHandler.UploadImageAsync(
                formFile: FormFile,
                appUser: appUser,
                relatedId: null,
                uploadType: IUserImageUploadHandler.ImageCategory.TestUpload
            );

            switch (result.ResultCode)
            {
                case IUserImageUploadHandler.ImageUploadResultCode.Success:
                    Console.WriteLine("upload success");
                    break;
                default:
                    Console.WriteLine("upload failed: " + result.ToString());
                    break;
            }

            //result.

            Console.WriteLine(result);
            return Page();

        }
    }
}
