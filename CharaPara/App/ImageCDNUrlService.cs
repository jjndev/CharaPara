using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CharaPara.App
{
    public class ImageCDNUrlService
    {
        public string CDNUrl { get; }

        public ImageCDNUrlService()
        {
            //get the image cdn url from the appsettings.json
            var appConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            CDNUrl = appConfiguration.GetSection("Str_A")["ImageCDNUrl"];
        }

        /// <summary>
        /// Converts a storage bucket key to an Image CDN URL.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string KeyToUrl(string key)
        {
            return $"{CDNUrl}/{key}";
        }
    }
}
