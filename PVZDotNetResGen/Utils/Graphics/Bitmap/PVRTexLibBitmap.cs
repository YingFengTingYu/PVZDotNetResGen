//using PVRTexLib;
//using System;

//namespace PVZDotNetResGen.Utils.Graphics.Bitmap
//{
//    public unsafe class PVRTexLibBitmap : IDisposableBitmap
//    {
//        public int Width => width;
//        public int Height => height;
//        public int Area => area;

//        public Span<YFColor> AsSpan()
//        {
//            if (texture == null)
//            {
//                throw new Exception("texture shouldn't be null");
//            }
//            return new Span<YFColor>((YFColor*)texture.GetTextureDataPointer(), area);
//        }

//        public RefBitmap AsRefBitmap()
//        {
//            return new RefBitmap(width, height, AsSpan());
//        }

//        private readonly int width;
//        private readonly int height;
//        private readonly int area;
//        private bool disposedValue;
//        private PVRTexture? texture;

//        public PVRTexLibBitmap(PVRTexture texture)
//        {
//            this.texture = texture;
//            ulong rgba8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
//            if (texture.GetTexturePixelFormat() != rgba8888)
//            {
//                texture.Transcode(rgba8888, PVRTexLibVariableType.UnsignedByteNorm, PVRTexLibColourSpace.sRGB);
//            }
//            width = (int)texture.GetTextureWidth();
//            height = (int)texture.GetTextureHeight();
//            area = width * height;
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    texture?.Dispose();
//                    texture = null;
//                }
//                disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
