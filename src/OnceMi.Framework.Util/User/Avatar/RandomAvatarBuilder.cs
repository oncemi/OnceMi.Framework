using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.User
{
    public class RandomAvatarBuilder
    {
        public static RandomAvatarBuilder Build(int size, bool isSymmetry = true)
        {
            return new RandomAvatarBuilder(size, isSymmetry);
        }

        private static readonly List<Color> Colors = new List<Color>()
        {
            Color.FromArgb(127, 127, 220),
            Color.FromArgb(100, 207, 172),
            Color.FromArgb(198, 87, 181),
            Color.FromArgb(134, 166, 220),
            Color.FromArgb(0xf2, 0x4e, 0x33),
            Color.FromArgb(0xf9, 0x97, 0x40),
            Color.FromArgb(0xf9, 0xc8, 0x2f),
            Color.FromArgb(0x14, 0xad, 0xe3),
            Color.FromArgb(0x9e, 0xd2, 0x00),
            Color.FromArgb(0xb5, 0x4e, 0x33),
            Color.FromArgb(0xb5, 0x44, 0xec),
        };

        private readonly RandomAvatar _instance;

        private RandomAvatarBuilder(int size, bool isSymmetry = true)
        {
            _instance = new RandomAvatar
            {
                SquareSize = size,
                FontColor = Color.White,
                Colors = Colors,
                IsSymmetry = isSymmetry
            };
        }

        public RandomAvatarBuilder SetPadding(int padding)
        {
            _instance.Padding = padding;
            return this;
        }

        public RandomAvatarBuilder SetAsymmetry(bool isSymmetry = true)
        {
            _instance.IsSymmetry = isSymmetry;
            return this;
        }
        public RandomAvatarBuilder SetBlockSize(int blockSize)
        {
            _instance.BlockSize = blockSize;
            return this;
        }
        public RandomAvatarBuilder SetFontColor(Color fontColor)
        {
            _instance.FontColor = fontColor;
            return this;
        }
        public Image ToImage()
        {
            return _instance.GenerateImage();
        }

        public byte[] ToBytes()
        {
            var image = ToImage();
            return ImageToBuffer(image, ImageFormat.Png);
        }

        public static byte[] ImageToBuffer(Image image, ImageFormat imageFormat)
        {
            if (image == null)
            {
                return null;
            }
            byte[] data;
            using (MemoryStream stream = new MemoryStream())
            {
                using (Bitmap bitmap = new Bitmap(image))
                {
                    bitmap.Save(stream, imageFormat);
                    stream.Position = 0;
                    data = new byte[stream.Length];
                    stream.Read(data, 0, Convert.ToInt32(stream.Length));
                    stream.Flush();
                }
            }
            return data;
        }

        public RandomAvatarBuilder FixedSeed(bool fixedSeed, string seed)
        {
            _instance.FixedSeed = fixedSeed;
            if (fixedSeed && !string.IsNullOrWhiteSpace(seed))
            {
                if (seed.Length < 3)
                    seed = $"RA{seed}";
                _instance.Seed = Encoding.UTF8.GetBytes(seed);
            }
            return this;
        }
    }
}
