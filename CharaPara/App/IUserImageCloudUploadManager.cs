using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CharaPara.App.IUserImageCloudUploadManager;

namespace CharaPara.App
{
    public interface IUserImageCloudUploadManager
    {
        public enum UploadImageType : byte
        {
            TestUpload, GroupMessageAttachment, DirectMessageAttachment, ProfileIcon, ProfileAltIcon, ProfileBackground, GalleryImage, Emoticon, ChannelBackground, ParaBanner
        }

        public Task<string> GenerateUserUploadUrlAsync(string userId, UploadImageType imageType);

        /// <summary>
        /// Validates that the user has uploaded an image of the expected type. 
        /// returns a tuple of (bool validationSucceeded, string? imageExtensionWithPeriod).
        /// </summary>
        /// <param name="userId">The uploader's AppUser ID.</param>
        /// <param name="expectedImageType"></param>
        /// <returns></returns>
        public Task<(bool validationSucceeded, string? imageExtensionWithPeriod)> ValidateUserUploadAsync(string userId, UploadImageType expectedImageType);

        /// <summary>
        /// Moves an uploaded image from the submitted image bucket to the cdn bucket, then empties the submitted image bucket.
        /// returns a tuple of (string? imageKey, string? thumbnailImageKey). Will equal null if the function fails.
        /// </summary>
        /// <param name="userId">The uploader's AppUser ID.</param>
        /// <param name="_relevantId">The ID relevant to the type of image uploaded.</param>
        /// <param name="imageType">The type of image uploaded.</param>
        /// <returns></returns>
        public Task<(string imageKey, string thumbnailKey)> ConfirmUserUploadAsync(string userId, long _relevantId, string _fileExtensionIncludingPeriod, UploadImageType imageType);
    }


    public static class IUserImageCloudUploadManager_UploadImageTypeExtensions
    {
        public static UploadImageType FromChar(this char value)
        {
            switch (value)
            {
                case 't':
                    return UploadImageType.TestUpload;
                case 'm':
                    return UploadImageType.GroupMessageAttachment;
                case 'd':
                    return UploadImageType.DirectMessageAttachment;
                case 'i':
                    return UploadImageType.ProfileIcon;
                case 'a':
                    return UploadImageType.ProfileAltIcon;
                case 'b':
                    return UploadImageType.ProfileBackground;
                case 'c':
                    return UploadImageType.ChannelBackground;
                case 'g':
                    return UploadImageType.GalleryImage;
                case 'p':
                    return UploadImageType.ParaBanner;
                case 'e':
                    return UploadImageType.Emoticon;
                default:
                    throw new Exception("Invalid Image Upload Type");
            }
        }

        public static string ToChar(this UploadImageType value)
        {
            switch (value)
            {
                case UploadImageType.TestUpload:
                    return "t";
                case UploadImageType.GroupMessageAttachment:
                    return "m";
                case UploadImageType.DirectMessageAttachment:
                    return "d";
                case UploadImageType.ProfileIcon:
                    return "i";
                case UploadImageType.ProfileAltIcon:
                    return "a";
                case UploadImageType.ProfileBackground:
                    return "b";
                case UploadImageType.ChannelBackground:
                    return "c";
                case UploadImageType.GalleryImage:
                    return "g";
                case UploadImageType.ParaBanner:
                    return "p";
                case UploadImageType.Emoticon:
                    return "e";
                default: throw new Exception("Invalid Image Upload Type");
            }
        }
    }

    public class IUserImageCloudUploadManager_ValidateImageResult
    {
        public bool IsValid { get; private set; }
        public string? ImageExtension { get; private set; }

        public string? ImageKey { get; private set; }
        public string? ThumbnailKey { get; private set; }

        public IUserImageCloudUploadManager_ValidateImageResult(bool isValid, string? imageExtension = null, string? imageKey = null, string? thumbnailKey = null)
        {
            IsValid = isValid;
            ImageExtension = imageExtension;
            ImageKey = imageKey;
            ThumbnailKey = thumbnailKey;
        }
    }




}
