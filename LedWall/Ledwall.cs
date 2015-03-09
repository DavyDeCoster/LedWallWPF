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
        static SerialPort port;
        static sbyte[] data = new sbyte[107 * 48 * 3 + 3];
        static float gamma = 1.7F;
        static int[] gammatable = new int[256];

        public static void ReadImage(string path)
        {
            String[] Coms = SerialPort.GetPortNames();
            port = new SerialPort();
            port.PortName = Coms[0];

            GenerateGammaTable();

            Bitmap bmOriginal = new Bitmap(path);
            Bitmap bm = new Bitmap(bmOriginal, 107, 48);

            ReadPixelsFromImage(bm);
            AddIntroData();

            port.Open();

        }

        private static void AddIntroData()
        {
            data[0] = (sbyte)System.Text.Encoding.ASCII.GetBytes("*")[0];
            int usec = (int)((1000000.0 / 25) * 0.75);
            data[1] = (sbyte)(usec);   // request the frame sync pulse
            data[2] = (sbyte)(usec >> 8); // at 75% of the frame time
        }

        private static void ReadPixelsFromImage(Bitmap bm)
        {
            Color[] pixel = new Color[8];
            int[] editedPixels = new int[8];
            int x, y, xbegin, xend, xinc, mask, offset = 3, linesPerPin = 3;

            for (y = 0; y < linesPerPin; y++)
            {
                if ((y & 1) == (0))
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
                for (x = xbegin; x != xend; x += xinc)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        pixel[i] = bm.GetPixel(x, y * linesPerPin);
                        editedPixels[i] = colorWiring(pixel[i], gammatable);
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

        private static void GenerateGammaTable()
        {
            for (int i = 0; i < 256; i++)
            {
                gammatable[i] = (int)(Math.Pow((float)i / 255.0, gamma) * 255.0 + 0.5);
            }
        }

        private static int colorWiring(Color color, int[] gammatable)
        {
            int red = color.R;
            int green = color.G;
            int blue = color.B;

            red = gammatable[red];
            green = gammatable[green];
            blue = gammatable[blue];

            return ((green << 16) | (red << 8) | (blue));
        }

        public static void readVideo(string path)
        {
            FileVideoSource fvs = new FileVideoSource(_path + "Video\\red.mp4");

            fvs.NewFrame += fvs_NewFrame;

            fvs.Start();
        }

        static void fvs_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap frame = new Bitmap(eventArgs.Frame, 107, 48);
            ReadPixelsFromImage(frame);
            AddIntroData();
        }
    }
}