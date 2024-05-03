using PVRTexLib;
using PVZDotNetResGen.Utils.Graphics;
using PVZDotNetResGen.Utils.Graphics.Bitmap;
using PVZDotNetResGen.Utils.MemoryHelper;
using PVZDotNetResGen.Utils.StreamHelper;
using PVZDotNetResGen.Utils.XnbContent;
using System;
using System.IO;

namespace PVZDotNetResGen.Sexy.Image
{
    public class XnbTexture2D : XnbBase<IDisposableBitmap>
    {
        public SurfaceFormat mSurfaceFormat;
        public PVRTexLibCompressorQuality mQuality;

        public static XnbTexture2D Shared { get; } = new XnbTexture2D();

        protected override string ReaderTypeString => "Microsoft.Xna.Framework.Content.Texture2DReader";

        private void SurfaceToPVRTexLibFormat(SurfaceFormat surface, out ulong format, out PVRTexLibColourSpace colourSpace)
        {
            switch (surface)
            {
                case SurfaceFormat.Color:
                    format = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Bgr565:
                    format = PVRDefine.PVRTGENPIXELID3('r', 'g', 'b', 5, 6, 5);
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Bgra5551:
                    format = PVRDefine.PVRTGENPIXELID4('a', 'r', 'g', 'b', 1, 5, 5, 5);
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Bgra4444:
                    colourSpace = PVRTexLibColourSpace.Linear;
                    format = PVRDefine.PVRTGENPIXELID4('a', 'r', 'g', 'b', 4, 4, 4, 4);
                    break;
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                    format = (ulong)PVRTexLibPixelFormat.DXT1;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Dxt1SRgb:
                    format = (ulong)PVRTexLibPixelFormat.DXT1;
                    colourSpace = PVRTexLibColourSpace.sRGB;
                    break;
                case SurfaceFormat.Dxt3:
                    format = (ulong)PVRTexLibPixelFormat.DXT3;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Dxt3SRgb:
                    format = (ulong)PVRTexLibPixelFormat.DXT3;
                    colourSpace = PVRTexLibColourSpace.sRGB;
                    break;
                case SurfaceFormat.Dxt5:
                    format = (ulong)PVRTexLibPixelFormat.DXT5;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Dxt5SRgb:
                    format = (ulong)PVRTexLibPixelFormat.DXT5;
                    colourSpace = PVRTexLibColourSpace.sRGB;
                    break;
                case SurfaceFormat.RgbEtc1:
                    format = (ulong)PVRTexLibPixelFormat.ETC1;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Rgb8Etc2:
                    format = (ulong)PVRTexLibPixelFormat.ETC2_RGB;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Rgb8A1Etc2:
                    format = (ulong)PVRTexLibPixelFormat.ETC2_RGB_A1;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.Rgba8Etc2:
                    format = (ulong)PVRTexLibPixelFormat.ETC2_RGBA;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.RgbPvrtc2Bpp:
                    format = (ulong)PVRTexLibPixelFormat.PVRTCI_2bpp_RGB;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.RgbPvrtc4Bpp:
                    format = (ulong)PVRTexLibPixelFormat.PVRTCI_4bpp_RGB;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.RgbaPvrtc2Bpp:
                    format = (ulong)PVRTexLibPixelFormat.PVRTCI_2bpp_RGBA;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                case SurfaceFormat.RgbaPvrtc4Bpp:
                    format = (ulong)PVRTexLibPixelFormat.PVRTCI_4bpp_RGBA;
                    colourSpace = PVRTexLibColourSpace.Linear;
                    break;
                default:
                    throw new Exception("unsupported surface format:" + surface);
            }
        }

        public override IDisposableBitmap ReadContent(Stream stream, string originalAssetName, byte version)
        {
            var surfaceFormat = (SurfaceFormat)stream.ReadInt32LE();
            int width = stream.ReadInt32LE();
            int height = stream.ReadInt32LE();
            /*int levelCount = */
            stream.ReadInt32LE();
            int thisMipmapSize = stream.ReadInt32LE();
            // 使用PVRTexLib解码
            SurfaceToPVRTexLibFormat(surfaceFormat, out ulong inFormat, out PVRTexLibColourSpace colourSpace);
            unsafe
            {
                using (NativeMemoryOwner memoryOwner = new NativeMemoryOwner((uint)thisMipmapSize))
                {
                    stream.Read(memoryOwner.AsSpan());
                    using (PVRTextureHeader header = new PVRTextureHeader(inFormat, (uint)width, (uint)height, colourSpace: colourSpace))
                    {
                        using (PVRTexture texture = new PVRTexture(header, memoryOwner.Pointer))
                        {
                            ulong rgba8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
                            if (texture.GetTexturePixelFormat() != rgba8888)
                            {
                                texture.Transcode(rgba8888, PVRTexLibVariableType.UnsignedByteNorm, PVRTexLibColourSpace.Linear);
                            }
                            return new PVRTexLibBitmap(new PVRTexture(in texture));
                        }
                    }
                }
            }
        }

        public override void WriteContent(IDisposableBitmap content, Stream stream, string originalAssetName, byte version)
        {
            var surfaceFormat = mSurfaceFormat;
            stream.WriteInt32LE((int)surfaceFormat);
            stream.WriteInt32LE(content.Width);
            stream.WriteInt32LE(content.Height);
            stream.WriteInt32LE(1); // levelCount
            // 使用PVRTexLib编码
            SurfaceToPVRTexLibFormat(surfaceFormat, out ulong outFormat, out PVRTexLibColourSpace colourSpace);
            using (PVRTextureHeader header = new PVRTextureHeader(PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8), (uint)content.Width, (uint)content.Height, colourSpace: PVRTexLibColourSpace.Linear, channelType: PVRTexLibVariableType.UnsignedByteNorm))
            {
                unsafe
                {
                    fixed (YFColor* ptr = content.AsSpan())
                    {
                        using (PVRTexture tex = new PVRTexture(header, ptr))
                        {
                            if (tex.GetTextureDataSize() != 0)
                            {
                                if (tex.Transcode(outFormat, PVRTexLibVariableType.UnsignedByteNorm, colourSpace, mQuality))
                                {
                                    int thisMipmapSize = (int)tex.GetTextureDataSize(0);
                                    stream.WriteInt32LE(thisMipmapSize);
                                    stream.Write(new ReadOnlySpan<byte>(tex.GetTextureDataPointer(0), thisMipmapSize));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
