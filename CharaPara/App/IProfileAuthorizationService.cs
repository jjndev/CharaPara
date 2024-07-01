using CharaPara.Data;
using CharaPara.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;


public interface IProfileAuthorizationService {

    /// <summary>
    /// Checks if the user can view the profile.
    /// </summary> 
    /// <returns>True if the user can view the profile.</returns>
    public Task<bool> CheckUserProfileAuthorization_View(Profile profile, AppUser? loggedInUser);
    public Task<bool> CheckUserProfileAuthorization_Edit(Profile profile, AppUser? loggedInUser);
    public Task<bool> CheckUserProfileAuthorization_View(Profile profile, PageModel pageModel);
    public Task<bool> CheckUserProfileAuthorization_Edit(Profile profile, PageModel pageModel);

    public Task<bool> CheckUserProfileAuthorization_View(int profileId, AppUser? loggedInUser);
    public Task<bool> CheckUserProfileAuthorization_Edit(int profileId, AppUser? loggedInUser);



    public Task<AppUser?> GetAppUserAsync(PageModel pageModel);
    public Task<AppUser?> GetAppUserAsync(ClaimsPrincipal user);
}

public class ProfileAuthorizationService : IProfileAuthorizationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ProfileAuthorizationService(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<AppUser?> GetAppUserAsync(PageModel pageModel)
    {
        return await _userManager.GetUserAsync(pageModel.User);
    }

    public async Task<AppUser?> GetAppUserAsync(ClaimsPrincipal user)
    {
        return await _userManager.GetUserAsync(user);
    }

    private async Task<bool> IsProfileActive(Profile profile) {
        return profile.VisibleStatus != VisibleStatus.DeletedByUser && profile.VisibleStatus != VisibleStatus.DeletedBySiteMod;
    }


    public async Task<bool> CheckUserProfileAuthorization_View(Profile profile, AppUser? loggedInUser)
    {
        
        //var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == id);

        if (profile == null)
        {
            return false;
        }

        //being logged in isn't required for public profiles
        if (profile.VisibleStatus == VisibleStatus.Public)
            return true;
        
        //otherwise, check for a login
        if (loggedInUser == null)
            return false;

        switch (profile.VisibleStatus) {
            //for logged in users, return true.
            case VisibleStatus.LoggedInUsersOnly:
                return true;

            //for connected paras only, return true if the user has a profile that shares a chat or para with the profile.    
            case VisibleStatus.ConnectedUsersOnly:
                return await DoesUserShareChatsWithProfile(loggedInUser, profile);

            //for hidden profiles, only the uploader can see the profile.
            case VisibleStatus.HiddenByUser:
            case VisibleStatus.HiddenByMod:
                return profile.AppUserId == loggedInUser?.Id;

            //for any unhandled cases, return false.
            default:
                return false;
        }
    }

    public async Task<bool> CheckUserProfileAuthorization_View(int profileId, AppUser? loggedInUser)
    {
        return await CheckUserProfileAuthorization_View(
            await _context.Profiles.FirstOrDefaultAsync(m => m.Id == profileId),
            loggedInUser);
    }


    public async Task<bool> CheckUserProfileAuthorization_Edit(int profileId, AppUser? loggedInUser)
    {
        return await CheckUserProfileAuthorization_Edit(
            await _context.Profiles.FirstOrDefaultAsync(m => m.Id == profileId),
            loggedInUser);
    }

    public async Task<bool> CheckUserProfileAuthorization_Edit(Profile profile, AppUser? loggedInUser)
    {
        
        //var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == id);

        //check that a login and a profile exists
        if (profile == null || loggedInUser == null)
        {
            return false;
        }

        //allow edits if the profile belongs to the logged-in user and the profile is not deleted
        
        return profile.AppUserId == loggedInUser.Id && profile.VisibleStatus != VisibleStatus.DeletedByUser && profile.VisibleStatus != VisibleStatus.DeletedBySiteMod;
    }

    public async Task<bool> CheckUserProfileAuthorization_View(Profile profile, PageModel pageModel) {

        return await CheckUserProfileAuthorization_View(profile, await _userManager.GetUserAsync(pageModel.User));
    }

    public async Task<bool> CheckUserProfileAuthorization_Edit(Profile profile, PageModel pageModel) {

        return await CheckUserProfileAuthorization_Edit(profile, await _userManager.GetUserAsync(pageModel.User));
    }

    private async Task<bool> DoesUserShareChatsWithProfile(AppUser user, Profile profile) {
        //TODO. for now, return true.
        return true;

    }

    private async Task<bool> IsProfileAccessible(int id, AppUser loggedInUser)
    {
        var profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == id);
        return profile != null && profile.AppUserId == loggedInUser?.Id;
    }
}