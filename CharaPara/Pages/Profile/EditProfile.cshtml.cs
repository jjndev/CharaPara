using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace CharaPara.Pages.Profile
{

    public class EditProfileModel : PageModel
    {

        public class VM_Profile_EditCharacter
        {
            public int? Id { get; set; }

            [Required, Length(1, 30)]
            [Display(Name = "Name", Prompt = "The name of your character.")]
            public string? Name { get; set; }

            [Required, DefaultValue(Gender.NoneSelected)]
            public Gender Gender { get; set; }

            [DefaultValue(""), MaxLength(50)]
            public string? Title { get; set; }

            [DefaultValue(""), MaxLength(500)]
            public string? Summary { get; set; }

            [Required, DefaultValue(VisibleStatus.Public)]
            public VisibleStatus VisibleStatus { get; set; }

            public bool IsSensitive { get; set; }

            public bool IsMature { get; set; }




            [Required, DefaultValue(SensitiveStatus.None)]
            [Display(Name = "Mature Status", Prompt = "The name of your character.")]
            public SensitiveStatus SensitiveStatus { get; set; }


            [DefaultValue("#DDDDDD")]
            public string? TextColor { get; set; }

            [DefaultValue("#777777")]
            public string? ProfileColor { get; set; }

            [DefaultValue("#222222")]
            public string? BorderColor { get; set; }

            [DefaultValue(""), MaxLength(500)]
            public string? SearchTagString { get; set; }

            public IFormFile? AvatarFile { get; set; }

            [FileExtensions(Extensions = "gif,jpg,jpeg,png,bmp")]
            public string? AvatarFileName
            {
                get
                {
                    if (AvatarFile != null)
                        return AvatarFile.FileName;
                    else
                        return null;
                }
            }


            [NotMapped]
            public string[] SearchTags
            {
                get => SearchTagString == null ? [] : SearchTagString.Split(',');
                set => SearchTagString = string.Join(",", value);
            }

            public VM_Profile_EditCharacter(string name)
            {
                Name = name;
                //TextColor = default!;
                //ProfileColor = default!;
                //BorderColor = default!;
            }

            public VM_Profile_EditCharacter()
            {
                Name = "<Error>";
                Gender = Gender.NoneSelected;
                Title = default!;
                Summary = default!;
                SensitiveStatus = SensitiveStatus.None;
                VisibleStatus = VisibleStatus.Public;
                //TextColor = default!;
                //ProfileColor = default!;
                //BorderColor = default!;
            }

            public VM_Profile_EditCharacter(Data.Model.Profile profile)
            {
                Id = profile.Id;
                Name = profile.Name;
                Gender = profile.Gender;
                Title = profile.Title;
                Summary = profile.Summary;
                VisibleStatus = profile.VisibleStatus;
                TextColor = profile.TextColor;
                ProfileColor = profile.ProfileColor;
                BorderColor = profile.BorderColor;
                IsMature = profile.IsMature;
                IsSensitive = profile.IsSensitive;
                SearchTagString = profile.SearchTagString;
                TextColor = ColorFormat(profile.TextColor);
                ProfileColor = ColorFormat(profile.ProfileColor);
                BorderColor = ColorFormat(profile.BorderColor);
                //SearchTagIdString = string.Join(";", profile.SearchTagIds);
            }

            private string ColorFormat(string color)
            {
                if (color == null)
                    return "#DDDDDD";
                if (color[0] != '#')
                    color = "#" + color;
                return color;
            }
        }





        private readonly ApplicationDbContext _context;
        //private readonly UserManager<AppUser> _userManager;
        private readonly IProfileAuthorizationService _profileAuthorizationService;

        public EditProfileModel(ApplicationDbContext context, IProfileAuthorizationService profileAuthorizationService) //UserManager<AppUser> userManager)
        {
            _context = context;
            //_userManager = userManager;
            _profileAuthorizationService = profileAuthorizationService;
        }

        //private Data.Model.Profile Profile { get; set; };
        [BindProperty]
        public VM_Profile_EditCharacter ProfileVM { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? p)
        {
            var id = p;

            if (id == null) return NotFound();
            var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == id);
            if (profile == null) return NotFound();

            if (!await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(profile, this))
                return Forbid();

            //Profile = profile;
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");



            ProfileVM = new VM_Profile_EditCharacter(profile);

            return Page();
        }



        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {

            var checkProfileVM = ProfileVM;

            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateValue = ModelState[modelStateKey];
                    foreach (var error in modelStateValue.Errors)
                    {
                        // Log or display the error message
                        Console.WriteLine($"Error: {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == ProfileVM.Id);

            if (!await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(profile, this))
                return Forbid();


            //update the profile with ProfileVM's values
            profile.Title = ProfileVM.Title;
            profile.Name = ProfileVM.Name;
            profile.Gender = ProfileVM.Gender;
            profile.Title = ProfileVM.Title;
            profile.Summary = ProfileVM.Summary;
            profile.SensitiveStatus = ProfileVM.SensitiveStatus;
            profile.DateTimeModified = DateTimeOffset.Now;
            profile.IsMature = ProfileVM.IsMature;
            profile.IsSensitive = ProfileVM.IsSensitive;
            profile.SearchTagString = ProfileVM.SearchTagString == null ? "" : ProfileVM.SearchTagString.Replace(',', ';');
            profile.ProfileColor = ProfileVM.ProfileColor ?? "FFFFFF";
            profile.BorderColor = ProfileVM.BorderColor ?? "000000";
            profile.TextColor = ProfileVM.TextColor ?? "000000";

            _context.Attach(profile).State = EntityState.Modified;




            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProfileExists(profile.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Redirect($"../Profile/{profile.Id}");
        }

        public JsonResult OnGetSearchForTag(string q)
        {
            var searchResults = _context.SearchTags
                .Where(e => e.Name.Contains(q) || e.SearchAliases.Contains(q))
                .ToList();

            return new JsonResult(searchResults);
        }

        private async Task<bool> ProfileExists(int id)
        {
            return await _context.Profiles.AnyAsync(e => e.Id == id);
        }



        private string ValidateSearchTagString(string searchTagString)
        {
            //filter the string in title case
            var returnString = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(searchTagString);
            //split and trim the string by comma
            var returnStringList = searchTagString.Split(',').Select(x => x.Trim()).ToList();

            //return the string
            return string.Join(",", returnStringList);
        }
    }
}

