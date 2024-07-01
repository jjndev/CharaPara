using System.Security.Cryptography;
using System.Text;

namespace CharaPara.App.Utility
{
    
    public static class FileNameStringGenerator
    {
        /// <summary>
        /// Generate random string with given length, filter out vulgar words
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Generate_FilterExplicit(int length)
        {
            string generatedString = Generate(length);

            // List of vulgar words
            List<string> vulgarWords = new List<string> { "vulgar_word1", "vulgar_word2", "vulgar_word3" };

            while (vulgarWords.Any(word => generatedString.Contains(word)))
            {
                generatedString = Generate(length);
            }

            return generatedString;
        }

        /// <summary>
        /// Generate random string with given length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Generate(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyz0123456789_-";
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                StringBuilder result = new StringBuilder(length);
                foreach (byte b in randomBytes)
                {
                    result.Append(validChars[b % (validChars.Length)]);
                }
                return result.ToString();
            }
        }
    }
}
