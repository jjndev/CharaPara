
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amazon.S3;
using Amazon.S3.Model;
using CharaPara.Data;
using Microsoft.AspNetCore.Identity;
using CharaPara.Data.Model;
using CharaPara.App;
using System.ComponentModel.DataAnnotations;

namespace CharaPara.Pages.Profile
{
    public class EditGalleryImageModel : PageModel
    {
        public class VM_AddGalleryImage_Edit
        {
            [Required(ErrorMessage = "Please select the type of image that was uploaded"), Display(Name = "Image Type")]
            public GalleryImageCreationType CreationType { get; set; }

            [Required(ErrorMessage = "Please name the (Artist / Editor / Photographer / Source Material / AI Model) that this image is sourced from.")]
            [StringLength(100), Display(Name = "Image Credit")]
            public string? SourceCredit { get; set; }

            [StringLength(50), Display(Name = "Title (Optional)")]
            public string? Title { get; set; } // Property to bind your text data from the form

            [StringLength(1000), Display(Name = "Description (Optional)")]
            public string? Description { get; set; }

            [Required]
            public int ProfileId { get; set; }

            public VM_AddGalleryImage_Edit()
            {
                Title = "Image";
            }
        }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserImageCloudUploadManager _uploadManager;


        [BindProperty]
        public VM_AddGalleryImage_Edit GalleryImageVM { get; set; } = default!;


        public EditGalleryImageModel(ApplicationDbContext context, UserManager<AppUser> userManager, IUserImageCloudUploadManager uploadManager)
        {
            _context = context;
            _userManager = userManager;
            _uploadManager = uploadManager;
        }

        public async Task<IActionResult> OnGetAsync(int? p = null)
        {
            if (User.Identity.IsAuthenticated == false)
            {
                return Unauthorized();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            //TODO: Unfinished page

            return Page();
        }



        public async Task<IActionResult> OnPost()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null) return Unauthorized();

            //TODO: Unfinished page

            return Page();
        }
    }
}