using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CharaPara.Data.Model
{
    public class GalleryImage
    {
        


        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(100)]
        public string? ImagePath { get; set; }

        [MaxLength(100)]
        public string? ThumbnailPath { get; set; }

        [MaxLength(100)]
        public string? SourceCredit { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public GalleryImageCreationType CreationType { get; set; }




        //public TabTemplate? TabTemplate { get; set; }

        //public string? TemplateString { get; set; }

        //public int? TemplateVersion { get; set; }

        public Profile Profile { get; set; } = null!;

        public int ProfileId { get; set; }

        public byte Order { get; set; }

        [DefaultValue(SensitiveStatus.None)]
        public SensitiveStatus SensitiveStatus { get; set; }

        [DefaultValue(VisibleStatus.Public)]
        public VisibleStatus VisibleStatus {get; set;} = VisibleStatus.Public;

        public DateTimeOffset? DateTimeCreated { get; set; } = DateTime.UtcNow;

        public DateTimeOffset? DateTimeModified { get; set; } = DateTime.UtcNow;

        [DefaultValue(null)]
        public DateTimeOffset? DateTimeDeleted { get; set; } = null;

    }
}
