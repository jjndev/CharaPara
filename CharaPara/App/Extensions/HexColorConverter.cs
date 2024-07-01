using System.Globalization;
namespace CharaPara.App.Extensions
   
{

    public class HexColorConverter
    {
        public static string ClampAndConvertHexToHSV(string hex, int minV = 50, int maxV = 205)
        {
            var rgb = HexToRGB(hex);
            var hsv = RGBToHSV(rgb.Item1, rgb.Item2, rgb.Item3);
            hsv.Item3 = Clamp(hsv.Item3, minV, maxV);
            return HSVToHex(hsv.Item1, hsv.Item2, hsv.Item3);
        }

        private static (int, int, int) HexToRGB(string hex)
        {
            hex = hex.Replace("#", "");
            int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return (r, g, b);
        }

        private static (int, int, int) RGBToHSV(int r, int g, int b)
        {
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var delta = max - min;

            var h = 0;
            if (delta != 0)
            {
                if (max == r)
                    h = (int)Math.Round((g - b) / (double)delta * 60 % 360);
                else if (max == g)
                    h = (int)Math.Round((b - r) / (double)delta * 60 + 120);
                else if (max == b)
                    h = (int)Math.Round((r - g) / (double)delta * 60 + 240);
            }

            var s = max == 0 ? 0 : (int)Math.Round(delta / (double)max * 100);
            var v = (int)Math.Round(max / 255.0 * 100);

            return (h, s, v);
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        private static string HSVToHex(int h, int s, int v)
        {
            var r = (int)Math.Round(v / 100.0 * 255 * (1 - s / 100.0 + (s / 100.0 * (h % 60) / 60)));
            var g = (int)Math.Round(v / 100.0 * 255 * (1 - s / 100.0 + (s / 100.0 * (h % 180 < 60 ? h % 180 / 60 : (h % 180 - 120) / 60))));
            var b = (int)Math.Round(v / 100.0 * 255 * (1 - s / 100.0 + (s / 100.0 * (h % 180 >= 60 ? (h % 180 - 120) / 60 : (h % 180) / 60))));

            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }

}
