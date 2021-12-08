using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// NOTE: Debug.Writeline does not work in appservice

namespace WL_WebAPI.Services
{
    public static class ResizeImageService
    {
        public static byte[] GetJpegThumbnail(byte[] imagebytes)
        {
            SKBitmap resized = ResizeImageService.Resize(imagebytes);

            SKData skdata = resized.Encode(SKEncodedImageFormat.Jpeg, 75);

            Debug.WriteLine($"XXXX: GetJpegThumbnail new size: {skdata.Size} old size: {imagebytes.Length}");

            byte[] jpeg_bytes = skdata.ToArray();

            return jpeg_bytes;
        }


        public static SKBitmap Resize(byte[] imagebytes)
        {
            Debug.WriteLine("XXXX: Resize start" + System.DateTime.Now + " " + System.DateTime.Now.Millisecond);
            SKBitmap bitmap;
           
            bitmap = SKBitmap.Decode(imagebytes);

            Debug.WriteLine("XXXX: Resizeoriginal bitmap w: " + bitmap.Width + "h: " + bitmap.Height);

            double dwidth = bitmap.Width, dheight = bitmap.Height;
            bool tall = bitmap.Width < bitmap.Height;

            Debug.WriteLine("XXXX: Resize tall: " + tall);

            double setImageSizeTo = 200; // = width for tall or height for wide images

            if (tall)
            {
                while (dwidth > setImageSizeTo)
                {
                    //dwidth /= 1.1; dheight /= 1.1;
                    double ratio = dheight / dwidth;
                    dwidth = setImageSizeTo;
                    dheight = ratio * dwidth;
                    Debug.WriteLine("XXXX Resize: calc1 w: " + dwidth + "/" + (int)dwidth +
                        " h: " + dheight + "/" + (int)dheight);
                }
            }
            else
            {
                while (dheight > setImageSizeTo)
                {
                    //dwidth /= 1.1; dheight /= 1.1;
                    double ratio = dwidth / dheight;
                    dheight = setImageSizeTo;
                    dwidth = ratio * dheight;
                    Debug.WriteLine("XXXX Resize: calc2 w: " + dwidth + "/" + (int)dwidth +
                        " h: " + dheight + "/" + (int)dheight);
                }
            }
            Debug.WriteLine("XXXX Resize: calc END w: " + dwidth + "/" + (int)dwidth +
                        " h: " + dheight + "/" + (int)dheight);

            int width = (int)dwidth, height = (int)dheight;

            var smallerBitmap = new SKBitmap(width, height, bitmap.ColorType, bitmap.AlphaType);
            bool scaling_okay = bitmap.ScalePixels(smallerBitmap, SKFilterQuality.Low);

            Debug.WriteLine("XXXX Resize: resize end: " + System.DateTime.Now + " " + System.DateTime.Now.Millisecond);
            if (!scaling_okay)
            {
                Debug.WriteLine("XXXX Resize: scaling ERROR");
                return bitmap;
            }
            else
            {
                //ToggleUploadButtonProperty = true;
                return smallerBitmap;
            }



        }
    }
}
