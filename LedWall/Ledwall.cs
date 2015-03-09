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
using AForge.Video.DirectShow;

namespace LedWall
{
    class Ledwall
    {
        static sbyte[] data = new sbyte[107 * 48 * 3 + 3];

        private int[] gammatable { get; set; }
        private float gamma { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Ledwall(int width, int height)
        {
            this.Height = height;
            this.Width = width;
            this.gammatable = new int[256];
            GenerateGammaTable();
        }

        public void ReadImage(string path)
        {
            GenerateGammaTable();

            Bitmap bmOriginal = new Bitmap(path);
            Bitmap bm = new Bitmap(bmOriginal, Width, Height);

            ReadPixelsFromImage(bm);
            AddIntroData();
            
        }

        private void AddIntroData()
        {
            data[0] = (sbyte)System.Text.Encoding.ASCII.GetBytes("*")[0];
            int usec = (int)((1000000.0 / 25) * 0.75);
            data[1] = (sbyte)(usec);   // request the frame sync pulse
            data[2] = (sbyte)(usec >> 8); // at 75% of the frame time
        }

        private void ReadPixelsFromImage(Bitmap bm)
        {
            Color[] pixel = new Color[8];
            int[] editedPixels = new int[8];
            int x, y, xbegin, xend, xinc, mask, offset = 3, linesPerPin = Height/8;

            for (y = 0; y < linesPerPin; y++)
            {
                if ((y & 1) == (0))
                {
                    xbegin = 0;
                    xend = Width;
                    xinc = 1;
                }
                else
                {
                    xbegin = Width-1;
                    xend = -1;
                    xinc = -1;
                }
                for (x = xbegin; x != xend; x += xinc)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        pixel[i] = bm.GetPixel(x, y + (linesPerPin*i));
                        editedPixels[i] = colorWiring(pixel[i]);
                    }

                    for (mask = 0x800000; mask != 0; mask >>= 1)
                    {
                        sbyte b = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            if ((editedPixels[i] & mask) != 0)
                            {
                                b |= (sbyte)(1 << i);
                            }
                        }
                        data[offset++] = b;
                    }
                }
            }
        }

        private void GenerateGammaTable()
        {
            for (int i = 0; i < 256; i++)
            {
                gammatable[i] = (int)(Math.Pow((float)i / 255.0, gamma) * 255.0 + 0.5);
            }
        }

        private int colorWiring(Color color)
        {
            int red = color.R;
            int green = color.G;
            int blue = color.B;

            red = gammatable[red];
            green = gammatable[green];
            blue = gammatable[blue];

            return ((green << 16) | (red << 8) | (blue));
        }

        public void readVideo(string path)
        {
            FileVideoSource fvs = new FileVideoSource(path);

            fvs.NewFrame += fvs_NewFrame;

            fvs.Start();
        }

        private void fvs_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap frame = new Bitmap(eventArgs.Frame, Width, Height);
            ReadPixelsFromImage(frame);
            AddIntroData();
        }
    }
}