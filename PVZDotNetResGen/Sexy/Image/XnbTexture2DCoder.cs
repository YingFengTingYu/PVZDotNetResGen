using PVZDotNetResGen.Utils.Graphics.Bitmap;
using PVZDotNetResGen.Utils.MemoryHelper;
using PVZDotNetResGen.Utils.StreamHelper;
using PVZDotNetResGen.Utils.XnbContent;
using System.IO;
using System;
using PVZDotNetResGen.Utils.Graphics;
using PVRTexLibNET;

namespace PVZDotNetResGen.Sexy.Image;

public class XnbTexture2DCoder : IXnbContentCoder<IDisposableBitmap>
{
    public static SurfaceFormat SurfaceFormat;
    public static TextureQuality Quality = TextureQuality.Fast;

    public enum TextureQuality
    {
        Fast,
        Medium,
        High,
    }

    public static XnbTexture2DCoder Shared { get; } = new XnbTexture2DCoder();

    public string ReaderTypeString => "Microsoft.Xna.Framework.Content.Texture2DReader";

    private static CompressorQuality TextureQualityToPVRTexLibCompressorQuality(PixelFormat format, TextureQuality quality)
    {
        switch (format)
        {
            case PixelFormat.ETC1:
            case PixelFormat.ETC2_RGB:
            case PixelFormat.ETC2_RGBA:
            case PixelFormat.ETC2_RGB_A1:
            case PixelFormat.EAC_R11:
            case PixelFormat.EAC_RG11:
                return quality switch
                {
                    TextureQuality.Fast => CompressorQuality.ETCFastPerceptual,
                    TextureQuality.Medium => CompressorQuality.ETCMediumPerceptual,
                    TextureQuality.High => CompressorQuality.ETCSlowPerceptual,
                    _ => CompressorQuality.ETCFastPerceptual,
                };
            case PixelFormat.PVRTCI_2bpp_RGB:
            case PixelFormat.PVRTCI_2bpp_RGBA:
            case PixelFormat.PVRTCI_4bpp_RGB:
            case PixelFormat.PVRTCI_4bpp_RGBA:
            case PixelFormat.PVRTCII_2bpp:
            case PixelFormat.PVRTCII_4bpp:
                return quality switch
                {
                    TextureQuality.Fast => CompressorQuality.PVRTCFast,
                    TextureQuality.Medium => CompressorQuality.PVRTCNormal,
                    TextureQuality.High => CompressorQuality.PVRTCBest,
                    _ => CompressorQuality.PVRTCFast,
                };
        }
        return 0;
    }

    private static void SurfaceToPVRTexLibFormat(SurfaceFormat surface, out PixelFormat format, out ColourSpace colourSpace)
    {
        switch (surface)
        {
            case SurfaceFormat.Color:
                format = PixelFormat.RGBA8888;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Bgr565:
                format = PixelFormat.RGB565;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Dxt1:
            case SurfaceFormat.Dxt1a:
                format = PixelFormat.DXT1;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Dxt1SRgb:
                format = PixelFormat.DXT1;
                colourSpace = ColourSpace.sRGB;
                break;
            case SurfaceFormat.Dxt3:
                format = PixelFormat.DXT3;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Dxt3SRgb:
                format = PixelFormat.DXT3;
                colourSpace = ColourSpace.sRGB;
                break;
            case SurfaceFormat.Dxt5:
                format = PixelFormat.DXT5;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Dxt5SRgb:
                format = PixelFormat.DXT5;
                colourSpace = ColourSpace.sRGB;
                break;
            case SurfaceFormat.RgbEtc1:
                format = PixelFormat.ETC1;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Rgb8Etc2:
                format = PixelFormat.ETC2_RGB;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Rgb8A1Etc2:
                format = PixelFormat.ETC2_RGB_A1;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Rgba8Etc2:
                format = PixelFormat.ETC2_RGBA;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.RgbPvrtc2Bpp:
                format = PixelFormat.PVRTCI_2bpp_RGB;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.RgbPvrtc4Bpp:
                format = PixelFormat.PVRTCI_4bpp_RGB;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.RgbaPvrtc2Bpp:
                format = PixelFormat.PVRTCI_2bpp_RGBA;
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.RgbaPvrtc4Bpp:
                format = PixelFormat.PVRTCI_4bpp_RGBA;
                colourSpace = ColourSpace.lRGB;
                break;
            default:
                throw new Exception("unsupported surface format:" + surface);
        }
    }

    public object ReadContent(Stream stream, string originalAssetName, byte version)
    {
        //var surfaceFormat = (SurfaceFormat)stream.ReadInt32LE();
        //int width = stream.ReadInt32LE();
        //int height = stream.ReadInt32LE();
        ///*int levelCount = */
        //stream.ReadInt32LE();
        //int thisMipmapSize = stream.ReadInt32LE();
        //// 使用PVRTexLib解码
        //SurfaceToPVRTexLibFormat(surfaceFormat, out PixelFormat inFormat, out ColourSpace colourSpace);
        //unsafe
        //{
        //    using (NativeMemoryOwner memoryOwner = new NativeMemoryOwner((uint)thisMipmapSize))
        //    {
        //        stream.Read(memoryOwner.AsSpan());
        //        using (PVRTextureHeader header = new PVRTextureHeader(inFormat, (uint)width, (uint)height, colourSpace: colourSpace))
        //        {
        //            using (PVRTexture texture = new PVRTexture(header, memoryOwner.Pointer))
        //            {
        //                PixelFormat
        //                PixelFormat rgba8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
        //                if (texture.GetTexturePixelFormat() != rgba8888)
        //                {
        //                    texture.Transcode(rgba8888, VariableType.UnsignedByteNorm, ColourSpace.lRGB);
        //                }
        //                return new PVRTexLibBitmap(new PVRTexture(in texture));
        //            }
        //        }
        //    }
        //}
        throw new NotImplementedException();
    }

    public void WriteContent(object content, Stream stream, string originalAssetName, byte version)
    {
        IDisposableBitmap bitmap = (IDisposableBitmap)content;
        var surfaceFormat = SurfaceFormat;
        stream.WriteInt32LE((int)surfaceFormat);
        stream.WriteInt32LE(bitmap.Width);
        stream.WriteInt32LE(bitmap.Height);
        stream.WriteInt32LE(1); // levelCount
        // 使用PVRTexLib编码
        SurfaceToPVRTexLibFormat(surfaceFormat, out PixelFormat outFormat, out ColourSpace colourSpace);
        unsafe
        {
            fixed (YFColor* ptr = bitmap.AsSpan())
            {
                YFColor* colorPtr = ptr;
                for (int i = 0; i < bitmap.Area; i++)
                {
                    colorPtr->mRed = (byte)(colorPtr->mRed * colorPtr->mAlpha / 255);
                    colorPtr->mGreen = (byte)(colorPtr->mGreen * colorPtr->mAlpha / 255);
                    colorPtr->mBlue = (byte)(colorPtr->mBlue * colorPtr->mAlpha / 255);
                    colorPtr++;
                }
                nint tex = PVRTexture.CreateTexture((nint)ptr, (uint)bitmap.Width, (uint)bitmap.Height, 1, PixelFormat.RGBA8888, false, VariableType.UnsignedByte, ColourSpace.lRGB);
                {
                    if (PVRTexture.GetTextureDataSize(tex) != 0)
                    {
                        // 预乘
                        if (PVRTexture.Transcode(tex, outFormat, VariableType.UnsignedByte, colourSpace, TextureQualityToPVRTexLibCompressorQuality(outFormat, Quality)))
                        {
                            int thisMipmapSize = (int)PVRTexture.GetTextureDataSize(tex, 0);
                            stream.WriteInt32LE(thisMipmapSize);
                            byte[] arr = new byte[thisMipmapSize];
                            fixed (byte* arrPtr = arr)
                            {
                                PVRTexture.GetTextureData(tex, (nint)arrPtr, (uint)thisMipmapSize, 0);
                            }
                            stream.Write(arr);
                        }
                    }
                }
                PVRTexture.DestroyTexture(tex);
            }
        }
    }
}