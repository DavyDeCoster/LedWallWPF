using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;
using System.IO;

namespace LedWall
{
    class Ledwall
    {
        public static void ReadPixels(string path)
        {
            List<System.Drawing.Color> lstColor = new List<Color>();

            Bitmap bmOriginal = new Bitmap("C:\\Users\\Davy\\Dropbox\\0.School\\Stage\\LedWallC#\\LedWall\\LedWall\\Images\\red.jpg");
            Bitmap bm = new Bitmap(bmOriginal, 107, 48);
            byte[] data = new byte[107 * 48 * 3 + 3];

            Color[] pixel = new Color[8];
            int[] editedPixels = new int[8];
            int x, y, xbegin, xend, xinc, mask, offset = 3, linesPerPin = 3;
            
            for(y=0;y<linesPerPin;y++)
            {
                if((y&1)==(0))
                {
                    xbegin = 0;
                    xend = 107;
                    xinc = 1;
                }
                else
                {
                    xbegin = 106;
                    xend = -1;
                    xinc = -1;
                }
                for (x = xbegin; x !=xend; x+=xinc)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        pixel[i] = bm.GetPixel(x, y * linesPerPin);
                        editedPixels[i] = colorWiring(pixel[i]);
                    }

                    for (mask = 0x800000; mask != 0; mask >>= 1)
                    {
                        byte b = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            if ((editedPixels[i] & mask) != 0)
                            {
                                b |= (byte)(1 << i);
                            }
                        }

                        data[offset++] = b;
                    }
                }
                char ster = '*';
                data[0] = System.Text.Encoding.ASCII.GetBytes(ster);
                int usec = (int)((1000000.0 / 25) * 0.75);
                data[1] = (byte)(usec);   // request the frame sync pulse
                data[2] = (byte)(usec >> 8); // at 75% of the frame time
            }
            foreach (byte b in data)
            {
                Console.WriteLine(b);
            }
        }

        private static int colorWiring(Color color)
        {
            int red = color.R;
            int green = color.G;
            int blue = color.B;

            return (green << 16) | (red << 8) | (blue);
        }
    }
}
