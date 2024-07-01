using System.Text;
using static CharaPara.App.ICloudImageUploadService;
using static CharaPara.App.IUserImageUploadHandler;

namespace CharaPara.App
{
    public interface IUserImagePathHandler
    {
        public string GetImagePath(ImageCategory imageCategory, string? fileNameWithExtension = null, long? relatedId = null);
    }

    public class UserImagePathHandler : IUserImagePathHandler
    {

        public UserImagePathHandler()
        {

            //load app settings
            //var appConfiguration = new ConfigurationBuilder()
            //    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            //    .AddJsonFile("appsettings.json")
            //    .Build();

            ////image buckets
            //imageBuckets = new Dictionary<ImageBucket, string>();
            //var imageBucketFromConfig = appConfiguration.GetSection("ImageBuckets");
            //
            //foreach (var item in imageBucketFromConfig.GetChildren())
            //{
            //    if (item.Value == null) throw new Exception("Image Bucket Name Config has missing value.");
            //
            //    imageBuckets.Add((ImageBucket)Enum.Parse(typeof(ImageBucket), item.Key), item.Value);
            //}
            //
            ////image folders
            //imagePaths = new Dictionary<ImagePath, string>();
            //var imagePathsFromConfig = appConfiguration.GetSection("ImagePaths");
            //
            //foreach (var item in imagePathsFromConfig.GetChildren())
            //{
            //    if (item.Value == null) throw new Exception("Image Folder Name Config has missing value.");
            //
            //    imagePaths.Add((ImagePath)Enum.Parse(typeof(ImagePath), item.Key), item.Value);
            //    Console.WriteLine($"Item added: {item}");
            //}
        }

        public string GetImagePath(ImageCategory imageCategory, string? fileNameWithExtension = null, long? relatedId = null)

        {
            switch (imageCategory)
            {
                case ImageCategory.GroupMessageAttachment:
                    return $"/m/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.DirectMessageAttachment:
                    return $"/d/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.ProfileBackground:
                    return $"/p/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.ProfileAvatar:
                case ImageCategory.ProfileAltAvatar:
                    return $"/a/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.GroupIcon:
                    return $"/i/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.GroupBanner:
                    return $"/b/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.ChannelBackground:
                    return $"/c/{relatedId}/{fileNameWithExtension}";
                case ImageCategory.TestUpload:
                    return $"/test/{fileNameWithExtension}";
                default:
                    throw new Exception("Invalid Image Upload Type");
            };
        }



    }
}
