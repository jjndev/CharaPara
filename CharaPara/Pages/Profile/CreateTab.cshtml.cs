using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CharaPara.App;
using CharaPara.Data;
using CharaPara.Data.Model;

namespace CharaPara.Pages.Profile
{
    public class CreateTabModel : PageModel
    {

        public class VM_CreateTab
        {

            public int ProfileId { get; set; }
            [Length(1, 50)]
            public string Name { get; set; }
            [MaxLength(15000)]
            public string RawContent { get; set; }

            public byte ProfileCurrentTabCount { get; set; } = 0;

            public VM_CreateTab()
            {
                ProfileId = 0;
                Name = "New Tab";
                RawContent = "";
            }

            public VM_CreateTab(int profileId)
            {
                ProfileId = profileId;
                Name = "New Tab";
                RawContent = "";
            }

        }



        private readonly ApplicationDbContext _context;
        private readonly IProfileAuthorizationService _profileAuthorizationService;
        private readonly IUserInputMessageFormatService _messageFormatService;

        public CreateTabModel(ApplicationDbContext context, IProfileAuthorizationService profileAuthorizationService, IUserInputMessageFormatService userInputMessageFormatService)
        {
            _context = context;
            _profileAuthorizationService = profileAuthorizationService;
            _messageFormatService = userInputMessageFormatService;
        }

        public async Task<IActionResult> OnGetAsync(int? p = null)
        {
            if (p == null) return NotFound();

            TabViewModel = new VM_CreateTab(p.Value);



            return Page();
        }

        [BindProperty]
        public VM_CreateTab TabViewModel { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            //check if viewmodel is valid
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //locate the profile
            var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == TabViewModel.ProfileId);
            if (profile == null) return NotFound();

            //check if profile is authorized
            if (await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(profile, this) == false)
            {
                return Unauthorized();
            }

            var tabCount = await _context.Tabs.CountAsync(t => t.ProfileId == profile.Id);
            if (tabCount > 30)
            {
                //add error message to modelstate
                ModelState.AddModelError("Name", "Error: You can only have 30 tabs.");
            }

            //TODO: auto-moderate text?


            //rudimentary string formatting
            //var htmlContent = TabViewModel.RawContent.Replace(Environment.NewLine, "<br>");
            var htmlContent = await _messageFormatService.FormatTextAsync(TabViewModel.RawContent);
            if (htmlContent.Length > 32000) htmlContent = htmlContent.Substring(0, 32000);

            //create new tab with data from the viewmodel
            var newTab = new Tab()
            {
                ProfileId = TabViewModel.ProfileId,
                Name = TabViewModel.Name,
                RawContent = TabViewModel.RawContent,
                GeneratedHtmlContent = htmlContent, 
                Order = (byte)tabCount
            };

            //add the new tab to the database
            _context.Tabs.Add(newTab);
            await _context.SaveChangesAsync();


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //return an error if the save fails
                return RedirectToPage("/Error");
            }

            //redirect to the profile page
            return Redirect($"../Profile/{profile.Id}");


            //return RedirectToPage("./Index");
        }
    }
}
