using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
namespace Wikiped.Models
{
    public class SlikeServices
    {
        public static Image ResizeByWidth(Image Img, int NewWidth)
        {

            float PercentW = ((float)Img.Width / (float)NewWidth);

            Bitmap bmp = new Bitmap(NewWidth, Img.Height);
            Graphics g = Graphics.FromImage(bmp);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(Img, 0, 0, bmp.Width, bmp.Height);
            g.Dispose();

            return bmp;
        }

        public static Image ResizeByHeight(Image Img, int NewHeight)
        {

            float PercentH = ((float)Img.Height / (float)NewHeight);

            Bitmap bmp = new Bitmap(Img.Width, NewHeight);
            Graphics g = Graphics.FromImage(bmp);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(Img, 0, 0, bmp.Width, bmp.Height);
            g.Dispose();

            return bmp;
        }
        public static Image IzreziSlikuPravilno(Image img, int w, int h, int tmp)
        {
            using (img)
            {
                Bitmap newBitmap = new Bitmap(w, h);

                using (Graphics graphics = Graphics.FromImage(newBitmap))
                {
                    if (tmp == 1)
                    {

                        graphics.DrawImage(
                            img, 0, 0,
                            new Rectangle((img.Width / 2) - (w / 2), 0, newBitmap.Width, newBitmap.Height), GraphicsUnit.Pixel);
                        graphics.Flush();
                    }
                    else
                    {
                        graphics.DrawImage(
                           img, 0, 0,
                           new Rectangle(0, ((img.Height / 2) - (h / 2)), newBitmap.Width, newBitmap.Height), GraphicsUnit.Pixel);
                        graphics.Flush();
                    }
                }

                return newBitmap;
            }

        }
        public static Image ResizeSlikePravilno(Image img, int noviWidth, int noviHeight)
        {

            float width = (float)img.Width;
            float height = (float)img.Height;

            float WidthF = 0;
            float heightF = 0;

            if (width >= noviWidth && height >= noviHeight)
            {
                float wPar = width / noviWidth;
                float hPar = height / noviHeight;

                if ((height / wPar) > noviHeight)
                {
                    WidthF = width / wPar;
                    heightF = height / wPar;
                }
                if ((width / hPar) >= noviWidth)
                {
                    heightF = height / hPar;
                    WidthF = width / hPar;
                }


            }
            if (width >= noviWidth && height < noviHeight)
            {
                WidthF = width * (noviHeight / height);
                heightF = noviHeight;
            }
            if (width < noviWidth && height >= noviHeight)
            {
                heightF = height * (noviWidth / width);
                WidthF = noviWidth;
            }
            if (width < noviWidth && height < noviHeight)
            {
                float wPar = noviWidth / width;
                float hPar = noviHeight / height;

                if ((width * hPar) > noviHeight)
                {
                    heightF = height * hPar;
                    WidthF = width * hPar;


                }
                if ((height * wPar) >= noviWidth)
                {

                    heightF = height * wPar;
                    WidthF = width * wPar;


                }


            }
            int hf = (int)Math.Round(heightF, 1);
            int wf = (int)Math.Round(WidthF, 1);

            img = ResizeByWidth(img, wf);
            img = ResizeByHeight(img, hf);

            if (WidthF > noviWidth)
            {
                img = IzreziSlikuPravilno(img, noviWidth, noviHeight, 1);
            }
            if (heightF > noviHeight)
            {
                img = IzreziSlikuPravilno(img, noviWidth, noviHeight, 2);
            }

            return img;







        }
    }
}