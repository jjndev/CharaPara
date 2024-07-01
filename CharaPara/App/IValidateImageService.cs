using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Plugins;
using static CharaPara.App.IUserImageUploadHandler;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Gif;

namespace CharaPara.App
{
    public interface IValidateImageService
    {
        public Task<ImageUploadResultCode> PreValidateImageFormFileAsync(IFormFile formFile, ImageCategory uploadType);

        public Task<ImageUploadResultCode> ValidateImageAsync(Image image, ImageCategory uploadType, bool resizeIfTooBig = true);


    }

    public class ValidateImageService : IValidateImageService
    {

        private readonly List<string> PermittedImageFormats;
        private readonly Dictionary<ImageCategory, int> MaxFileDimensions;
        private readonly Dictionary<ImageCategory, int> MaxFileSize;

        public ValidateImageService()
        {
            var appConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();


            MaxFileDimensions = new Dictionary<ImageCategory, int>();
            MaxFileSize = new Dictionary<ImageCategory, int>();

            try
            {
                foreach (var value in Enum.GetValues(typeof(ImageCategory)))
                {
                    var enumValue = (ImageCategory)value;

                    var dimensions = Convert.ToInt32(
                        appConfiguration.GetSection("ImageValidation")
                        [$"{enumValue}_Dimensions"]);

                    var fileSize = Convert.ToInt32(
                        appConfiguration.GetSection("ImageValidation")
                        [$"{enumValue}_FileSize"]);

                    MaxFileDimensions.Add(enumValue, dimensions);
                    MaxFileSize.Add(enumValue, fileSize);
                }

                PermittedImageFormats = appConfiguration.GetSection("ImageValidation")["ImageFormats"]
                    .Split(',').ToList();
            }
            catch
            {
                throw new Exception("ValidateImageService - failed to load file dimensions and size settings from appsettings.json");
            }
        }

        public async Task<ImageUploadResultCode> PreValidateImageFormFileAsync(IFormFile formFile, ImageCategory uploadType)
        {
            if (formFile == null)
            {
                //failed validation, no file given
                return ImageUploadResultCode.NoFileGiven;
            }

            //validate filesize
            if (formFile.Length > MaxFileSize[uploadType])
            {
                //failed validation, filesize too big
                return ImageUploadResultCode.ImageFileSizeTooBig;
            }

            //validate file format
            var fileFormat = formFile.ContentType.ToLowerInvariant().Split('/').Last();
            if (!PermittedImageFormats.Contains(fileFormat))
            {
                //failed validation, invalid file format
                return ImageUploadResultCode.InvalidFileFormat;
            }

            //all pre-validation checks passed
            return ImageUploadResultCode.Success;
        }

        public async Task<ImageUploadResultCode> ValidateImageAsync(Image image, ImageCategory uploadType, bool resizeIfTooBig = true)
        {

            var imageFileFormat = image.Metadata.DecodedImageFormat;

            if (imageFileFormat == null)
            {
                //failed validation, invalid file format
                return ImageUploadResultCode.InvalidFileFormat_UnsupportedByValidationService;
            }
            var imageExtension = imageFileFormat.FileExtensions.First();

            //validate size
            //filter images that are too small to be practical
            if (image.Width < 5 || image.Width < 5)
            {
                //failed validation, dimensions too small
                return ImageUploadResultCode.ImageDimensionsTooSmall;
            }

            //resize
            var maxDimensions = MaxFileDimensions[uploadType];

            if (image.Width > maxDimensions || image.Height > maxDimensions)
            {
                //don't resize animated gifs; just deny them if they're too big
                if (imageExtension == "gif" && image.Frames.Count > 1)
                {
                    //failed validation, dimensions too big
                    return ImageUploadResultCode.ImageDimensionsTooBig;
                }

                //resize the image
                image.Mutate(x =>
                    x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxDimensions, maxDimensions)
                    }));
            }

            return ImageUploadResultCode.Success;
        }
    }
}
