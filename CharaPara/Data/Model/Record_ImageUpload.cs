using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CharaPara.Data.Model
{
    public class Record_ImageUpload
    {
        [Key]
        public long Id { get; set; }
        
        public DateTimeOffset DateTime { get; set; }

        public string filePath { get; set; }

        public AppUser AppUser { get; set; }

        public string AppUserId { get; set; }
    }
}
