using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Ganss.Xss;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CharaPara.App;
using CharaPara.Data;
using CharaPara.Data.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CharaPara.Pages.Profile
{
    public class EditTabModel : PageModel
    {

        public class VM_EditTab
        {
            public int TabId { get; set; }
            public string Name { get; set; }

            [Range(1, 250)]
            public int TabPosition { get; set; } = 1;

            [MaxLength(15000)]
            public string RawContent { get; set; }

            

            public VM_EditTab()
            {
                Name = "New Tab";
                RawContent = "";
                TabPosition = 0;
            }

            public VM_EditTab(int tabId, string name, int tabPosition, string rawContent)
            {
                TabId = tabId;
                Name = name;
                TabPosition = tabPosition;
                RawContent = rawContent;
            }

        }



        private readonly ApplicationDbContext _context;
        private readonly IProfileAuthorizationService _profileAuthorizationService;
        private readonly IUserInputMessageFormatService _messageFormatService;

        public EditTabModel(ApplicationDbContext context, IProfileAuthorizationService profileAuthorizationService, IUserInputMessageFormatService userInputMessageFormatService)
        {
            _context = context;
            _profileAuthorizationService = profileAuthorizationService;
            _messageFormatService = userInputMessageFormatService;
        }

        public async Task<IActionResult> OnGetAsync(int? t = null)
        {
            if (t == null) return NotFound();

            //get the tab and the profile that owns it
            var requestedTab = await _context.Tabs
                .Where(x => x.Id == t)
                .Include(x => x.Profile)
                .FirstOrDefaultAsync();

            if (requestedTab == null || requestedTab.Profile == null) return NotFound();

            //check if this user has permission to edit the profile this tab is featured on
            //TODO: make a check for tab edit permissions instead of just profile permissions - future feature for GMs
            if (await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(requestedTab.Profile, this) == false)
            {
                return Unauthorized();
            }

            //populate the viewmodel
            TabViewModel = new VM_EditTab(requestedTab.Id, requestedTab.Name, requestedTab.Order + 1, requestedTab.RawContent);

            return Page();
        }

        [BindProperty]
        public VM_EditTab TabViewModel { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            //check if viewmodel is valid
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //get the tab and the profile that owns it
            var requestedTab = await _context.Tabs
                .Where(x => x.Id == TabViewModel.TabId)
                .Include(x => x.Profile)
                .FirstOrDefaultAsync();

            if (requestedTab == null || requestedTab.Profile == null) return NotFound();

            //check if this user has permission to edit the profile this tab is featured on
            //TODO: make a check for tab edit permissions instead of just profile permissions - future feature for GMs
            if (await _profileAuthorizationService.CheckUserProfileAuthorization_Edit(requestedTab.Profile, this) == false)
            {
                return Unauthorized();
            }

            //update the tab with the viewmodel's values
            requestedTab.Name = TabViewModel.Name;
            requestedTab.RawContent = TabViewModel.RawContent;

            byte oldTabOrder = requestedTab.Order;
            byte newTabOrder = (byte)Math.Clamp(TabViewModel.TabPosition - 1, 0, 255);

            _context.Attach(requestedTab).State = EntityState.Modified;

            //if the order was updated, we edit all of the tabs' orders to match
            if (oldTabOrder != newTabOrder)
            {
                //TODO: optimize?
                var tabsList = await _context.Tabs.Where(x => x.ProfileId == requestedTab.Profile.Id).OrderBy(x => x.Order).ToListAsync();

                newTabOrder = (byte)Math.Min(newTabOrder, tabsList.Count - 1);

                for (int i = 0; i < tabsList.Count; i++)
                {
                    if (tabsList[i].Id == requestedTab.Id)
                    {
                        continue;
                    }
                    tabsList[i].Order = (i >= newTabOrder) ? (byte)(i + 1) : (byte)i;
                }

                requestedTab.Order = newTabOrder;
            }


            //rudimentary html string formatting
            //var sanitizer = new HtmlSanitizer(new HtmlSanitizerOptions()
            //{
            //    //AllowedTags = new[] { }
            //});
            //var htmlContent = sanitizer.Sanitize(TabViewModel.RawContent);




            //htmlContent = htmlContent.Replace(Environment.NewLine, "<br>");
            var htmlContent = await _messageFormatService.FormatTextAsync(TabViewModel.RawContent);
            if (htmlContent.Length > 32000) htmlContent = htmlContent.Substring(0, 32000);

            requestedTab.GeneratedHtmlContent = htmlContent;

            //save changes to the database
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
            return Redirect($"../Profile/{requestedTab.Profile.Id}");


            //return RedirectToPage("./Index");
        }
    }
}
