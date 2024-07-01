using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CharaPara.Data.Model
{
    public enum Language : byte { English }
    public enum Gender : byte
    {
        [Display(Name = "---")]
        NoneSelected = 0,
        [Display(Name = "Male")]
        Male = 1,
        [Display(Name = "Female")]
        Female = 2,
        [Display(Name = "Androgynous")]
        Androgynous = 3,
        [Display(Name = "Genderless")]
        Genderless = 4,
        [Display(Name = "Unknown / Other")]
        Other = 5
    }

    public enum ProfileOriginCategory
    {
        [Display(Name = "Unknown")] //??
        Unknown = 0,
        [Display(Name = "Original Character")] //OC
        Original = 1,
        [Display(Name = "Canon Character")] //CC
        Canon = 3,
        
    }

    public enum SensitiveStatus : byte {
        [Display(Name = "None")]
        None = 0,
        [Display(Name = "Sensitive")]
        Sensitive = 1,
        [Display(Name = "Mature")]
        Mature = 2,
        [Display(Name = "Sensitive & Mature")]
        SensitiveAndMature = 3 
    }

    public enum GalleryImageCreationType : byte
    {
        [Display(Name = "Drawing / Illustration")]
        Drawing = 0,
        [Display(Name = "Photograph")]
        Photo = 1,
        [Display(Name = "Photo-Edit / Photo-Manipulation")]
        Photomanipulation = 2,
        [Display(Name = "Template / Character Creator")]
        TemplateGenerated = 3,
        [Display(Name = "AI-Generated")]
        AIGenerated = 4,
        [Display(Name = "Other")]
        Other = 5
    }

    public enum ParaProfileRank : byte
    {
        Guest = 0,
        Member = 1,
        Helper = 2,
        Moderator = 3,
        Admin = 4
    }


    public enum AllowCommentsSetting : byte
    {
        DoNotAllow = 0,
        Allow = 1,
        AllowAndRequestCritique = 2
    }

    public enum InfractionType : byte
    {
        Warning = 0,
        Mute = 1,
        Ban = 2,
        MuteUndone = 101,
        BanUndone = 102,
    }

    public enum InfractionReason : byte
    {
        [Display(Name = "None")]
        None = 0,
        [Display(Name = "Spam")]
        Spam = 1,
        [Display(Name = "Offensive Language")]
        OffensiveLanguage = 3,
        [Display(Name = "Harrassment")]
        Harrassment = 4,
        [Display(Name = "Other")]
        Other = 255

    }



    public enum VisibleStatus : byte { Public = 0, LoggedInUsersOnly = 10, ConnectedUsersOnly = 20, HiddenByUser = 100, HiddenByMod = 110, DeletedByUser = 200, DeletedBySiteMod = 210 }
    public enum SearchableStatus : byte { Searchable, ConnectionsOnly }

    public enum PrivacyStatus : byte { Public = 0, Unlisted = 10, ConnectedUsersOnly = 20, Private = 100, HiddenBySiteMod = 200 }

    public enum ModerationStatus : byte { NeverChecked = 0, UpdatedSinceLastCheck = 1, CheckedByBot = 1, CheckedByModerator = 2 }

    public enum ProfileType : byte { Avatar = 0, Creator = 1, World = 2 }
}
