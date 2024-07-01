using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Mysqlx;
using CharaPara.App;
using CharaPara.Data;
using CharaPara.Data.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CharaPara.Pages.Profile
{
    public class CreateCharacterModel : PageModel
    {

        public class VM_Profile_Createcharacter
        {
            [Required, Length(1, 30)]
            public string Name { get; set; }

            [Required, DefaultValue(Gender.NoneSelected)]
            public Gender Gender { get; set; }


            [Required, DefaultValue(ProfileOriginCategory.Original)]
            public ProfileOriginCategory OriginCategory { get; set; }

            [DefaultValue(""), MaxLength(50)]
            public string? Title { get; set; }

            public VM_Profile_Createcharacter()
            {
                Name = "<ProfileCreateCharacter_Error>";
            }

            public void SetProfileOriginCategory(string category)
            {
                switch (category)
                {
                    case "Original":
                        OriginCategory = ProfileOriginCategory.Original;
                        break;
                    case "Canon":
                        OriginCategory = ProfileOriginCategory.Canon;
                        break;
                    default:
                        OriginCategory = ProfileOriginCategory.Unknown;
                        break;
                }
            }

            public VM_Profile_Createcharacter(Data.Model.Profile profile)
            {
                Name = profile.Name;
                Gender = profile.Gender;
                Title = profile.Title;
            }
        }



        private readonly CharaPara.Data.ApplicationDbContext _context;
        private readonly IUserInputMessageFormatService _messageFormatService;

        private static string CreateTabStatsTemplateString = """
Height:

Age:

Species:

Occupation:

Birthplace:

---

The first thing you notice:

Personality:

Skills:

Hobbies:

Short-Term Goal:

Long-Term Goal:
""";

        public CreateCharacterModel(CharaPara.Data.ApplicationDbContext context, IUserInputMessageFormatService messageFormatService)
        {
            _context = context;
            _messageFormatService = messageFormatService;
        }

        public async Task<IActionResult> OnGet()
        {
            //the user must be logged in to create a profile
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) { return Redirect("../Login"); }

            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public VM_Profile_Createcharacter Profile { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null) { return Redirect("../Login"); }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var newProfile = new Data.Model.Profile
            {
                AppUser = user,
                SensitiveStatus = SensitiveStatus.None,
                ProfileType = ProfileType.Avatar,
                Title = Profile.Title,
                Name = Profile.Name,
                DateTimeCreated = DateTimeOffset.Now,
                DateTimeModified = DateTimeOffset.Now,
                DateTimeAvatarModified = DateTimeOffset.Now,
                DateTimeLastSeen = DateTimeOffset.Now,
                DateTimeLastSeenPublic = DateTimeOffset.Now
            };
            
            var result = _context.Profiles.Add(newProfile);

            //add a Tab to the profile
            var newTab = new Tab
            {
                Name = "Biography",
                RawContent = "",
                GeneratedHtmlContent = "",
                Profile = newProfile,
                ProfileId = newProfile.Id
                
            };
            _context.Tabs.Add(newTab);


            var formattedStatsString = await _messageFormatService.FormatTextAsync(CreateTabStatsTemplateString);

            newTab = new Tab
            {
                Name = "Stats",
                RawContent = CreateTabStatsTemplateString,
                GeneratedHtmlContent = formattedStatsString,
                Profile = newProfile,
                ProfileId = newProfile.Id
            };
            _context.Tabs.Add(newTab);

            await _context.SaveChangesAsync();

            return Redirect($"/Profile/{newProfile.Id}");
        }
    }
}
