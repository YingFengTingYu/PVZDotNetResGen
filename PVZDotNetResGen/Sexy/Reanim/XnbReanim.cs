using PVZDotNetResGen.Utils.StreamHelper;
using PVZDotNetResGen.Utils.XnbContent;
using System.Diagnostics;
using System.IO;

namespace PVZDotNetResGen.Sexy.Reanim
{
    internal class XnbReanim : XnbBase<ReanimatorDefinition>
    {
        private enum ReanimOptimisationType
        {
            New,
            CopyPrevious,
            Placeholder
        }

        protected override string ReaderTypeString => "Sexy.TodLib.ReanimReader";

        private ReanimatorTransform? mPrevious;

        public override ReanimatorDefinition ReadContent(Stream stream, string originalAssetName, byte version)
        {
            mPrevious = null;
            ReanimatorDefinition reanimatorDefinition = new ReanimatorDefinition();
            reanimatorDefinition.mDoScale = (ReanimScaleType)stream.ReadUInt8();
            reanimatorDefinition.mFPS = stream.ReadFloat32LE();
            reanimatorDefinition.mTrackCount = stream.ReadInt32LE();
            reanimatorDefinition.mTracks = new ReanimatorTrack[reanimatorDefinition.mTrackCount];
            for (int i = 0; i < reanimatorDefinition.mTrackCount; i++)
            {
                reanimatorDefinition.mTracks[i] = ReadReanimTrack(stream);
            }
            mPrevious = null;
            return reanimatorDefinition;
        }

        private ReanimatorTrack ReadReanimTrack(Stream input)
        {
            ReanimatorTrack track = new ReanimatorTrack();
            track.mName = input.ReadString(input.ReadInt32LE() * 2, encoding: System.Text.Encoding.UTF8);
            track.mTransformCount = input.ReadInt32LE();
            track.mTransforms = new ReanimatorTransform[track.mTransformCount];
            for (int i = 0; i < track.mTransformCount; i++)
            {
                track.mTransforms[i] = ReadReanimTransform(input);
            }
            return track;
        }

        private ReanimatorTransform ReadReanimTransform(Stream input)
        {
            ReanimatorTransform transform = new ReanimatorTransform();
            ReanimOptimisationType reanimOptimisationType = (ReanimOptimisationType)input.ReadByte();
            if (reanimOptimisationType == ReanimOptimisationType.Placeholder)
            {
                transform.mTransX = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mTransY = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mScaleX = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mScaleY = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mSkewX = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mSkewY = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mFrame = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mAlpha = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
                transform.mFont = null;
                transform.mImage = null;
                transform.mText = null;
            }
            else if (reanimOptimisationType == ReanimOptimisationType.CopyPrevious)
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
            else
            {
                transform.mFont = input.ReadString(input.ReadInt32LE() * 2, encoding: System.Text.Encoding.UTF8);
                transform.mImage = input.ReadString(input.ReadInt32LE() * 2, encoding: System.Text.Encoding.UTF8);
                transform.mText = input.ReadString(input.ReadInt32LE() * 2, encoding: System.Text.Encoding.UTF8);
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

        public override void WriteContent(ReanimatorDefinition content, Stream stream, string originalAssetName, byte version)
        {
            throw new System.NotImplementedException();
        }
    }
}
