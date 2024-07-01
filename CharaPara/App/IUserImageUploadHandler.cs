using CharaPara.Data.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Text;
using System.IO;
using System;
using static CharaPara.App.ICloudImageUploadService;
using static CharaPara.App.IUserImageUploadHandler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Drawing.Imaging;
using CharaPara.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace CharaPara.App
{
    public interface IUserImageUploadHandler
    {
        public Task<ImageUploadResult> UploadImageAsync(
            IFormFile formFile, 
            AppUser appUser, 
            long? relatedId, 
            ImageCategory uploadType, 
            string? fileName = null
        );
        public Task<ImageUploadResult> UploadImageAsync(
            Image image,
            AppUser appUser, 
            long? relatedId,  
            ImageCategory uploadType, 
            string? fileName = null
        );

        public class ImageUploadResult
        {
            public readonly ImageUploadResultCode ResultCode;

            public readonly string? UploadUrl;
            public readonly string? ResultMessage;

            public ImageUploadResult(ImageUploadResultCode resultCode, string? uploadUrl = null, string? resultMessage = null)
            {
                ResultCode = resultCode;
                UploadUrl = uploadUrl;
                ResultMessage = resultMessage;
            }

            public static implicit operator ImageUploadResultCode(ImageUploadResult result)
            {
                return result.ResultCode;
            }
        }


        public enum ImageUploadResultCode : byte
        {
            Success = 0,
            Success_MatureImageDetected = 10,

            NoFileGiven = 100,
            InvalidFileFormat = 110,
            InvalidFileFormat_UnsupportedByValidationService = 111,
            ImageFileSizeTooBig = 120,
            ImageDimensionsTooBig = 130,
            ImageDimensionsTooSmall = 131,
            ImageUploadFailed = 200,

            ExplicitImageDetected = 250
        }

        public enum ImageCategory : byte
        {
            TestUpload,
            GroupMessageAttachment, 
            DirectMessageAttachment, 
            ProfileBackground, 
            ProfileAvatar, 
            ProfileAltAvatar, 
            Emoticon, 
            GroupBanner, 
            GroupIcon, 
            ChannelBackground, 
            DirectMessageBackground,
            
        }
    }

    public class UserImageUploadHandler : IUserImageUploadHandler
    {
        private ICloudImageUploadService _cloudImageUploadService;
        private IValidateImageService _validateImageService;
        private IUserImagePathHandler _userImagePathHandler;
        private readonly IServiceProvider _serviceProvider;
        //private ApplicationDbContext _context;
        Random rng;

        public UserImageUploadHandler(
            ICloudImageUploadService cloudImageUploadService, 
            IValidateImageService validateImageService, 
            IUserImagePathHandler userImagePathHandler,
            IServiceProvider serviceProvider
            //ApplicationDbContext context
            )
        {
            //dependency injection
            _cloudImageUploadService = cloudImageUploadService;
            _validateImageService = validateImageService;
            _userImagePathHandler = userImagePathHandler;
            _serviceProvider = serviceProvider;
            //_context = context;
            rng = new Random();
        }

        public async Task<ImageUploadResult> UploadImageAsync(
            IFormFile formFile, 
            AppUser appUser, 
            long? relatedId, 
            ImageCategory imageCategory, 
            string? fileName = null)
        {
            //pre-validate the image

            var resultCode = await _validateImageService.PreValidateImageFormFileAsync(formFile, imageCategory);

            if (resultCode != ImageUploadResultCode.Success)
            {
                return new ImageUploadResult(resultCode);
            }

            ImageUploadResult result;

            //convert to image file to validate dimensions
            using (var img = (await Image.LoadAsync(formFile.OpenReadStream())))
            {
                result = await UploadImageAsync(img, appUser, relatedId, imageCategory, fileName);
            }

            return result;
        }

        public async Task<ImageUploadResult> UploadImageAsync(Image image, 
            AppUser appUser, 
            long? relatedId, 
            ImageCategory imageCategory, 
            string? fileName = null)
        {
            //validate the image

            var resultCode = await _validateImageService.ValidateImageAsync(image, imageCategory);

            if (resultCode != ImageUploadResultCode.Success)
            {
                return new ImageUploadResult(resultCode); ;
            }

            //determine the bucket and path to upload to

            //var bucket = _cloudImageUploadService.GetBucketName(uploadType);
            //var path = _cloudImageUploadService.GetFolderName(uploadType);


            var fileFormat = image.Metadata.DecodedImageFormat;
           

            if (fileName == null)
            {
                fileName = GenerateRandomFileName(image);
            }
            var key = _userImagePathHandler.GetImagePath(imageCategory, fileName, relatedId);

            //upload to cloud
            bool cloudUploadSuccess;
            using (var imageStream = new MemoryStream())
            {
                await image.SaveAsync(imageStream, fileFormat);
                cloudUploadSuccess = await _cloudImageUploadService.UploadImageAsync(imageStream, key);
            }

            if (cloudUploadSuccess == false)
            {
                return new ImageUploadResult(ImageUploadResultCode.ImageUploadFailed);
            }

            //store a record on the database
            var record = new Record_ImageUpload
            {
                filePath = key,
                AppUser = appUser,
                AppUserId = appUser.Id,
                DateTime = DateTimeOffset.Now,
            };

            //independently add a record to the database

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Task independentTask = Task.Run(() =>
                {
                    try
                    {
                        dbContext.Record_ImageUploads.AddAsync(record);
                    }
                    catch
                    {
                        Console.WriteLine("failed to add imageupload record to database");
                    }
                });
            }


            //finally, return success

            return new ImageUploadResult(ImageUploadResultCode.Success, key);
        }


        private string GetImageFileFormat(Image img)
        {
            var result = img.Metadata.DecodedImageFormat.FileExtensions.First();

            return result == "jpeg" ? "jpg" : result;
        }

        public string GenerateRandomFileName(Image img)
        {

            string urlSafeChars = "abcdefghijklmnopqrstuvwxyz0123456789";

            StringBuilder stringBuilder = new StringBuilder(12);

            for (int i = 0; i < 12; i++)
            {
                char c = urlSafeChars[rng.Next(urlSafeChars.Length)];
                stringBuilder.Append(c);
            }

            string randomString = stringBuilder.ToString();

            return $"{randomString}.{GetImageFileFormat(img)}";
        }
    }
}
