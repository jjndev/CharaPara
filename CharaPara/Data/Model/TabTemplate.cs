using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CharaPara.Data.Model
{
    public class TabTemplate
    {
        public enum TemplateStatus : byte
        {
            Normal = 0, Default = 1, Official = 2, Recommended = 3
        }

        [Key]
        public int Id { get; set; }

        public Profile Profile { get; set; } = null!;

        public int ProfileId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required, DefaultValue(Language.English)]
        public Language Language { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        [Required, MaxLength(300)]
        public string TemplateString { get; set; } = "";

        [Required, DefaultValue(0)]
        public int TemplateVersion { get; set; } = 1;

        [Required, DefaultValue(0)]
        public TemplateStatus Status { get; set; } = TemplateStatus.Normal;

        public DateTimeOffset DateTimeCreated { get; set; } = DateTime.UtcNow;
        public DateTimeOffset DateTimeModified { get; set; } = DateTime.UtcNow;
    }
}
