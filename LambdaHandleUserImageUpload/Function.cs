using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaHandleUserImageUpload;

public class Function
{
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client">The service client to access Amazon S3.</param>
    public Function(IAmazonS3 s3Client)
    {
        this.S3Client = s3Client;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();

        var destinationBucket = Environment.GetEnvironmentVariable("DESTINATION_BUCKET_NAME");
        if (destinationBucket == null) {
            context.Logger.LogError("No destination bucket specified");
            return;
        }
        var customHeaderName = Environment.GetEnvironmentVariable("OBJECT_METADATA_HEADER_IMAGE_UPLOAD_TYPE");
        //var avatarIconDimensions = Convert.ToInt32(Environment.GetEnvironmentVariable("AVATAR_ICON_DIMENSIONS"));
        //var galleryFullImageMaxDimensions = Convert.ToInt32(Environment.GetEnvironmentVariable("GALLERY_FULLIMAGE_MAX_DIMENSIONS"));
        //var galleryThumbnailMaxDimensions = Convert.ToInt32(Environment.GetEnvironmentVariable("GALLERY_THUMBNAIL_MAX_DIMENSIONS"));
        var imageMaxDimensions = Convert.ToInt32(Environment.GetEnvironmentVariable("IMAGE_DIMENSIONS_DEFAULT"));


        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            
            if (s3Event == null) continue;
            
            

            try
            {
                var sourceBucket = s3Event.Bucket.Name;
                var objectKey = s3Event.Object.Key;
                var metadataResponse = await S3Client.GetObjectMetadataAsync(sourceBucket, objectKey);
                var contentType = metadataResponse.Headers.ContentType;
                
                //get the uploader ID from the object key
                var userIdSplit = objectKey.Split("/");
                var userId = userIdSplit[userIdSplit.Length - 2];

                if (userId == null) {
                    context.Logger.LogLine($"Failed to get user ID from object key: {objectKey}");
                    continue;
                }

                // Check if the object is an image
                if (!contentType.StartsWith("image/")) {
                    context.Logger.LogLine($"Object '{objectKey}' is not an image");
                    continue;
                }

                //next, check the object's custom header to identify the use case for the image
                string headerKey = $"x-amz-meta-{customHeaderName}";

                // Retrieve the custom header value
                if (!metadataResponse.Metadata.Keys.Contains(headerKey)) return;
                
                var imageUploadType = metadataResponse.Metadata[headerKey];

                var targetImageDimensionsString = Environment.GetEnvironmentVariable($"IMAGE_DIMENSIONS_{imageUploadType.ToUpperInvariant()}");
                var targetImageDimensions = targetImageDimensionsString == null ? imageMaxDimensions : Convert.ToInt32(targetImageDimensionsString);

                var targetImageNeedsExactDimensionsString = Environment.GetEnvironmentVariable($"IMAGE_EXACT_DIMENSIONS_{imageUploadType.ToUpperInvariant()}");
                var targetImageNeedsExactDimensions = targetImageNeedsExactDimensionsString == "1" ? true : false;

                var targetImageThumbnailDimensionsString = Environment.GetEnvironmentVariable($"THUMBNAIL_DIMENSIONS_{imageUploadType.ToUpperInvariant()}");
                var targetImageThumbnailDimensions = targetImageThumbnailDimensionsString == null ? 0 : Convert.ToInt32(targetImageThumbnailDimensionsString);

                await using var objectStream = await S3Client.GetObjectStreamAsync(sourceBucket, objectKey, new Dictionary<string, object>());
                using var outImageStream = new MemoryStream();
                using var outThumbnailStream = new MemoryStream();
                using (var image = await Image.LoadAsync(objectStream))
                {
                    var originalImageType = image.Metadata.DecodedImageFormat;
                    //if this file can't be decoded as an image, it can't be resized
                    if (originalImageType == null) continue;

                    var originalImageDimensions = Math.Max(image.Width, image.Height);
                    
                    //check if the image needs to be resized
                    if (targetImageNeedsExactDimensions || originalImageDimensions > targetImageDimensions)
                    {
                        var resizeMode = targetImageNeedsExactDimensions ? ResizeMode.Crop : ResizeMode.Max;

                        image.Mutate(x =>
                            x.Resize(new ResizeOptions()
                            {
                                Size = new Size(targetImageDimensions, targetImageDimensions),
                                Mode = ResizeMode.Crop,
                                Position = AnchorPositionMode.Center
                            }));

                        //save the image, keeping its original format
                        await image.SaveAsync(outImageStream, originalImageType);
                    }

                    //also convert and save as a thumbnail if needed
                    if (targetImageThumbnailDimensions > 0)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions()
                        {
                            Size = new Size(targetImageThumbnailDimensions, targetImageThumbnailDimensions),
                            Mode = ResizeMode.Crop,
                            Position = AnchorPositionMode.Center
                        }));
                        await image.SaveAsync(outThumbnailStream, originalImageType);
                    }
                }

                //if outImageStream has data, upload it to the destination bucket. if not, copy the object to the destination bucket
                var destinationKey = $"{userId}/{objectKey}";

                if (outImageStream.Length > 0)
                {
                    var putObjectRequest = new PutObjectRequest()
                    {
                        BucketName = destinationBucket,
                        InputStream = outImageStream,
                        AutoCloseStream = true,
                        Key = $"{userId}/image"
                    };

                    putObjectRequest.Metadata.Add($"x-amz-meta-{customHeaderName}", imageUploadType);

                    await S3Client.PutObjectAsync(putObjectRequest);

                }
                else
                {
                    await CopyS3ObjectAsync(sourceBucket, $"{userId}/image", destinationBucket);
                }

                //if outThumbnailStream has data, upload it to the destination bucket.
                if (outThumbnailStream.Length > 0)
                {
                    var putObjectRequest = new PutObjectRequest()
                    {
                        BucketName = destinationBucket,
                        InputStream = outThumbnailStream,
                        AutoCloseStream = true,
                        Key = $"{userId}/thumbnail"
                    };

                    putObjectRequest.Metadata.Add($"x-amz-meta-{customHeaderName}", imageUploadType);

                    await S3Client.PutObjectAsync(putObjectRequest);
                }

                //now that the object(s) is in the destination bucket, we can delete it from the source bucket
                await S3Client.DeleteObjectAsync(sourceBucket, objectKey);


            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogError(e.Message);
                context.Logger.LogError(e.StackTrace);
                throw;
            }
        }


    }

    private async Task CopyS3ObjectAsync(string sourceBucket, string sourceKey, string destinationBucket)
    {
        var copyRequest = new CopyObjectRequest
        {
            SourceBucket = sourceBucket,
            SourceKey = sourceKey,
            DestinationBucket = destinationBucket,
            DestinationKey = sourceKey
        };
        await S3Client.CopyObjectAsync(copyRequest);
    }
}