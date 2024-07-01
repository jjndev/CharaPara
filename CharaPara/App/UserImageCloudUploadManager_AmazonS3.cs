using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Plugins;
using static CharaPara.App.IUserImageCloudUploadManager;
using System.Drawing.Imaging;
using Amazon.Runtime.Internal.Transform;
using MySqlX.XDevAPI;
using System.Drawing.Printing;
using CharaPara.App.Utility;

namespace CharaPara.App
{

    public class UserImageCloudUploadManager_AmazonS3 : IUserImageCloudUploadManager
    {
        //private readonly AmazonS3Client _s3Client;
        private readonly string uploadPortalBucketName;
        private readonly string uploadAccessBucketName;
        private readonly string appBucketName;
        private readonly TimeSpan uploadUrlExpiryDuration;

        private readonly BasicAWSCredentials S3Credentials;
        private readonly AmazonS3Config S3Config;

        private readonly List<string> PermittedImageFormats;
        private readonly Dictionary<UploadImageType, int> MaxFileSize;
        private readonly Dictionary<UploadImageType, int> MaxFileDimensions;

        public UserImageCloudUploadManager_AmazonS3()
        {
            var appConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            Console.WriteLine("S3 Constructor called");

            //uploadBucketName = appConfiguration.GetSection("Str_A")["B"];


            var uploadUrlExpiryDurationString = appConfiguration.GetSection("Str_A")["UploadUrlTimeSpanInMinutes"];
            uploadUrlExpiryDuration = TimeSpan.FromMinutes(Convert.ToInt32(uploadUrlExpiryDurationString));

            //s3 client

            //var bucketName = appConfiguration.GetSection("Str_A")["B"];
            uploadPortalBucketName = appConfiguration.GetSection("ImageBuckets")["UserSubmitPortal"];
            uploadAccessBucketName = appConfiguration.GetSection("ImageBuckets")["UserSubmitAccess"];
            appBucketName = appConfiguration.GetSection("ImageBuckets")["UserImages"];

            BasicAWSCredentials credentials;
            Amazon.RegionEndpoint.GetBySystemName(appConfiguration.GetSection("Str_A")["E"]);

            string endPoint;
            try
            {
                S3Credentials = new Amazon.Runtime.BasicAWSCredentials(
                    appConfiguration.GetSection("Str_A")["A"],
                    appConfiguration.GetSection("Str_A")["S"]
                );
                endPoint = appConfiguration.GetSection("Str_A")["E"];

                S3Config = new AmazonS3Config()
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(endPoint),
                    Timeout = TimeSpan.FromSeconds(15)
                };
            }
            catch
            {
                Console.WriteLine("S3ImageUploadService - failed to load appsettings");
                //throw new Exception("S3ImageUploadService - failed to load appsettings");
            }
            


            //file dimensions and size caps

            MaxFileDimensions = new Dictionary<UploadImageType, int>();
            MaxFileSize = new Dictionary<UploadImageType, int>();

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
                    MaxFileSize.Add(enumValue, fileSize);
                }

                PermittedImageFormats = appConfiguration.GetSection("ImageValidation")["ImageFormats"]
                    .Split(',').ToList();
            }
            catch
            {
                throw new Exception("ValidateImageService - failed to load MaxFileDimensions from appsettings.json");
            }
            //if (PermittedImageFormats == null) throw new Exception("ValidateImageService - failed to load PermittedImageFormats from appsettings.json");

        }


        public async Task<string> GenerateUserUploadUrlAsync(string userId, UploadImageType imageType)
        {
            if (S3Config == null) return string.Empty;
            //todo: check if this user hasn't made too many requests?


            var objectKey = userId + "/" + "image";//Guid.NewGuid().ToString();


            MetadataCollection metadata = new MetadataCollection();
            metadata.Add("UploadType", imageType.ToString());

            var request = new GetPreSignedUrlRequest
            {
                BucketName = uploadPortalBucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
                Verb = HttpVerb.PUT,
                ContentType = "image/jpeg",
                Metadata =
                {

                }
            };
            //ContentType = "image/",
            request.Metadata.Add("UploadType", imageType.ToString());

            //request.Headers["Content-Type"] = "image/jpeg";
            //request.Headers["Content-Type"] = "image/png";
            //request.Headers["Content-Type"] = "image/gif";
            //
            request.Headers["Content-Length-range"] = "0,50000000";


            //request.Headers["Content-Type"] = "image/bmp";
            //request.Headers["Content-Type"] = "image/tiff";

            // Set a policy that restricts the size of the uploaded file (e.g., between 0 and 5 MB)




            string resultString;

            Console.WriteLine("Attempting to get presigned string");

            try
            {
                using (var s3 = new AmazonS3Client(S3Credentials, S3Config))
                {
                    resultString = await s3.GetPreSignedURLAsync(request);
                }
            }
            catch
            {
                resultString = "";
                throw new Exception("GenerateUserUploadUrlAsync - failed to get presigned url");
            }

            Console.WriteLine($"presigned string: {resultString}");

            return resultString;

        }





        public async Task<(bool validationSucceeded, string? imageExtensionWithPeriod)> ValidateUserUploadAsync(
            string userId, 
            UploadImageType expectedImageType
            )
        {
            if (S3Config == null) return (false, null);

            var isValid = false;
            string? imageExtension = null;

            var objectPath = userId + "/";

            //check if a S3 image with the above key was successfully uploaded
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = uploadAccessBucketName,
                    Key = objectPath + "image"
                };

                //attempt to get the file's metadata
                GetObjectMetadataResponse response;

                using (var s3 = new AmazonS3Client(S3Credentials, S3Config))
                {
                    response = await s3.GetObjectMetadataAsync(request);
                }

                //get the file's uploadtype from the metadata
                var uploadType = (UploadImageType)Enum.Parse(typeof(UploadImageType), response.Metadata["UploadType"]);

                //get the file's file type
                var fileType = response.Metadata["Content-Type"];

                if (
                       uploadType == expectedImageType //check that this is the expected upload type
                    && PermittedImageFormats.Contains(fileType)  //check if valid image format
                    && response.LastModified < DateTime.UtcNow.AddMinutes(-2) //check that this image was uploaded in the last 2 minutes
                    )
                {
                    isValid = true;
                }
            }
            catch (AmazonS3Exception e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // The object does not exist, return false
                    //return false;
                }
                else
                {
                    //TODO: handle error logging for other errors that could have occurred
                    //return false;
                }
            }

            return (isValid, imageExtension);
        }



        /// <summary>
        /// Moves an uploaded image from the submitted image bucket to the cdn bucket, then empties the submitted image bucket. 
        /// Returns the key of the image in the cdn bucket.
        /// Assumes the image has already been uploaded and validated.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="expectedImageType"></param>
        /// <returns></returns>
        public async Task<(string? imageKey, string? thumbnailKey)> ConfirmUserUploadAsync(string userId, long relevantId, string fileExtensionIncludingPeriod, UploadImageType uploadType)
        {
            if (S3Config == null) return (null, null);
            
            string? newImageBucketKey = null;
            string? newThumbnailBucketKey = null;

            var objectPath = userId + "/";

            //check if a S3 image with the above key was successfully uploaded
            try
            {
                //check if this upload type should have a thumbnail
                bool shouldHaveThumbnail;
                switch (uploadType)
                {
                    case UploadImageType.GalleryImage:
                        shouldHaveThumbnail = true;
                        break;
                    default:
                        shouldHaveThumbnail = false;
                        break;
                }

                //get the file extension from the image's file format; attach to a random string filename
                var newImageFileName = FileNameStringGenerator.Generate(20) + fileExtensionIncludingPeriod;

                newImageBucketKey = $"{(char)uploadType}/{relevantId}/{newImageFileName}";

                if (shouldHaveThumbnail)
                {
                    var newThumbnailFileName = FileNameStringGenerator.Generate(20) + fileExtensionIncludingPeriod;
                    newThumbnailBucketKey = $"{(char)uploadType}/{relevantId}/thumbnail/{newThumbnailFileName}";
                }

                //move the uploaded image to the destination bucket
                using (var s3 = new AmazonS3Client(S3Credentials, S3Config))
                {
                    //copy the uploaded image to the destination bucket
                    var result = await s3.CopyObjectAsync(new CopyObjectRequest
                    {
                        SourceBucket = uploadAccessBucketName,
                        SourceKey = objectPath + "image",
                        DestinationBucket = appBucketName,
                        DestinationKey = newImageBucketKey
                    });
                    //if the copy succeeded, delete the original

                    if (result != null) await s3.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = uploadAccessBucketName,
                        Key = objectPath + "image"
                    });


                    //copy the uploaded image's thumbnail to the destination bucket, if required
                    if (shouldHaveThumbnail)
                    {
                        result = await s3.CopyObjectAsync(new CopyObjectRequest
                        {
                            SourceBucket = uploadAccessBucketName,
                            SourceKey = objectPath + "thumb",
                            DestinationBucket = appBucketName,
                            DestinationKey = newThumbnailBucketKey
                        });
                        if (result != null) await s3.DeleteObjectAsync(new DeleteObjectRequest
                        {
                            BucketName = uploadAccessBucketName,
                            Key = objectPath + "thumb"
                        });

                    }
                }
            }
            catch (AmazonS3Exception e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // The object does not exist, return false
                    //return false;
                }
                else
                {
                    //TODO: handle error logging for other errors that could have occurred
                    return (null, null);
                }
            }

            return (newImageBucketKey, newThumbnailBucketKey);
        }


        private async Task DeleteAllUploadAttemptsFromGivenUserId(string bucket, string userId)
        {
            if (S3Config == null) return;
            // Delete all objects in the user's folder
            var folderKey = userId + "/";
            await DeleteObjectsInFolderAsync(bucket, folderKey);
        }

        private async Task DeleteObjectsInFolderAsync(string bucketName, string folderKey)
        {
            if (S3Config == null) return;
            // Ensure the folderKey ends with a '/' to denote a folder
            if (!folderKey.EndsWith("/"))
                folderKey += "/";

            // Initialize a list to hold all the keys of objects to be deleted
            List<KeyVersion> objectsToDelete = new List<KeyVersion>();

            // List objects and add them to the list for deletion
            ListObjectsV2Request listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = folderKey
            };


            using (var s3 = new AmazonS3Client(S3Credentials, S3Config))
            {
                ListObjectsV2Response listResponse;
                do
                {
                    // Get the list of objects
                    listResponse = await s3.ListObjectsV2Async(listRequest);

                    // Add the retrieved objects to the list
                    objectsToDelete.AddRange(listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key }));

                    // Set the continuation token for the next list request
                    listRequest.ContinuationToken = listResponse.NextContinuationToken;

                } while (listResponse.IsTruncated); // Check if there are more objects to list

                // Delete the objects
                if (objectsToDelete.Count > 0)
                {
                    // Split the deletion requests into batches if necessary (S3 API allows up to 1000 keys per batch)
                    int batchSize = 1000;
                    for (int i = 0; i < objectsToDelete.Count; i += batchSize)
                    {
                        var batch = objectsToDelete.Skip(i).Take(batchSize).ToList();

                        DeleteObjectsRequest deleteRequest = new DeleteObjectsRequest
                        {
                            BucketName = bucketName,
                            Objects = batch
                        };

                        // Perform the delete operation
                        await s3.DeleteObjectsAsync(deleteRequest);
                    }
                }
            }
        }
    }
}
