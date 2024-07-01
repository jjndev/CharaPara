using System.ComponentModel.DataAnnotations.Schema;

namespace CharaPara.Data.Model
{
    public class Record_AppUserInfraction
    {
        public int Id { get; set; }
        public InfractionType InfractionType { get; set; }

        public InfractionReason InfractionReason { get; set; }

        public string? PublicNotes { get; set; }
        public string? PrivateNotes { get; set; }

        public DateTimeOffset DateTime_InfractionApplied { get; set; }

        public DateTimeOffset DateTime_InfractionExpires { get; set; }

        public string? Reason { get; set; }


        //foreign keys
        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; } = null!;
        public string AppUserId { get; set; } = null!;

        [ForeignKey("AdministratorId")]
        public AppUser? Administrator { get; set; }
        public string? AdministratorId { get; set; }
    }
}
