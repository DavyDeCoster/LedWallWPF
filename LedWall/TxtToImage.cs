﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedWall
{
    class TxtToImage
    {
        static public Bitmap ConvertTextToImage(string txt, string fontname, int fontsize, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {

                Font font = new Font(fontname, fontsize);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), 0, 0);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), -1, 0);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), 1, 0);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }

            return MakePixelsWhite(bmp);
        }

        static public Bitmap ConvertTextToImage(string txt, string fontname, int fontsize, Color bgcolor, Color fcolor, int width, int Height, int x, int y)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {

                Font font = new Font(fontname, fontsize);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), x, y);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }

            return MakePixelsWhite(bmp);
        }

        private static Bitmap MakePixelsWhite(Bitmap bmp)
        {
            int lenX = bmp.Width;
            int lenY = bmp.Height;
            Color white = Color.White;

            for (int xImage = 0; xImage < lenX; xImage++)
            {
                for (int yImage = 0; yImage < lenY; yImage++)
                {
                    Color pixel = bmp.GetPixel(xImage, yImage);
                    if (pixel.R > 1 || pixel.G > 1 || pixel.B > 1)
                    {
                        bmp.SetPixel(xImage, yImage, white);
                    }
                }
            }
            return bmp;
        }
    }
}
