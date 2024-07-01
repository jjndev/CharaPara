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
    public class UndoDeleteTabModel : PageModel
    {



        private readonly ApplicationDbContext _context;
        private readonly IProfileAuthorizationService _profileAuthorizationService;

        public UndoDeleteTabModel(ApplicationDbContext context, IProfileAuthorizationService profileAuthorizationService)
        {
            _context = context;
            _profileAuthorizationService = profileAuthorizationService;
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

            //if the tab is not deleted, redirect.
            if (requestedTab.DateTimeDeleted == null)
            {
                return Redirect($"../Profile/{requestedTab.Profile.Id}");
            }

            //check if this tab was deleted recently enough to allow an undo.
            if (requestedTab.DateTimeDeleted < DateTime.UtcNow.AddMinutes(-10))
            {
                return Unauthorized();
            }

            //before allowing an undo, make sure that the user hasn't created a new tab since deleting this one.
            if (await _context.Tabs
                .AnyAsync(x => x.ProfileId == requestedTab.ProfileId && x.DateTimeCreated >= requestedTab.DateTimeDeleted))
            {
                return Unauthorized();
            }

            //delete this tab
            requestedTab.DateTimeDeleted = null;
            await _context.SaveChangesAsync();

            //report to the redirect page that this tab has been deleted
            TempData["TabRestored"] = true;

            //redirect to the profile page
            return Redirect($"../Profile/{requestedTab.Profile.Id}");
        }
    }
}
