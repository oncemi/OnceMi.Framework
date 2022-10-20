using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnceMi.Framework.Util.Images;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Images.Tests
{
    [TestClass()]
    public class ImageBase64ConverterTests
    {
        private string image0_path = "sources/image0.bmp";

        [TestMethod()]
        public void ImageToBase64Test()
        {
            if (!File.Exists(image0_path))
            {
                Assert.Fail("图片文件不存在");
            }
            SKData image0Data = SKData.Create(image0_path);
            string image0Base64 = ImageBase64Converter.ImageToBase64(image0Data, out SKEncodedImageFormat format);
            image0Base64 = ImageBase64Converter.ImageToBase64WithFormat(image0Data, out format);

            SKData image0Data1 = ImageBase64Converter.Base64ToImage(image0Base64, out format);
            SKBitmap bitmap = SKBitmap.Decode(image0Data1);
            string pngBase64 = ImageBase64Converter.BitmapToBase64WithFormat(bitmap, SKEncodedImageFormat.Png);

            SKBitmap bitmap1 = ImageBase64Converter.Base64ToBitmap(pngBase64, out _);
            Assert.IsTrue(true);
        }
    }
}