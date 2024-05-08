using System;
using System.IO;
using System.Xml;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public class XmlReanimCoder : IReanimCoder
    {
        public static XmlReanimCoder Shared { get; } = new XmlReanimCoder();

        public ReanimatorDefinition Decode(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Encode(ReanimatorDefinition content, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteElementString("doScale", ((byte)content.mDoScale).ToString());
                writer.WriteElementString("fps", content.mFPS.ToString("0.###"));
                ReanimatorTrack[]? tracks = content.mTracks;
                if (tracks != null && tracks.Length != 0)
                {
                    for (int i = 0; i < tracks.Length; i++)
                    {
                        WriteReanimTrack(tracks[i], writer);
                    }
                }
            }
        }

        private void WriteReanimTrack(ReanimatorTrack track, XmlWriter writer)
        {
            writer.WriteStartElement("track");
            writer.WriteElementString("name", track.mName);
            ReanimatorTransform[]? transforms = track.mTransforms;
            if (transforms != null && transforms.Length != 0)
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    WriteReanimTransform(transforms[i], writer);
                }
            }
            writer.WriteEndElement();
        }

        private void WriteReanimTransform(ReanimatorTransform transform, XmlWriter writer)
        {
            writer.WriteStartElement("t");
            if (transform.mTransX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("x", transform.mTransX.ToString("0.###"));
            }
            if (transform.mTransY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("y", transform.mTransY.ToString("0.###"));
            }
            if (transform.mSkewX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("kx", transform.mSkewX.ToString("0.###"));
            }
            if (transform.mSkewY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("ky", transform.mSkewY.ToString("0.###"));
            }
            if (transform.mScaleX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("sx", transform.mScaleX.ToString("0.###"));
            }
            if (transform.mScaleY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("sy", transform.mScaleY.ToString("0.###"));
            }
            if (transform.mFrame != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("f", transform.mFrame.ToString("0.###"));
            }
            if (transform.mAlpha != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("a", transform.mAlpha.ToString("0.###"));
            }
            if (transform.mImage != null)
            {
                writer.WriteElementString("i", transform.mImage);
            }
            if (transform.mFont != null)
            {
                writer.WriteElementString("font", transform.mFont);
            }
            if (transform.mText != null)
            {
                writer.WriteElementString("text", transform.mText);
            }
            writer.WriteEndElement();
        }
    }
}
