using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.User
{
    public class RandomAvatar
    {
        private byte[] _guidBytes;
        public int SquareSize { get; set; }

        public int BlockSize { get; set; }
        public bool IsSymmetry { get; set; }
        public List<Color> Colors { get; set; }

        public Color FontColor { get; set; }
        public int Padding { get; set; }

        int RealSize => SquareSize - (Padding * 2);
        public bool FixedSeed { get; set; }
        public byte[] Seed { get; set; }

        public Image GenerateImage()
        {
            SetDefaultOptions();
            bool[] blocks = null;
            while (!Validate(blocks))
            {
                blocks = GenerateRandomBlocks();
            }
            Bitmap avatar = new Bitmap(RealSize + (Padding * 2), RealSize + (Padding * 2));
            DrawAvatar(avatar, blocks);
            return avatar;
        }

        void SetDefaultOptions()
        {
            SetBytes();
            SquareSize = SquareSize > 0 ? SquareSize : 100;
            BlockSize = BlockSize >= 3 ? BlockSize : 5;
        }

        bool Validate(bool[] blocks)
        {
            if (blocks == null)
            {
                return false;
            }
            var count =
                blocks.Aggregate(0,
                    (current, block) => block ? current + 1 : current
                    );
            if (2 < BlockSize && count < 6)
            {
                return false;
            }
            if (count == BlockSize * BlockSize)
            {
                return false;
            }
            return true;
        }

        bool[] GenerateRandomBlocks()
        {
            bool[] blocks = new bool[BlockSize * BlockSize];
            for (int y = 0; y < BlockSize; y++)
            {
                for (int x = 0; x < BlockSize; x++)
                {
                    int index = (y * BlockSize) + x;
                    if (BlockSize / 2 < x && IsSymmetry)
                    {
                        blocks[index] = blocks[index - x + BlockSize - x - 1];
                    }
                    else
                    {
                        blocks[index] = (GetNextByte() & 1) == 0;
                    }
                }
            }
            return blocks;
        }
        int _i;
        public byte GetNextByte()
        {
            if (_i >= _guidBytes.Length)
            {
                _i = 0;
                SetBytes();
            }
            return _guidBytes[_i++];
        }

        private void SetBytes()
        {
            _guidBytes = (FixedSeed && Seed != null && Seed.Length > 0) ? Seed : Guid.NewGuid().ToByteArray();
        }

        private void DrawAvatar(Image avatar, bool[] blocks)
        {
            using (var g = Graphics.FromImage(avatar))
            {
                int size = RealSize / BlockSize;
                var index = GetNextByte() % Colors.Count;
                Color color = Colors[index];
                int holeBlockSizeX = 0;
                var holeBlockSizeY = BlockSize / 2;
                g.Clear(color);
                for (int y = 0; y < BlockSize; y++)
                {
                    for (int x = 0; x < BlockSize; x++)
                    {
                        if (y < holeBlockSizeY && x < holeBlockSizeX)
                        {
                            continue;
                        }
                        if (!blocks[(y * BlockSize) + x]) continue;
                        Brush brush = new SolidBrush(Color.White);
                        g.FillRectangle(brush,
                            new Rectangle(Padding + (x * size), Padding + (y * size), size, size)
                            );
                    }
                }
                //RenderFont(g);
            }
        }

        //private void RenderFont(Graphics graphics)
        //{
        //    //Font f = new Font(new FontFamily("Arial Black"), 
        //    //    (int)(holeBlockSizeY * size * 0.65)
        //    //    );

        //    // Brush brush1 = new SolidBrush(Colors[(index + 1)%Colors.Count]);

        //    // g.DrawString("BC", f, brush1, 10, 10);
        //}
    }
}
