using Amazon.S3.Transfer;
using Amazon.S3;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata;
using System.Drawing.Printing;
using static CharaPara.App.ICloudImageUploadService;
using Amazon.Runtime;

namespace CharaPara.App
{
    public interface ICloudImageUploadService
    {
        public Task<bool> UploadImageAsync(IFormFile imageFile, string key);
        public Task<bool> UploadImageAsync(Stream imageStream, string key);

        //public enum ImageBucket : byte { Base, UserSubmitted }
        //public enum ImagePath : byte { GroupMessage, DirectMessage, Profile, Gallery, Site }

    }


    public class AmazonS3ImageUploadService : ICloudImageUploadService
    {
        //private Dictionary<ICloudImageUploadService.ImageBucket, string> imageBuckets;
        //private Dictionary<ICloudImageUploadService.ImagePath, string> imagePaths;
        //private readonly string keyName;
        //private Dictionary<string, string> folder;


        private readonly string bucketName;
        private readonly AmazonS3Client S3Client;

        public AmazonS3ImageUploadService()
        {
            Console.WriteLine("S3 Constructor called");

            var appConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            
            try
            {
                bucketName = appConfiguration.GetSection("Str_A")["B"]!;

                BasicAWSCredentials credentials;
                Amazon.RegionEndpoint.GetBySystemName(bucketName);

                string endPoint = appConfiguration.GetSection("Str_A")["E"]!;

                credentials = new Amazon.Runtime.BasicAWSCredentials(
                    appConfiguration.GetSection("Str_A")["A"],
                    appConfiguration.GetSection("Str_A")["S"]
                );

                var amazonConfig = new AmazonS3Config()
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(endPoint),
                    Timeout = TimeSpan.FromSeconds(30)
                };

                S3Client = new AmazonS3Client(credentials, amazonConfig);
            }
            catch
            {
                Console.WriteLine("S3ImageUploadService - failed to load appsettings");
                //throw new Exception("S3ImageUploadService - failed to load appsettings");
            }

            

        }



        public async Task<bool> UploadImageAsync(Stream imageFile, string key)
        {
            if (S3Client == null) return false;


            var result = false;
            try
            {
                using (var fileTransferUtility = new TransferUtility(S3Client))
                {
                    //upload the file
                    await fileTransferUtility.UploadAsync(imageFile, bucketName, key);
                    result = true;
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = false;
            }
            return result;
        }

        public async Task<bool> UploadImageAsync(
            IFormFile imageFile, string key)
        {
            //validate the formfile
            if (imageFile.Length == 0)
            {
                throw new System.Exception("S3 UploadImageAsync - The file is empty");
            }

            //upload the file
            using (var fileToUpload = imageFile.OpenReadStream())
            {
                return await UploadImageAsync(fileToUpload, key);
            }

        }

    }
}

