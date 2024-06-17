using PVZDotNetResGen.Utils.StreamHelper;
using PVZDotNetResGen.Utils.XnbContent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PVZDotNetResGen.Sexy.Reanim;

public class XnbReanimCoder : IReanimCoder, IXnbContentCoder<ReanimatorDefinition>
{
    private enum ReanimOptimisationType
    {
        New = 0,
        CopyPrevious = 1,
        Placeholder = 2
    }

    public static XnbReanimCoder Shared { get; } = new XnbReanimCoder();

    public string ReaderTypeString => "Sexy.TodLib.ReanimReader, LAWN";

    public bool AggressiveUsePlaceHolder = false;
    public bool UsePrevious = true;

    public object ReadContent(Stream stream, string originalAssetName, byte version)
    {
        ReanimatorDefinition reanimatorDefinition = new ReanimatorDefinition();
        reanimatorDefinition.DoScale = (ReanimScaleType)stream.ReadUInt8();
        reanimatorDefinition.Fps = stream.ReadFloat32LE();
        int trackCount = stream.ReadInt32LE();
        for (int i = 0; i < trackCount; i++)
        {
            reanimatorDefinition.Tracks.Add(ReadReanimTrack(stream));
        }
        return reanimatorDefinition;
    }

    private ReanimatorTrack ReadReanimTrack(Stream input)
    {
        ReanimatorTransform? previous = null;
        ReanimatorTrack track = new ReanimatorTrack();
        track.Name = FastReadString(input);
        int transformCount = input.ReadInt32LE();
        for (int i = 0; i < transformCount; i++)
        {
            track.Transforms.Add(ReadReanimTransform(input, ref previous));
        }
        return track;
    }

    private ReanimatorTransform ReadReanimTransform(Stream input, ref ReanimatorTransform? previous)
    {
        ReanimatorTransform transform = new ReanimatorTransform();
        ReanimOptimisationType reanimOptimisationType = (ReanimOptimisationType)input.ReadUInt8();
        if (reanimOptimisationType == ReanimOptimisationType.CopyPrevious)
        {
            Debug.Assert(previous != null);
            transform.TransX = previous.TransX;
            transform.TransY = previous.TransY;
            transform.ScaleX = previous.ScaleX;
            transform.ScaleY = previous.ScaleY;
            transform.SkewX = previous.SkewX;
            transform.SkewY = previous.SkewY;
            transform.Frame = previous.Frame;
            transform.Alpha = previous.Alpha;
            transform.Font = previous.Font;
            transform.Image = previous.Image;
            transform.Text = previous.Text;
        }
        else if (reanimOptimisationType != ReanimOptimisationType.Placeholder)
        {
            transform.Font = FastReadString(input);
            transform.Image = FastReadString(input);
            transform.Text = FastReadString(input);
            transform.Alpha = input.ReadFloat32LE();
            transform.Frame = input.ReadFloat32LE();
            transform.ScaleX = input.ReadFloat32LE();
            transform.ScaleY = input.ReadFloat32LE();
            transform.SkewX = input.ReadFloat32LE();
            transform.SkewY = input.ReadFloat32LE();
            transform.TransX = input.ReadFloat32LE();
            transform.TransY = input.ReadFloat32LE();
        }
        previous = transform;
        return transform;
    }

    private static string? FastReadString(Stream stream)
    {
        int readLen = stream.ReadInt32LE();
        if (readLen <= 0)
        {
            return null;
        }
        return stream.ReadString(readLen * 2, encoding: Encoding.Unicode);
    }

    public void WriteContent(object content, Stream stream, string originalAssetName, byte version)
    {
        ReanimatorDefinition reanim = (ReanimatorDefinition)content;
        stream.WriteUInt8((byte)reanim.DoScale);
        stream.WriteFloat32LE(reanim.Fps);
        List<ReanimatorTrack> tracks = reanim.Tracks;
        stream.WriteInt32LE(tracks.Count);
        for (int i = 0; i < tracks.Count; i++)
        {
            WriteReanimTrack(tracks[i], stream);
        }
    }

    private void WriteReanimTrack(ReanimatorTrack track, Stream stream)
    {
        ReanimatorTransform? previous = null;
        FastWriteString(track.Name, stream);
        List<ReanimatorTransform > transforms = track.Transforms;
        stream.WriteInt32LE(transforms.Count);
        for (int i = 0; i < transforms.Count; i++)
        {
            WriteReanimTransform(transforms[i], stream, ref previous);
        }
    }

    private void WriteReanimTransform(ReanimatorTransform transform, Stream stream, ref ReanimatorTransform? previous)
    {
        if (AggressiveUsePlaceHolder
            && transform.TransX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.TransY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.ScaleX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.ScaleY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.SkewX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.SkewY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.Frame == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.Alpha == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
            && transform.Font == null
            && transform.Image == null
            && transform.Text == null)
        {
            stream.WriteUInt8((byte)ReanimOptimisationType.Placeholder);
        }
        else if (UsePrevious && previous != null
                 && transform.TransX == previous.TransX
                 && transform.TransY == previous.TransY
                 && transform.ScaleX == previous.ScaleX
                 && transform.ScaleY == previous.ScaleY
                 && transform.SkewX == previous.SkewX
                 && transform.SkewY == previous.SkewY
                 && transform.Frame == previous.Frame
                 && transform.Alpha == previous.Alpha
                 && transform.Font == previous.Font
                 && transform.Image == previous.Image
                 && transform.Text == previous.Text)
        {
            stream.WriteUInt8((byte)ReanimOptimisationType.CopyPrevious);
        }
        else
        {
            stream.WriteUInt8((byte)ReanimOptimisationType.New);
            FastWriteString(transform.Font, stream);
            FastWriteString(transform.Image, stream);
            FastWriteString(transform.Text, stream);
            stream.WriteFloat32LE(transform.Alpha);
            stream.WriteFloat32LE(transform.Frame);
            stream.WriteFloat32LE(transform.ScaleX);
            stream.WriteFloat32LE(transform.ScaleY);
            stream.WriteFloat32LE(transform.SkewX);
            stream.WriteFloat32LE(transform.SkewY);
            stream.WriteFloat32LE(transform.TransX);
            stream.WriteFloat32LE(transform.TransY);
        }
        previous = transform;
    }

    private static void FastWriteString(string? str, Stream stream)
    {
        Encoding encoding = Encoding.Unicode;
        int size = encoding.GetMaxByteCount(str?.Length ?? 0);
        Span<byte> buffer = stackalloc byte[size];
        size = encoding.GetBytes(str, buffer);
        stream.WriteInt32LE(size / 2);
        stream.Write(buffer[..size]);
    }

    public void Encode(ReanimatorDefinition content, Stream stream)
    {
        XnbHelper.Encode(new XnbContent(content, 0), "reanim", stream);
    }

    public ReanimatorDefinition Decode(Stream stream)
    {
        return (ReanimatorDefinition)XnbHelper.Decode("reanim", stream).PrimaryResource;
    }
}