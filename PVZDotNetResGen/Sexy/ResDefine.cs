using PVZDotNetResGen.Utils.JsonHelper;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PVZDotNetResGen.Sexy
{
    public class BuildInfo : IJsonVersionCheckable
    {
        public static uint JsonVersion => 0;

        public DiskFormat mDiskFormat;

        public string? mHash;
    }

    public class BuildImageInfo : IJsonVersionCheckable
    {
        public static uint JsonVersion => 0;

        public DiskFormat mDiskFormat;

        public SurfaceFormat mSurface;

        public TextureFormat mFormat;

        public string? mHash;
    }

    public class BuildAtlasInfo : IJsonVersionCheckable
    {
        public class SubImageBuildInfo
        {
            public string? mId;
            public int mX;
            public int mY;
            public int mWidth;
            public int mHeight;
            public string? mHash;
        }

        public static uint JsonVersion => 0;

        public SurfaceFormat mSurface;

        public TextureFormat mFormat;

        public int mWidth;

        public int mHeight;

        public List<SubImageBuildInfo>? mSubImages;
    }

    public class ResLocInfo
    {
        public required string mResPath;
        public required List<string> mLocs;
    }

    public class PackInfo : IJsonVersionCheckable
    {
        public List<ResLocInfo> mResLocs = [];

        public List<string> mGroups = [];

        public static uint JsonVersion => 0;
    }

    [JsonDerivedType(typeof(ResBase<AtlasRes>), typeDiscriminator: nameof(ResType.Atlas))]
    [JsonDerivedType(typeof(ResBase<ImageRes>), typeDiscriminator: nameof(ResType.Image))]
    [JsonDerivedType(typeof(ResBase<SubImageRes>), typeDiscriminator: nameof(ResType.SubImage))]
    [JsonDerivedType(typeof(ResBase<ReanimRes>), typeDiscriminator: nameof(ResType.Reanim))]
    [JsonDerivedType(typeof(ResBase<ParticleRes>), typeDiscriminator: nameof(ResType.Particle))]
    [JsonDerivedType(typeof(ResBase<TrailRes>), typeDiscriminator: nameof(ResType.Trail))]
    [JsonDerivedType(typeof(ResBase<SoundRes>), typeDiscriminator: nameof(ResType.Sound))]
    [JsonDerivedType(typeof(ResBase<FontRes>), typeDiscriminator: nameof(ResType.Font))]
    [JsonDerivedType(typeof(ResBase<MusicRes>), typeDiscriminator: nameof(ResType.Music))]
    [JsonDerivedType(typeof(ResBase<LevelRes>), typeDiscriminator: nameof(ResType.Level))]
    public class ResBase : IJsonVersionCheckable
    {
        public required string mId;
        public required string mGroup;
        public DiskFormat mDiskFormat;

        public static uint JsonVersion => 0;
    }

    public class ResBase<T> : ResBase where T : PlatformProperties
    {
        public required T mUniversalProp;
        public T? mWindowsProp;
        public T? mMacOSProp;
        public T? mLinuxProp;
        public T? mAndroidProp;
        public T? mIOSProp;
        public List<ResBase<T>>? mSameIds;
    }

    public class PlatformProperties
    {
        public int? mUnloadGroup;
    }

    public class AtlasRes : PlatformProperties
    {
        public SurfaceFormat? mSurface = SurfaceFormat.Bgra4444;
        public string? mAtlasName;
        public bool? mNoPal;
        public bool? mA4R4G4B4;
        public bool? mDDSurface;
        public bool? mNoBits;
        public bool? mNoBits2D;
        public bool? mNoBits3D;
        public bool? mA8R8G8B8;
        public bool? mR5G6B5;
        public bool? mA1R5G5B5;
        public bool? mMinSubdivide;
        public bool? mNoAlpha;
        public string? mAlphaImage;
        public uint? mAlphaColor;
        public string? mVariant;
        public string? mAlphaGrid;
        public bool? mLanguageSpecific;
        public TextureFormat? mFormat;
        public int? mWidth;
        public int? mHeight;
        public int? mExtrude;
    }

    public class ImageRes : PlatformProperties
    {
        public SurfaceFormat? mSurface = SurfaceFormat.Bgra4444;
        public bool? mNoPal;
        public bool? mA4R4G4B4;
        public bool? mDDSurface;
        public bool? mNoBits;
        public bool? mNoBits2D;
        public bool? mNoBits3D;
        public bool? mA8R8G8B8;
        public bool? mR5G6B5;
        public bool? mA1R5G5B5;
        public bool? mMinSubdivide;
        public bool? mNoAlpha;
        public string? mAlphaImage;
        public uint? mAlphaColor;
        public string? mVariant;
        public string? mAlphaGrid;
        public int? mRows;
        public int? mCols;
        public bool? mLanguageSpecific;
        public TextureFormat? mFormat = TextureFormat.Png;
        public AnimType? mAnim;
        public int? mFrameDelay;
        public int? mBeginDelay;
        public int? mEndDelay;
        public string? mPerFrameDelay;
        public string? mFrameMap;
    }

    public class SubImageRes : PlatformProperties
    {
        public required string mParent;
        public int mRows;
        public int mCols;
        public AnimType mAnim;
        public int mFrameDelay;
        public int mBeginDelay;
        public int mEndDelay;
    }

    public class ReanimRes : PlatformProperties
    {
        public CompiledFileFormat mFormat;
    }

    public class ParticleRes : PlatformProperties
    {
        public CompiledFileFormat mFormat;
    }

    public class TrailRes : PlatformProperties
    {
        public CompiledFileFormat mFormat;
    }

    public class SoundRes : PlatformProperties
    {
        public double? mVolume;
        public int? mPan;
    }

    public class FontRes : PlatformProperties
    {
        public string? mTags;
        public bool? mIsDefault;
        public bool? mTrueType;
        public bool? mSys;
        public int? mSize;
        public bool? mBold;
        public bool? mItalic;
        public bool? mShadow;
        public bool? mUnderline;
        public int? mStroke;
    }

    public class MusicRes : PlatformProperties
    {

    }

    public class LevelRes : PlatformProperties
    {

    }

    [JsonConverter(typeof(JsonStringEnumConverter<ResType>))]
    public enum ResType
    {
        Atlas,
        Image,
        SubImage,
        Reanim,
        Particle,
        Trail,
        Sound,
        Font,
        Music,
        Level,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<TextureFormat>))]
    public enum TextureFormat
    {
        Content,
        Png,
        Jpg,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<DiskFormat>))]
    public enum DiskFormat
    {
        None,
        Png,
        Jpg,
        Gif,
        Psd,
        Xnb,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<AnimType>))]
    public enum AnimType
    {
        None,
        Once,
        PingPong,
        Loop,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<CompiledFileFormat>))]
    public enum CompiledFileFormat
    {
        Xnb,
        Xml,
        Json,
        CompiledPC,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<SurfaceFormat>))]
    public enum SurfaceFormat
    {
        Color = 0,
        Bgr565 = 1,
        Bgra5551 = 2,
        Bgra4444 = 3,
        Dxt1 = 4,
        Dxt3 = 5,
        Dxt5 = 6,
        NormalizedByte2 = 7,
        NormalizedByte4 = 8,
        Rgba1010102 = 9,
        Rg32 = 10,
        Rgba64 = 11,
        Alpha8 = 12,
        Single = 13,
        Vector2 = 14,
        Vector4 = 15,
        HalfSingle = 16,
        HalfVector2 = 17,
        HalfVector4 = 18,
        HdrBlendable = 19,
        Bgr32 = 20,
        Bgra32 = 21,
        ColorSRgb = 30,
        Bgr32SRgb = 31,
        Bgra32SRgb = 32,
        Dxt1SRgb = 33,
        Dxt3SRgb = 34,
        Dxt5SRgb = 35,
        RgbPvrtc2Bpp = 50,
        RgbPvrtc4Bpp = 51,
        RgbaPvrtc2Bpp = 52,
        RgbaPvrtc4Bpp = 53,
        RgbEtc1 = 60,
        Dxt1a = 70,
        RgbaAtcExplicitAlpha = 80,
        RgbaAtcInterpolatedAlpha = 81,
        Rgb8Etc2 = 90,
        Srgb8Etc2 = 91,
        Rgb8A1Etc2 = 92,
        Srgb8A1Etc2 = 93,
        Rgba8Etc2 = 94,
        SRgb8A8Etc2 = 95
    }
}
