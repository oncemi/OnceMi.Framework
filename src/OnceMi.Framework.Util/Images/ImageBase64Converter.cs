using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OnceMi.Framework.Util.Images
{
    public static class ImageBase64Converter
    {
        /// <summary>
        /// Converts Image to Base64
        /// </summary>
        /// <param name="image">Image</param>
        /// <returns>Base64 String</returns>
        public static string ImageToBase64(Image image)
        {
            using (var m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        /// <summary>
        /// Converts Base64-String to Image
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static Image Base64ToImage(string base64String)
        {
            int indexOfSplit = base64String.LastIndexOf(',');
            if (indexOfSplit != -1)
            {
                base64String = base64String.Substring(indexOfSplit + 1, base64String.Length - (indexOfSplit + 1));
            }
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms);
                return image;
            }
        }
    }
}
