using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using CharaPara.App.Extensions;
using Google.Protobuf;

namespace CharaPara.Data.Model
{
    
    public class Profile
    {
        [Key]
        public int Id { get; set; }


        [Required, DefaultValue(ProfileType.Avatar)]
        public ProfileType ProfileType { get; set; }


        [Required, DefaultValue(SensitiveStatus.None)]
        public SensitiveStatus SensitiveStatus { get; set; }

        [Required, DefaultValue(Gender.NoneSelected)]
        public Gender Gender { get; set; }

        [Required, DefaultValue("#222222")]
        public ProfileOriginCategory OriginCategory { get; set; } = ProfileOriginCategory.Unknown;

        [Required, Length(1, 30)]
        public string Name { get; set; }

        [DefaultValue(""), MaxLength(50)]
        public string? Title { get; set; }

        [DefaultValue(""), MaxLength(500)]
        public string? Summary { get; set; }

        [Required, DefaultValue(VisibleStatus.Public)]
        public VisibleStatus VisibleStatus { get; set; }

        [DefaultValue(0)]
        public ModerationStatus ModerationStatus { get; set; }

        [Required, DefaultValue("#DDDDDD")]
        public string TextColor { get; set; }

        [Required, DefaultValue("#777777")]
        public string ProfileColor { get; set; }

        [Required, DefaultValue("#222222")]
        public string BorderColor { get; set; }

        //public DateTimeOffset DateTimeCreated
        //{
        //    get { return this.dateTimeCreated.HasValue? this.dateTimeCreated.Value: DateTime.Now; }
        //    set { this.dateTimeCreated = value; }
        //}
        //private DateTimeOffset dateTimeCreated;
        //
        //[SqlDefaultValue(DefaultValue = "getutcdate()")]
        //[Sql(DefaultValue = "getutcdate()")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? DateTimeCreated { get; set; } 
        public DateTimeOffset? DateTimeModified { get; set;}

        public DateTimeOffset? DateTimeAvatarModified { get; set; }

        public DateTimeOffset? DateTimeLastSeen { get; set; }

        public DateTimeOffset? DateTimeLastSeenPublic { get; set; }
        public DateTimeOffset? DateTimeDeleted { get; set;}


        [Required]
        public string AppUserId { get; set; }
        [Required]
        public AppUser AppUser { get; set; }





        public ICollection<Tab> Tabs { get; set; } = [];

        public ICollection<GalleryImage> GalleryImages { get; set; } = [];

        //public ICollection<TabTemplate> TabTemplates { get; set; }

        [DefaultValue(""), MaxLength(500)]
        public string? SearchTagString { get; set; }


        [DefaultValue(""), MaxLength(500)]
        public string? AdvertPost { get; set; }


        public string? ImageFileName_Avatar { get; set; }
        public string? ImageFileName_Background { get; set; }


        //public byte[]? SearchTagIdsData;
        //
        //[NotMapped]
        //public short[] SearchTagIds
        //{
        //    get => SearchTagIdsData.CombineToShortArray() ?? new short[0];
        //    set => SearchTagIdsData = value.SplitToByteArray();
        //}




        [NotMapped]
        public bool IsMature
        {
            get => this.SensitiveStatus == SensitiveStatus.Mature || this.SensitiveStatus == SensitiveStatus.SensitiveAndMature;
            set => this.SensitiveStatus = (SensitiveStatus)(Convert.ToByte(IsSensitive) + Convert.ToByte(value) * 2);
        }
        [NotMapped]
        public bool IsSensitive
        {
            get => this.SensitiveStatus == SensitiveStatus.Sensitive || this.SensitiveStatus == SensitiveStatus.SensitiveAndMature;
            set => this.SensitiveStatus = (SensitiveStatus)(Convert.ToByte(value) + Convert.ToByte(IsMature) * 2);
        }

        [NotMapped]
        public string[] SearchTags { 
            get => SearchTagString != null ? SearchTagString.Split(';').Select(p => p.Trim()).ToArray() : new string[0];
            set => SearchTagString = string.Join(";", value); 
        }


        public Profile()
        {
            Name = "<NewProfile_Error>";
            TextColor = "DDDDDD";
            ProfileColor = "777777";
            BorderColor = "222222";
            Summary = "";
            SearchTagString = "";
        }



        public string? GetAvatarFilePath()
        {
            return (ImageFileName_Avatar == null) ? null : $"/a/{Id}.{ImageFileName_Avatar}";
        }

        public string? GetBackgroundFilePath()
        {
            return (ImageFileName_Background == null) ? null : $"/p/{Id}.{ImageFileName_Background}";
        }
    }


    
    

    public class VM_Profile_EditCharacterAvatar
    {
        public int Id { get; set; }

        public FormFile Avatar { get; set; } = null!;

        public VM_Profile_EditCharacterAvatar(int id)
        {
            Id = id;
        }
    }
}
