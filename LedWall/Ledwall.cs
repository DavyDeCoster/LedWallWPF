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
        static sbyte[] data;

        private int[] gammatable { get; set; }
        private float gamma { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public SerialWriter[] Ports { get; set; }

        public Ledwall(int width, int height, SerialWriter[] Ports)
        {
            this.Height = height;
            this.Width = width;
            this.Ports = Ports;
            this.gammatable = new int[256];
            GenerateGammaTable();
        }

        public void ReadImage(string path)
        {
            GenerateGammaTable();

            Bitmap bmOriginal = new Bitmap(path);
            Bitmap bm = new Bitmap(bmOriginal, Width, Height);

            Bitmap[] cropped = cropImagesWithSetting(bm);

            SendProtocol(cropped);
        }

        private void DefineData(Bitmap b)
        {
                data = new sbyte[b.Height * b.Width * 3 + 3];
        }

        private Bitmap[] cropImagesWithSetting(Bitmap bm)
        {
            Bitmap[] cropped = new Bitmap[Ports.Length];
            int i = 0;
            foreach (SerialWriter s in Ports)
            {
                Rectangle cropArea = new Rectangle(s.XOffset, s.YOffset, s.WriterWidth, s.WriterHeight);
                cropped[i] = (Bitmap)cropImage(bm, cropArea);
                i++;
            }
            return cropped;
        }

        private void AddIntroData(Boolean master)
        {
            if (master)
            {
                data[0] = (sbyte)System.Text.Encoding.ASCII.GetBytes("*")[0];
                int usec = (int)((1000000.0 / 25) * 0.75);
                data[1] = (sbyte)(usec);   // request the frame sync pulse
                data[2] = (sbyte)(usec >> 8); // at 75% of the frame time
                return;
            }
            else
            {
                data[0] = (sbyte)System.Text.Encoding.ASCII.GetBytes("%")[0];
                data[1] = 0;   // request the frame sync pulse
                data[2] = 0; // at 75% of the frame time
                return;
            }
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
            Bitmap[] cropped = cropImagesWithSetting(frame);
            SendProtocol(cropped);
        }

        private void SendProtocol(Bitmap[] cropped)
        {
            int i = 0;
            bool master = true;
            foreach (Bitmap bm in cropped)
            {
                DefineData(bm);
                ReadPixelsFromImage(bm);
                AddIntroData(master);
                master = false;
                Ports[i].data = data;
                Thread SendThread = new Thread(new ThreadStart(Ports[i].SendData));
                SendThread.Start();
                i++;

            }
        }

        private Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        private double percentageFloat(int percent)
        {
            if (percent == 33) return 1.0 / 3.0;
            if (percent == 17) return 1.0 / 6.0;
            if (percent == 14) return 1.0 / 7.0;
            if (percent == 13) return 1.0 / 8.0;
            if (percent == 11) return 1.0 / 9.0;
            if (percent == 9) return 1.0 / 11.0;
            if (percent == 8) return 1.0 / 12.0;
            return (double)percent / 100.0;
        }

        // scale a number by a percentage, from 0 to 100
        private int percentage(int num, int percent)
        {
            double mult = percentageFloat(percent);
            double output = num * mult;
            return (int)output;
        }

        // scale a number by the inverse of a percentage, from 0 to 100
        private int percentageInverse(int num, int percent)
        {
            double div = percentageFloat(percent);
            double output = num / div;
            return (int)output;
        }
    }
}