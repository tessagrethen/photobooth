using System;
using System.Diagnostics;
using Gtk;

namespace Photobooth
{
    public abstract class Filter
    {
        protected struct RGB
        {
            public byte red; // 0-255
            public byte green; // 0-255
            public byte blue; // 0-255

            public RGB(byte r, byte g, byte b)
            {
                red = r;
                green = g;
                blue = b;
            }
        }

        // static creator method for creating filters
        // subclasses should define their own CreateFn which returns a new instance of the Filter
        public delegate Filter CreateFn();

        // requires: valis buffer
        // effects: creates a new Pixbuf modified according to the filter
        public abstract Gdk.Pixbuf Run(Gdk.Pixbuf buffer);

        // requires: 0 <= row < Width; 0 <= col < Height
        // effects: returns RGB with the color of the pixel
        protected static RGB GetPixel(Gdk.Pixbuf buffer, int row, int col)
        {
            Debug.Assert(0 <= row && row < buffer.Height, "Invalid row: " + row);
            Debug.Assert(0 <= col && col < buffer.Width, "Invalid col: " + col);

            bool hasAlpha = buffer.HasAlpha;
            int rowstride = buffer.Rowstride;
            int pixelstride = buffer.NChannels;

            int idx = row * rowstride + col * pixelstride;
            IntPtr data = buffer.Pixels + idx;

            RGB color = new RGB();
            unsafe
            {
                byte* raw = (byte*) data.ToPointer();
                color.red   = *(raw + 0);
                color.green = *(raw + 1);
                color.blue  = *(raw + 2);
            }
            return color;
        }

        // requires: 0 <= row < Width; 0 <= col < Height
        // effects: sets the RGB value of buffer to the color of the given pixel
        protected static void SetPixel(Gdk.Pixbuf buffer, int row, int col, RGB color)
        {
            Debug.Assert(0 <= row && row < buffer.Height, "Invalid row: " + row);
            Debug.Assert(0 <= col && col < buffer.Width, "Invalid col: " + col);

            bool hasAlpha = buffer.HasAlpha;
            int rowstride = buffer.Rowstride;
            int pixelstride = buffer.NChannels;

            int idx = row * rowstride + col * pixelstride;
            IntPtr data = buffer.Pixels + idx;

            unsafe
            {
                byte* raw = (byte*) data.ToPointer();
                *(raw + 0) = color.red; 
                *(raw + 1) = color.green;
                *(raw + 2) = color.blue;
            }
        }
    }

    public class NoneFilter : Filter
    {
        public static Filter Create()
        {
            return new NoneFilter();
        }

        public override Gdk.Pixbuf Run(Gdk.Pixbuf buffer)
        {
            Gdk.Pixbuf result = buffer.Copy();
            return result;
        }
    }

    public class GrayscaleFilter : Filter
    {
        public static Filter Create()
        {
            return new GrayscaleFilter();
        }
        public override Gdk.Pixbuf Run(Gdk.Pixbuf buffer)
        {
            Gdk.Pixbuf result = buffer.Copy();
            int height = buffer.Height;
            int width = buffer.Width;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RGB rgb = GetPixel(buffer, y, x);
                    int gray = (rgb.red + rgb.green + rgb.blue) / 3;
                    byte grayb = Convert.ToByte(gray);
                    RGB color = new RGB(grayb, grayb, grayb);
                    SetPixel(result, y, x, color);
                }
            }
            return result;
        }
    }
    
    public class LightenFilter : Filter
    {
        public static Filter Create()
        {
            return new LightenFilter();
        }
        public override Gdk.Pixbuf Run(Gdk.Pixbuf buffer)
        {
            Gdk.Pixbuf result = buffer.Copy();
            int height = buffer.Height;
            int width = buffer.Width;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RGB rgb = GetPixel(buffer, y, x);
                    int red = Math.Clamp(rgb.red + 100, 0, 255);
                    int green = Math.Clamp(rgb.green + 100, 0, 255);
                    int blue = Math.Clamp(rgb.blue + 100, 0, 255);
                    byte redb = Convert.ToByte(red);
                    byte greenb = Convert.ToByte(green);
                    byte blueb = Convert.ToByte(blue);
                    RGB color = new RGB(redb, greenb, blueb);
                    SetPixel(result, y, x, color);
                }
            }
            return result;
        }
    }  

    public class DarkenFilter : Filter
    {
        public static Filter Create()
        {
            return new DarkenFilter();
        }
        public override Gdk.Pixbuf Run(Gdk.Pixbuf buffer)
        {
            Gdk.Pixbuf result = buffer.Copy();
            int height = buffer.Height;
            int width = buffer.Width;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RGB rgb = GetPixel(buffer, y, x);
                    int red = Math.Clamp(rgb.red - 100, 0, 255);
                    int green = Math.Clamp(rgb.green - 100, 0, 255);
                    int blue = Math.Clamp(rgb.blue - 100, 0, 255);
                    byte redb = Convert.ToByte(red);
                    byte greenb = Convert.ToByte(green);
                    byte blueb = Convert.ToByte(blue);
                    RGB color = new RGB(redb, greenb, blueb);
                    SetPixel(result, y, x, color);
                }
            }
            return result;
        }
    }  
   
    public class JitterFilter : Filter
    {
        public static Filter Create()
        {
            return new JitterFilter();
        }
        public override Gdk.Pixbuf Run(Gdk.Pixbuf buffer)
        {
            Gdk.Pixbuf result = buffer.Copy();
            int height = buffer.Height;
            int width = buffer.Width;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int firstRandom = RandomMaker();
                    int secondRandom = RandomMaker();
                    int thirdRandom = RandomMaker();
                    RGB rgb = GetPixel(buffer, y, x);
                    int r = Math.Clamp(rgb.red + firstRandom, 0, 255);
                    int g = Math.Clamp(rgb.green + secondRandom, 0, 255);
                    int b = Math.Clamp(rgb.blue + thirdRandom, 0, 255);
                    byte redb = Convert.ToByte(r);
                    byte greenb = Convert.ToByte(g);
                    byte blueb = Convert.ToByte(b);
                    RGB color = new RGB(redb, greenb, blueb);
                    SetPixel(result, y, x, color);
                }
            }
            return result;
        }
        public int RandomMaker()
        {
            Random random = new Random();
            return random.Next(-100, 100);
        }
    }    
}
