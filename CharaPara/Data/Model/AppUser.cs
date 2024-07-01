using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CharaPara.Data.Model
{
    public class AppUser : IdentityUser
    {
        public ICollection<Profile> Profiles { get; }

        public ICollection<Record_AppUserInfraction> Record_AppUserInfractions { get; set; }


        [DefaultValue(5)]
        [Required]
        public short ProfileSlots { get; set; }

        [Required]
        [DefaultValue(0)]
        public long BirthDateUnixTime { get; set; }

        [NotMapped]
        public DateTimeOffset BirthDate { 
            get => DateTimeOffset.FromUnixTimeSeconds(BirthDateUnixTime); 
            set => BirthDateUnixTime = value.ToUnixTimeSeconds();
        }

        [NotMapped]
        public bool IsBanned { get => DateTimeBannedUntil != null && DateTimeBannedUntil > DateTimeOffset.Now; }
        [NotMapped]
        public bool IsLimited { get => DateTimeLimitedUntil != null && DateTimeLimitedUntil > DateTimeOffset.Now; }
        [NotMapped]
        public bool IsBannedOrLimited { get => IsBanned || IsLimited; }


        public bool IsMatureUser { get; set; }

        [DefaultValue("2000-01-01 00:00:00")]
        public DateTimeOffset? DateTimeCreated { get; set; }
        public DateTimeOffset? DateTimeLastActive { get; set; }

        public DateTimeOffset? DateTimeLimitedUntil { get; set; }
        public DateTimeOffset? DateTimeBannedUntil { get; set; }
    }

    public class AppUserSummaryAndProfilesDto
    {
        public string Id { get; set; } 
        public string Name { get; set; }
        public ICollection<Profile> Profiles { get; set; }
    }




}
