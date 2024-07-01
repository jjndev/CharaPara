using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CharaPara.Data.Model
{
    public class Tab
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(20000)]
        public string RawContent { get; set; }

        [MaxLength(65000)]
        public string GeneratedHtmlContent { get; set; }

        //public TabTemplate? TabTemplate { get; set; }

        //public string? TemplateString { get; set; }

        //public int? TemplateVersion { get; set; }

        public Profile Profile { get; set; } = null!;

        public int ProfileId { get; set; }

        public byte Order { get; set; }

        [DefaultValue(VisibleStatus.Public)]
        public VisibleStatus VisibleStatus {get; set;} = VisibleStatus.Public;

        public DateTimeOffset? DateTimeCreated { get; set; } = DateTime.UtcNow;

        public DateTimeOffset? DateTimeModified { get; set; } = DateTime.UtcNow;

        [DefaultValue(null)]
        public DateTimeOffset? DateTimeDeleted { get; set; } = null;

    }
}
