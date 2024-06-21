using PVZDotNetResGen.Utils.Graphics.Bitmap;
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
            case SurfaceFormat.Bgra5551:
                format = PVRTGENPIXELID4('a', 'r', 'g', 'b', 1, 5, 5, 5);
                colourSpace = ColourSpace.lRGB;
                break;
            case SurfaceFormat.Bgra4444:
                format = PVRTGENPIXELID4('a', 'r', 'g', 'b', 4, 4, 4, 4);
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

    public static PixelFormat PVRTGENPIXELID4(ulong C1Name, ulong C2Name, ulong C3Name, ulong C4Name, ulong C1Bits, ulong C2Bits, ulong C3Bits, ulong C4Bits)
    {
        return (PixelFormat)(C1Name + (C2Name << 8) + (C3Name << 16) + (C4Name << 24) + (C1Bits << 32) + (C2Bits << 40) + (C3Bits << 48) + (C4Bits << 56));
    }

    public static PixelFormat PVRTGENPIXELID3(ulong C1Name, ulong C2Name, ulong C3Name, ulong C1Bits, ulong C2Bits, ulong C3Bits)
    {
        return PVRTGENPIXELID4(C1Name, C2Name, C3Name, 0uL, C1Bits, C2Bits, C3Bits, 0uL);
    }

    public static PixelFormat PVRTGENPIXELID2(ulong C1Name, ulong C2Name, ulong C1Bits, ulong C2Bits)
    {
        return PVRTGENPIXELID4(C1Name, C2Name, 0uL, 0uL, C1Bits, C2Bits, 0uL, 0uL);
    }

    public static PixelFormat PVRTGENPIXELID1(ulong C1Name, ulong C1Bits)
    {
        return PVRTGENPIXELID4(C1Name, 0uL, 0uL, 0uL, C1Bits, 0uL, 0uL, 0uL);
    }

    public object ReadContent(Stream stream, string originalAssetName, byte version)
    {
        var surfaceFormat = (SurfaceFormat)stream.ReadInt32LE();
        int width = stream.ReadInt32LE();
        int height = stream.ReadInt32LE();
        /*int levelCount = */
        stream.ReadInt32LE();
        int thisMipmapSize = stream.ReadInt32LE();
        // 使用PVRTexLib解码
        SurfaceToPVRTexLibFormat(surfaceFormat, out PixelFormat inFormat, out ColourSpace colourSpace);
        unsafe
        {
            byte[] textureData = new byte[thisMipmapSize];
            stream.ReadExactly(textureData, 0, thisMipmapSize);
            fixed (byte* textureDataPtr = textureData)
            {
                nint tex = 0;
                try
                {
                    tex = PVRTexture.CreateTexture((nint)textureDataPtr, (uint)width, (uint)height, 1, inFormat, false, VariableType.UnsignedByte, colourSpace);
                    PVRTexture.Transcode(tex, PixelFormat.RGBA8888, VariableType.UnsignedByte, ColourSpace.lRGB);
                    uint outSize = PVRTexture.GetTextureDataSize(tex, 0);
                    MemoryPoolBitmap memBitmap = new MemoryPoolBitmap(width, height);
                    fixed (YFColor* ptr = memBitmap.AsSpan())
                    {
                        PVRTexture.GetTextureData(tex, (nint)ptr, outSize, 0);
                        YFColor* colorPtr = ptr;
                        for (int i = 0; i < memBitmap.Area; i++)
                        {
                            if (colorPtr->mAlpha != 0)
                            {
                                colorPtr->mRed = (byte)Math.Clamp(colorPtr->mRed * 255 / colorPtr->mAlpha, 0, 255);
                                colorPtr->mGreen = (byte)Math.Clamp(colorPtr->mGreen * 255 / colorPtr->mAlpha, 0, 255);
                                colorPtr->mBlue = (byte)Math.Clamp(colorPtr->mBlue * 255 / colorPtr->mAlpha, 0, 255);
                            }
                            colorPtr++;
                        }
                    }
                    return memBitmap;
                }
                finally
                {
                    if (tex != 0)
                    {
                        PVRTexture.DestroyTexture(tex);
                    }
                }
            }
        }
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
                nint tex = 0;
                try
                {
                    tex = PVRTexture.CreateTexture((nint)ptr, (uint)bitmap.Width, (uint)bitmap.Height, 1, PixelFormat.RGBA8888, false, VariableType.UnsignedByte, ColourSpace.lRGB);
                    {
                        if (PVRTexture.GetTextureDataSize(tex) != 0)
                        {
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
                }
                finally
                {
                    if (tex != 0)
                    {
                        PVRTexture.DestroyTexture(tex);
                    }
                }
            }
        }
    }
}