using PVZDotNetResGen.Utils.Graphics.Bitmap;
using StbImageWriteSharp;
using System;
using System.IO;

namespace PVZDotNetResGen.Utils.Graphics
{
    public static class BitmapHelper
    {
        public static bool Peek(string path, out int width, out int height)
        {
            using FileStream stream = File.OpenRead(path);
            StbImageSharp.ImageInfo? info = StbImageSharp.ImageInfo.FromStream(stream);
            if (info == null)
            {
                width = -1;
                height = -1;
                return false;
            }
            width = info.Value.Width;
            height = info.Value.Height;
            return true;
        }

        public static void CopyTo(this RefBitmap srcBitmap, RefBitmap destBitmap, int srcX, int srcY, int destX, int destY, int copyWidth, int copyHeight)
        {
            copyWidth = Math.Min(copyWidth, srcBitmap.Width - srcX);
            copyHeight = Math.Min(copyHeight, srcBitmap.Height - srcY);
            for (int y = 0; y < copyHeight; y++)
            {
                Span<YFColor> srcColorLine = srcBitmap[y + srcY].Slice(srcX, copyWidth);
                Span<YFColor> destColorLine = destBitmap[y + destY].Slice(destX, copyWidth);
                srcColorLine.CopyTo(destColorLine);
            }
        }

        public static void SaveAsPng<T>(this T bitmap, string path) where T : IBitmap
        {
            SaveAsPng(bitmap.AsRefBitmap(), path);
        }

        public static void SaveAsPng(this RefBitmap bitmap, string path)
        {
            using FileStream fileStream = File.Create(path);
            ImageWriter writer = new ImageWriter();
            unsafe
            {
                fixed (YFColor* colorPtr = bitmap.Data)
                {
                    writer.WritePng(colorPtr, bitmap.Width, bitmap.Height, ColorComponents.RedGreenBlueAlpha, fileStream);
                }
            }
        }

        public static void SaveAsJpg<T>(this T bitmap, string path) where T : IBitmap
        {
            SaveAsJpg(bitmap.AsRefBitmap(), path);
        }

        public static void SaveAsJpg(this RefBitmap bitmap, string path)
        {
            using FileStream fileStream = File.Create(path);
            ImageWriter writer = new ImageWriter();
            unsafe
            {
                fixed (YFColor* colorPtr = bitmap.Data)
                {
                    writer.WriteJpg(colorPtr, bitmap.Width, bitmap.Height, ColorComponents.RedGreenBlueAlpha, fileStream, 100);
                }
            }
        }
    }
}
