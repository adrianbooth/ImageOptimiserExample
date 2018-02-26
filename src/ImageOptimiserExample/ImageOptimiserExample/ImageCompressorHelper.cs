using System;
using System.Globalization;
using System.IO;

namespace ImageOptimiserExample
{
    public static class ImageCompressorHelper
    {
        public static string OptimiseImageCommand(string original, string optimised, bool lossy)
        {
            var fileExtension = Path.GetExtension(original).ToLowerInvariant();

            switch (fileExtension)
            {
                case ".png":
                {
                    if (lossy)
                        return String.Format(CultureInfo.CurrentCulture, "/c png-lossy.cmd \"{0}\" \"{1}\"", original, optimised);

                    return String.Format(CultureInfo.CurrentCulture, "/c png-lossless.cmd \"{0}\" \"{1}\"", original, optimised);
                }
                case ".jpg":
                case ".jpeg":
                {
                    if (lossy)
                        return String.Format(CultureInfo.CurrentCulture, "/c cjpeg -quality 80,60 -dct float -smooth 5 -outfile \"{1}\" \"{0}\"", original, optimised);


                    return String.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy none -optimize -progressive -outfile \"{1}\" \"{0}\"", original, optimised);
                }
                case ".gif":
                {
                    return String.Format(CultureInfo.CurrentCulture, "/c gifsicle -O3 --batch --colors=256 \"{0}\" --output=\"{1}\"", original, optimised);
                }
            }

            return null;
        }

        public static void Cleanup(CompressionResult result)
        {
            File.Delete(result.ResultFileName);
            File.Delete(result.OriginalFileName);
        }

        public static string[] SupportedFormats = { ".jpg", ".gif", ".jpeg", ".png" };
    }
}