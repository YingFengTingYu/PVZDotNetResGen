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
            for (int y = 0; y < copyHeight; y++)
            {
                Span<YFColor> srcColorLine = srcBitmap[y + srcY].Slice(srcX, copyWidth);
                Span<YFColor> destColorLine = destBitmap[y + destY].Slice(destX, copyWidth);
                srcColorLine.CopyTo(destColorLine);
            }
        }

        public static void CopyTo(this RefBitmap srcBitmap, RefBitmap destBitmap, int srcX, int srcY, int destX, int destY, int copyWidth, int copyHeight, int border)
        {
            // 先画图形
            for (int y = 0; y < copyHeight; y++)
            {
                Span<YFColor> srcColorLine = srcBitmap[y + srcY].Slice(srcX, copyWidth);
                Span<YFColor> destColorLine = destBitmap[y + destY].Slice(destX, copyWidth);
                srcColorLine.CopyTo(destColorLine);
            }
            // 再补顶角
            YFColor aColor = srcBitmap[srcX, srcY];
            for (int y = 0; y < border; y++)
            {
                for (int x = 0; x < border; x++)
                {
                    destBitmap[destX - border + x, destY - border + y] = aColor;
                }
            }
            aColor = srcBitmap[srcX + copyWidth - 1, srcY];
            for (int y = 0; y < border; y++)
            {
                for (int x = 0; x < border; x++)
                {
                    destBitmap[destX + copyWidth + x, destY - border + y] = aColor;
                }
            }
            aColor = srcBitmap[srcX, srcY + copyHeight - 1];
            for (int y = 0; y < border; y++)
            {
                for (int x = 0; x < border; x++)
                {
                    destBitmap[destX - border + x, destY + copyHeight + y] = aColor;
                }
            }
            aColor = srcBitmap[srcX + copyWidth - 1, srcY + copyHeight - 1];
            for (int y = 0; y < border; y++)
            {
                for (int x = 0; x < border; x++)
                {
                    destBitmap[destX + copyWidth + x, destY + copyHeight + y] = aColor;
                }
            }
            // 再补四周
            for (int y = 0; y < border; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    destBitmap[destX + x, destY - border + y] = srcBitmap[srcX + x, srcY];
                }
            }
            for (int y = 0; y < border; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    destBitmap[destX + x, destY + copyHeight + y] = srcBitmap[srcX + x, srcY + copyHeight - 1];
                }
            }
            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < border; x++)
                {
                    destBitmap[destX - border + x, destY + y] = srcBitmap[srcX, srcY + y];
                }
            }
            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < border; x++)
                {
                    destBitmap[destX + copyWidth + x, destY + y] = srcBitmap[srcX + copyWidth - 1, srcY + y];
                }
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
