using System;

namespace PVZDotNetResGen.Utils.Graphics.Bitmap
{
    public readonly struct MemoryBitmap : IBitmap
    {
        public readonly int Width => width;
        public readonly int Height => height;
        public readonly int Area => area;

        public Span<YFColor> AsSpan()
        {
            return data_ptr.Span;
        }

        public RefBitmap AsRefBitmap()
        {
            return new RefBitmap(width, height, AsSpan());
        }

        private readonly int width;
        private readonly int height;
        private readonly int area;
        private readonly Memory<YFColor> data_ptr;

        public MemoryBitmap(int width, int height, Memory<YFColor> data_ptr)
        {
            this.width = width;
            this.height = height;
            area = width * height;
            this.data_ptr = data_ptr[..area];
        }
    }
}
