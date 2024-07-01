
namespace CharaPara.App.Extensions
{
    public static class CombineSplitByteArrayExtensions
    {

        /// <summary>
        /// Converts a byte array to a short array by combining every two bytes.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static short[] CombineToShortArray(this byte[] byteArray)
        {
            if (byteArray == null) return [];
            var outputArray = new short[byteArray.Length / 2];
            var l = byteArray.Length / 2;
            for (int i = 0; i < byteArray.Length / 2; i++)
                outputArray[i] = BitConverter.ToInt16(byteArray, i * 2);

            return outputArray;
        }

        /// <summary>
        /// Converts a short array to a byte array by splitting each short into two bytes.
        /// </summary>
        /// <param name="shortArray"></param>
        /// <returns></returns>
        public static byte[] SplitToByteArray(this short[] shortArray)
        {
            if (shortArray == null) return [];
            var outputData = new byte[shortArray.Length * 2];

            for (int i = 0; i < shortArray.Length; i++)
            {
                var BinaryValue = BitConverter.GetBytes(shortArray[i]);

                outputData[(i * 2)] = BinaryValue[0];
                outputData[(i * 2) + 1] = BinaryValue[1];
            }

            return outputData;
        }
    }
}
