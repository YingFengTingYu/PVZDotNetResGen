using System;
using System.IO;
using System.Xml;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public class XmlReanimCoder : IReanimCoder
    {
        public ReanimatorDefinition Decode(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Encode(ReanimatorDefinition content, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true
            };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteElementString("doScale", content.mDoScale.ToString());
                writer.WriteElementString("fps", content.mFPS.ToString());
                ReanimatorTrack[]? tracks = content.mTracks;
                if (tracks != null && tracks.Length != 0)
                {
                    for (int i = 0; i < tracks.Length; i++)
                    {
                        WriteReanimTrack(tracks[i], writer);
                    }
                }
                writer.WriteEndDocument();
            }
        }

        private void WriteReanimTrack(ReanimatorTrack track, XmlWriter writer)
        {
            ReanimatorTransform previous = new ReanimatorTransform();
            previous.mTransX = 0.0f;
            previous.mTransY = 0.0f;
            previous.mSkewX = 0.0f;
            previous.mSkewY = 0.0f;
            previous.mScaleX = 1.0f;
            previous.mScaleY = 1.0f;
            previous.mFrame = 0.0f;
            previous.mAlpha = 1.0f;
            previous.mImage = null;
            previous.mFont = null;
            previous.mText = null;
            writer.WriteStartElement("track");
            writer.WriteElementString("name", track.mName);
            ReanimatorTransform[]? transforms = track.mTransforms;
            if (transforms != null && transforms.Length != 0)
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    WriteReanimTransform(transforms[i], writer, previous);
                }
            }
            writer.WriteEndElement();
        }

        private void WriteReanimTransform(ReanimatorTransform transform, XmlWriter writer, ReanimatorTransform previous)
        {
            writer.WriteStartElement("t");
            if (transform.mTransX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mTransX != previous.mTransX)
            {
                writer.WriteElementString("x", transform.mTransX.ToString());
                previous.mTransX = transform.mTransX;
            }
            if (transform.mTransY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mTransY != previous.mTransY)
            {
                writer.WriteElementString("y", transform.mTransY.ToString());
                previous.mTransY = transform.mTransY;
            }
            if (transform.mSkewX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mSkewX != previous.mSkewX)
            {
                writer.WriteElementString("kx", transform.mSkewX.ToString());
                previous.mSkewX = transform.mSkewX;
            }
            if (transform.mSkewY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mSkewY != previous.mSkewY)
            {
                writer.WriteElementString("ky", transform.mSkewY.ToString());
                previous.mSkewY = transform.mSkewY;
            }
            if (transform.mScaleX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mScaleX != previous.mScaleX)
            {
                writer.WriteElementString("sx", transform.mScaleX.ToString());
                previous.mScaleX = transform.mScaleX;
            }
            if (transform.mScaleY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mScaleY != previous.mScaleY)
            {
                writer.WriteElementString("sy", transform.mScaleY.ToString());
                previous.mScaleY = transform.mScaleY;
            }
            if (transform.mFrame != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mFrame != previous.mFrame)
            {
                writer.WriteElementString("f", transform.mFrame.ToString());
                previous.mFrame = transform.mFrame;
            }
            if (transform.mAlpha != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER && transform.mAlpha != previous.mAlpha)
            {
                writer.WriteElementString("a", transform.mAlpha.ToString());
                previous.mAlpha = transform.mAlpha;
            }
            if (transform.mImage != null && transform.mImage != previous.mImage)
            {
                writer.WriteElementString("i", transform.mImage);
                previous.mImage = transform.mImage;
            }
            if (transform.mFont != null && transform.mFont != previous.mFont)
            {
                writer.WriteElementString("font", transform.mFont);
                previous.mFont = transform.mFont;
            }
            if (transform.mText != null && transform.mText != previous.mText)
            {
                writer.WriteElementString("text", transform.mText);
                previous.mText = transform.mText;
            }
            writer.WriteEndElement();
        }
    }
}
