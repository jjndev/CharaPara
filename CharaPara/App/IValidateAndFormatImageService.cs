using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Plugins;
using static CharaPara.App.IValidateAndFormatImageService;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Gif;
using Amazon.Runtime.Internal.Transform;

namespace CharaPara.App
{
    public interface IValidateAndFormatImageService
    {
        public Task<ValidateAndFormatImageResult> ValidateAndFormatImage(IFormFile formFile, UploadImageType uploadType, bool checkForMatureImages = false);
        public enum ValidateImageResultCode : byte
        {
            Success = 0,
            Success_MatureImageDetected = 10,

            NoFileGiven = 100,
            InvalidFileFormat = 120,
            InvalidFileFormat_UnsupportedByValidationService = 121,
            ImageFileSizeTooBig = 130,
            ImageDimensionsTooBig = 140,
            ImageDimensionsTooSmall = 141,

            ExplicitImageDetected = 200
        }

        public enum UploadImageType : byte
        {
            Avatar, Emoji, Gallery, Message
        }

        public class ValidateAndFormatImageResult : IAsyncDisposable
        {
            public ValidateImageResultCode Code { get; internal set; }
            public bool explicitImageChecked { get; internal set; }
            public Stream? Stream { get; protected set; }

            public string? fileFormat { get; set; }

            public ValidateAndFormatImageResult(ValidateImageResultCode code, UploadImageType uploadType = UploadImageType.Gallery, bool? explicitImageChecked = null)
            {
                this.Code = code;
                this.explicitImageChecked = explicitImageChecked ?? false;
                this.fileFormat = fileFormat;
            }

            public bool IsSuccess { get => (byte)this.Code <= 100; }


            public async ValueTask DisposeAsync()
            {
                if (Stream != null)
                {
                    await Stream.DisposeAsync();
                    Stream = null;
                }
            }

        }
    }

    public class ValidateAndFormatImageService : IValidateAndFormatImageService
    {

        private readonly List<string> PermittedImageFormats;
        private readonly Dictionary<UploadImageType, int> MaxFileSize;
        private readonly Dictionary<UploadImageType, int> MaxFileDimensions;

        ValidateAndFormatImageService() {
            var appConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();


            MaxFileDimensions = new Dictionary<UploadImageType, int>();

                try
                {


                    foreach (var value in Enum.GetValues(typeof(UploadImageType)))
                    {
                        var enumValue = (UploadImageType)value;

                        var dimensions = Convert.ToInt32(
                            appConfiguration.GetSection("ImageValidation")
                            [$"{enumValue}ImageDimensions"]);

                        var fileSize = Convert.ToInt32(
                            appConfiguration.GetSection("ImageValidation")
                            [$"{enumValue}ImageFileSize"]);

                        MaxFileDimensions.Add(enumValue, dimensions);
                        MaxFileDimensions.Add(enumValue, fileSize);
                    }

                    PermittedImageFormats = appConfiguration.GetSection("ImageValidation")["ImageFormats"]
                        .Split(',').ToList();
                }
                catch { 
                    throw new Exception("ValidateImageService - failed to load MaxFileDimensions from appsettings.json"); 
                }
            //if (PermittedImageFormats == null) throw new Exception("ValidateImageService - failed to load PermittedImageFormats from appsettings.json");

        }

        public async Task<ValidateAndFormatImageResult> ValidateAndFormatImage(IFormFile formFile, UploadImageType uploadType, bool checkForMatureImages = false)
        {
            if (formFile == null) {
                //failed validation, no file given
                return new ValidateAndFormatImageResult(ValidateImageResultCode.NoFileGiven);
            }

            //validate filesize
            if (formFile.Length > MaxFileDimensions[uploadType]) {
                //failed validation, filesize too big
                return new ValidateAndFormatImageResult(ValidateImageResultCode.ImageFileSizeTooBig);
            }

            //validate file format
            var fileFormat = formFile.ContentType.ToLowerInvariant();
            if (!PermittedImageFormats.Contains(fileFormat)  ) {
                //failed validation, invalid file format
                return new ValidateAndFormatImageResult(ValidateImageResultCode.InvalidFileFormat);
            }


            //convert to image to validate dimensions

            var result = new ValidateAndFormatImageResult(ValidateImageResultCode.Success, uploadType);


            using (var img = (await Image.LoadAsync(formFile.OpenReadStream())))
            {
                //validate size
                //filter images that are too small to be practical
                if (img.Width < 5 || img.Width < 5) {
                    //failed validation, dimensions too small
                    return new ValidateAndFormatImageResult(ValidateImageResultCode.ImageDimensionsTooSmall);
                }

                //resize
                var maxDimensions = MaxFileDimensions[uploadType];

                if (img.Width > maxDimensions || img.Height > maxDimensions)
                {
                    //don't resize animated gifs; just deny them if they're too big
                    if (fileFormat == "gif" && img.Frames.Count > 1)
                    {
                        //failed validation, dimensions too big
                        await result.DisposeAsync();
                        return new ValidateAndFormatImageResult(ValidateImageResultCode.InvalidFileFormat);
                    }

                    //resize the image
                    img.Mutate(x =>
                        x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(maxDimensions, maxDimensions)
                        }));
                }


                //success; save the image and fileformat in the result object
                result.fileFormat = fileFormat;

                switch (fileFormat) {
                    case "png":
                    case "bmp":
                        await img.SaveAsPngAsync(result.Stream);
                        break;
                    case "gif":
                        await img.SaveAsGifAsync(result.Stream);
                        break;
                    case "jpg":
                    case "jpeg":
                        fileFormat = "jpg";
                        await img.SaveAsJpegAsync(result.Stream);
                        break;
                    //if the format wasn't recognised, flag as invalid
                    default:
                            await result.DisposeAsync();
                            return new ValidateAndFormatImageResult(ValidateImageResultCode.InvalidFileFormat_UnsupportedByValidationService);
                }
            }

            return result;
        }
    }
}
