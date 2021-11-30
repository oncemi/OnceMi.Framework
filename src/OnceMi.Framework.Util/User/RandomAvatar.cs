using OnceMi.Framework.Util.Text;
using SkiaSharp;
using System.Text;

namespace OnceMi.Framework.Util.User
{
    public class RandomAvatar
    {
        public byte[] Create(string username, int picSize)
        {
            if (picSize < 128)
            {
                picSize = 128;
            }
            string name = GetEncodeChars(username);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "WTF";
            }
            SKBitmap bmp = new SKBitmap(picSize, picSize);
            using (SKCanvas canvas = new SKCanvas(bmp))
            {
                canvas.DrawColor(RandomBackgroundColor(name));
                using (SKPaint sKPaint = new SKPaint())
                {
                    sKPaint.Color = SKColors.White;
                    sKPaint.TextSize = (int)(picSize / 3);
                    sKPaint.IsAntialias = true;
                    //sKPaint.Typeface = SkiaSharp.SKTypeface.FromFamilyName("微软雅黑", SKTypefaceStyle.Bold);//字体

                    SKRect size = new SKRect();
                    //计算文字宽度以及高度
                    sKPaint.MeasureText(name, ref size);

                    float temp = (picSize - size.Size.Width) / 2;
                    float temp1 = (picSize - size.Size.Height) / 2;
                    canvas.DrawText(name, temp, temp1 - size.Top, sKPaint);//画文字
                }
                //保存成图片文件
                using (SKImage img = SKImage.FromBitmap(bmp))
                {
                    using (SKData p = img.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        return p.ToArray();
                    }
                }
            }
        }

        private string GetEncodeChars(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < (username.Length <= 3 ? username.Length : 3); i++)
            {
                if (username[i] > 127)
                {
                    string zhChar = TextUtil.GetPinyinSpellCode(username[i]);
                    if (string.IsNullOrEmpty(zhChar))
                    {
                        sb.Append(username[i]);
                    }
                    sb.Append(zhChar);
                }
                else
                {
                    sb.Append(username[i]);
                }
            }
            return sb.ToString().ToUpper();
        }

        private SKColor RandomBackgroundColor(string username)
        {
            int multiple = 255 / (int)'1';
            byte red = (byte)(username.Length > 0 ? (username[0] * multiple) % 255 : 0);
            byte green = (byte)(username.Length > 1 ? (username[1] * multiple) % 255 : 0);
            byte blue = (byte)(username.Length > 2 ? (username[2] * multiple) % 255 : 0);
            SKColor color = new SKColor(red, green, blue);
            return color;
        }
    }
}
