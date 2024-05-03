using StbImageSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PVZDotNetResGen.Utils.Graphics.Bitmap
{
    public unsafe class StbBitmap : IDisposableBitmap
    {
        public int Width => width;
        public int Height => height;
        public int Area => area;

        public Span<YFColor> AsSpan()
        {
            return new Span<YFColor>(data_ptr, area);
        }

        public RefBitmap AsRefBitmap()
        {
            return new RefBitmap(width, height, AsSpan());
        }

        private readonly int width;
        private readonly int height;
        private readonly int area;
        private YFColor* data_ptr;
        private bool disposedValue;

        public StbBitmap(string path)
        {
            using FileStream fileStream = File.OpenRead(path);
            int width, height, comp;
            void* ptr = StbImage.stbi__load_and_postprocess_8bit(new StbImage.stbi__context(fileStream), &width, &height, &comp, (int)ColorComponents.RedGreenBlueAlpha);
            this.width = width;
            this.height = height;
            area = width * height;
            data_ptr = (YFColor*)ptr;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                Marshal.FreeHGlobal((nint)data_ptr);
                data_ptr = null;
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~StbBitmap()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
