using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CustomChrome
{
    public static class ImageUtil
    {
        public static Bitmap GetImage(Bitmap source, Color color)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var width = source.Width;
            var height = source.Height;
            var result = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // The provided image is used as an alpha map. Easiest for
                    // us would be to use the alpha channel, but those are
                    // more difficult to create. Instead we take the average
                    // of the color of the pixel and use that as a basis for
                    // the alpha channel. Best is to just use a black icon.

                    var pixel = source.GetPixel(x, y);
                    int alpha = 255 - (int)((pixel.R + pixel.G + pixel.B) / 3.0);
                    result.SetPixel(x, y, Color.FromArgb(alpha, color));
                }
            }

            return result;
        }
    }
}
