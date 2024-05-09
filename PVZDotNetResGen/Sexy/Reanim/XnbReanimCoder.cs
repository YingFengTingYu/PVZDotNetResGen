using PVZDotNetResGen.Utils.StreamHelper;
using PVZDotNetResGen.Utils.XnbContent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public class XnbReanimCoder : XnbBase<ReanimatorDefinition>, IReanimCoder
    {
        private enum ReanimOptimisationType
        {
            New,
            CopyPrevious,
            Placeholder
        }

        public static XnbReanimCoder Shared { get; } = new XnbReanimCoder();

        protected override string ReaderTypeString => "Sexy.TodLib.ReanimReader, LAWN";

        private ReanimatorTransform? mPrevious;

        public bool mAggressiveUsePlaceHolder = false;

        public override ReanimatorDefinition ReadContent(Stream stream, string originalAssetName, byte version)
        {
            ReanimatorDefinition reanimatorDefinition = new ReanimatorDefinition();
            reanimatorDefinition.mDoScale = (ReanimScaleType)stream.ReadUInt8();
            reanimatorDefinition.mFPS = stream.ReadFloat32LE();
            int trackCount = stream.ReadInt32LE();
            for (int i = 0; i < trackCount; i++)
            {
                reanimatorDefinition.mTracks.Add(ReadReanimTrack(stream));
            }
            return reanimatorDefinition;
        }

        private ReanimatorTrack ReadReanimTrack(Stream input)
        {
            mPrevious = null;
            ReanimatorTrack track = new ReanimatorTrack();
            track.mName = FastReadString(input);
            int transformCount = input.ReadInt32LE();
            for (int i = 0; i < transformCount; i++)
            {
                track.mTransforms.Add(ReadReanimTransform(input));
            }
            mPrevious = null;
            return track;
        }

        private ReanimatorTransform ReadReanimTransform(Stream input)
        {
            ReanimatorTransform transform = new ReanimatorTransform();
            ReanimOptimisationType reanimOptimisationType = (ReanimOptimisationType)input.ReadUInt8();
            if (reanimOptimisationType == ReanimOptimisationType.CopyPrevious)
            {
                Debug.Assert(mPrevious != null);
                transform.mTransX = mPrevious.mTransX;
                transform.mTransY = mPrevious.mTransY;
                transform.mScaleX = mPrevious.mScaleX;
                transform.mScaleY = mPrevious.mScaleY;
                transform.mSkewX = mPrevious.mSkewX;
                transform.mSkewY = mPrevious.mSkewY;
                transform.mFrame = mPrevious.mFrame;
                transform.mAlpha = mPrevious.mAlpha;
                transform.mFont = mPrevious.mFont;
                transform.mImage = mPrevious.mImage;
                transform.mText = mPrevious.mText;
            }
            else if (reanimOptimisationType != ReanimOptimisationType.Placeholder)
            {
                transform.mFont = FastReadString(input);
                transform.mImage = FastReadString(input);
                transform.mText = FastReadString(input);
                transform.mAlpha = input.ReadFloat32LE();
                transform.mFrame = input.ReadFloat32LE();
                transform.mScaleX = input.ReadFloat32LE();
                transform.mScaleY = input.ReadFloat32LE();
                transform.mSkewX = input.ReadFloat32LE();
                transform.mSkewY = input.ReadFloat32LE();
                transform.mTransX = input.ReadFloat32LE();
                transform.mTransY = input.ReadFloat32LE();
            }
            mPrevious = transform;
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

        public override void WriteContent(ReanimatorDefinition content, Stream stream, string originalAssetName, byte version)
        {
            stream.WriteUInt8((byte)content.mDoScale);
            stream.WriteFloat32LE(content.mFPS);
            List<ReanimatorTrack> tracks = content.mTracks;
            stream.WriteInt32LE(tracks.Count);
            for (int i = 0; i < tracks.Count; i++)
            {
                WriteReanimTrack(tracks[i], stream);
            }
        }

        private void WriteReanimTrack(ReanimatorTrack track, Stream stream)
        {
            mPrevious = null;
            FastWriteString(track.mName, stream);
            List<ReanimatorTransform > transforms = track.mTransforms;
            stream.WriteInt32LE(transforms.Count);
            for (int i = 0; i < transforms.Count; i++)
            {
                WriteReanimTransform(transforms[i], stream);
            }
            mPrevious = null;
        }

        private void WriteReanimTransform(ReanimatorTransform transform, Stream stream)
        {
            if (mAggressiveUsePlaceHolder
                && transform.mTransX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mTransY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mScaleX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mScaleY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mSkewX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mSkewY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mFrame == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mAlpha == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
                && transform.mFont == null
                && transform.mImage == null
                && transform.mText == null)
            {
                stream.WriteUInt8((byte)ReanimOptimisationType.Placeholder);
            }
            else if (mPrevious != null && (transform.mTransX == mPrevious.mTransX
                && transform.mTransY == mPrevious.mTransY
                && transform.mScaleX == mPrevious.mScaleX
                && transform.mScaleY == mPrevious.mScaleY
                && transform.mSkewX == mPrevious.mSkewX
                && transform.mSkewY == mPrevious.mSkewY
                && transform.mFrame == mPrevious.mFrame
                && transform.mAlpha == mPrevious.mAlpha
                && transform.mFont == mPrevious.mFont
                && transform.mImage == mPrevious.mImage
                && transform.mText == mPrevious.mText))
            {
                stream.WriteUInt8((byte)ReanimOptimisationType.CopyPrevious);
            }
            else
            {
                stream.WriteUInt8((byte)ReanimOptimisationType.New);
                FastWriteString(transform.mFont, stream);
                FastWriteString(transform.mImage, stream);
                FastWriteString(transform.mText, stream);
                stream.WriteFloat32LE(transform.mAlpha);
                stream.WriteFloat32LE(transform.mFrame);
                stream.WriteFloat32LE(transform.mScaleX);
                stream.WriteFloat32LE(transform.mScaleY);
                stream.WriteFloat32LE(transform.mSkewX);
                stream.WriteFloat32LE(transform.mSkewY);
                stream.WriteFloat32LE(transform.mTransX);
                stream.WriteFloat32LE(transform.mTransY);
            }
            mPrevious = transform;
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
            WriteOne(content, "reanim", stream);
        }

        public ReanimatorDefinition Decode(Stream stream)
        {
            return ReadOne("reanim", stream);
        }
    }
}
