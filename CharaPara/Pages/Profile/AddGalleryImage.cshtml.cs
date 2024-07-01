
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
    public class AddGalleryImageModel : PageModel
    {
        public class VM_AddGalleryImage_Create
        {
            [Required(ErrorMessage = "Please select the type of image you are uploading."), Display(Name = "Image Type")]
            public GalleryImageCreationType CreationType { get; set; }

            [Required(ErrorMessage = "Please name the (Artist / Editor / Photographer / Source Material / AI Model) that this image is sourced from.")]
            [StringLength(100), Display(Name = "Image Credit")]
            public string? SourceCredit { get; set; }

            public int? ProfileId { get; set; }

            public VM_AddGalleryImage_Create()
            {

            }
        }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserImageCloudUploadManager _uploadManager;
        private readonly IProfileAuthorizationService _profileAuthorizationService;

        public string PreSignedUrl { get; private set; }

        [BindProperty]
        public VM_AddGalleryImage_Create GalleryImageVM { get; set; } = default!;


        public AddGalleryImageModel(
            ApplicationDbContext context, 
            UserManager<AppUser> userManager, 
            IUserImageCloudUploadManager uploadManager,
            IProfileAuthorizationService profileAuthorizationService)
        {
            _context = context;
            _userManager = userManager;
            _uploadManager = uploadManager;
            _profileAuthorizationService = profileAuthorizationService;
            PreSignedUrl = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int? p = null)
        {
            //disable this page for now.
            return Unauthorized();

            if (p == null) return NotFound();

            //get appuser
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return Unauthorized();
            
            //check that the user has permission to edit the chosen profile
            if (await _profileAuthorizationService.CheckUserProfileAuthorization_Edit((int)p, appUser) == false)
            {
                return Unauthorized();
            }

            //set the preloaded variables
            GalleryImageVM = new VM_AddGalleryImage_Create()
            {
                ProfileId = p
            };

            //get presigned url for image uploads
            PreSignedUrl = await _uploadManager.GenerateUserUploadUrlAsync(appUser.Id, IUserImageCloudUploadManager.UploadImageType.GalleryImage);

            if (PreSignedUrl == null)
            {
                return Redirect("./Error");
            }

            return Page();
        }



        public async Task<IActionResult> OnPost()
        {
            //disable this page for now.
            return Unauthorized();

            //validate the model
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //validate the user
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return Unauthorized();

            //ensure there is a profileId given, and we have permission to edit that profile
            if (GalleryImageVM.ProfileId == null) return NotFound();
            if (await _profileAuthorizationService.CheckUserProfileAuthorization_Edit((int)GalleryImageVM.ProfileId, appUser) == false)
            {
                return Unauthorized();
            }

            //validate the image that the user should've uploaded in order to send this post request

            var userUploadResult = await _uploadManager.ValidateUserUploadAsync(appUser.Id, IUserImageCloudUploadManager.UploadImageType.GalleryImage);
            
            Console.WriteLine($"User upload result: {userUploadResult.validationSucceeded}");

            if (!userUploadResult.validationSucceeded || userUploadResult.imageExtensionWithPeriod == null)
            {
                //TODO: the upload failed; report this somehow
                return Page();
            }

            //create a new gallery image
            var newGalleryImage = new GalleryImage
            {
                SourceCredit = GalleryImageVM.SourceCredit,
                CreationType = GalleryImageVM.CreationType,
                ProfileId = (int)GalleryImageVM.ProfileId
            };

            _context.GalleryImages.Add(newGalleryImage);
            await _context.SaveChangesAsync();

            //'confirm' the user upload, using the database record's ID for the new key
            var userUploadConfirmation = 
                await _uploadManager.ConfirmUserUploadAsync(
                    appUser.Id, 
                    newGalleryImage.Id, 
                    userUploadResult.imageExtensionWithPeriod,
                    IUserImageCloudUploadManager.UploadImageType.GalleryImage
                );
            if (userUploadConfirmation.imageKey == null)
            {
                //upload failed for some reason. undo the database changes
                //TODO: report that the upload failed
                _context.GalleryImages.Remove(newGalleryImage);
                //await _context.SaveChangesAsync();
                //return Page();
            }
            else
            {
                //upload success; put the keys into the database
                newGalleryImage.ImagePath = userUploadConfirmation.imageKey;
                newGalleryImage.ThumbnailPath = userUploadConfirmation.thumbnailKey;
            }
            await _context.SaveChangesAsync();
            return Redirect($"../Profile/{GalleryImageVM.ProfileId}");
        }
    }
}