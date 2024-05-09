using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public class XmlReanimCoder : IReanimCoder
    {
        public static XmlReanimCoder Shared { get; } = new XmlReanimCoder();

        public CultureInfo mCulture = new CultureInfo("us");

        public ReanimatorDefinition Decode(Stream stream)
        {
            ReanimatorDefinition reanim = new ReanimatorDefinition();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "doScale")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                reanim.mDoScale = (ReanimScaleType)sbyte.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "fps")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                reanim.mFPS = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "track")
                        {
                            reanim.mTracks.Add(ReadReanimTrack(reader));
                        }
                    }
                }
            }
            return reanim;
        }

        private ReanimatorTrack ReadReanimTrack(XmlReader reader)
        {
            ReanimatorTrack track = new ReanimatorTrack();
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "name")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                track.mName = reader.Value;
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "t")
                        {
                            track.mTransforms.Add(ReadReanimTransform(reader));
                        }
                    }
                }
            }
            return track;
        }

        private ReanimatorTransform ReadReanimTransform(XmlReader reader)
        {
            ReanimatorTransform transform = new ReanimatorTransform();
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "x")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mTransX = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "y")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mTransY = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "kx")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mSkewX = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "ky")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mSkewY = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "sx")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mScaleX = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "sy")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mScaleY = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "f")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mFrame = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "a")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mAlpha = float.Parse(reader.Value, mCulture);
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "i")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mImage = reader.Value;
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "font")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mFont = reader.Value;
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "text")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.Text);
                                transform.mText = reader.Value;
                                Debug.Assert(reader.Read());
                                Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                    }
                }
            }
            return transform;
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
                writer.WriteElementString("doScale", ((sbyte)content.mDoScale).ToString());
                writer.WriteElementString("fps", content.mFPS.ToString("0.###", mCulture));
                List<ReanimatorTrack> tracks = content.mTracks;
                for (int i = 0; i < tracks.Count; i++)
                {
                    WriteReanimTrack(tracks[i], writer);
                }
            }
        }

        private void WriteReanimTrack(ReanimatorTrack track, XmlWriter writer)
        {
            writer.WriteStartElement("track");
            writer.WriteElementString("name", track.mName);
            List<ReanimatorTransform> transforms = track.mTransforms;
            for (int i = 0; i < transforms.Count; i++)
            {
                WriteReanimTransform(transforms[i], writer);
            }
            writer.WriteEndElement();
        }

        private void WriteReanimTransform(ReanimatorTransform transform, XmlWriter writer)
        {
            writer.WriteStartElement("t");
            if (transform.mTransX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("x", transform.mTransX.ToString("0.###", mCulture));
            }
            if (transform.mTransY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("y", transform.mTransY.ToString("0.###", mCulture));
            }
            if (transform.mSkewX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("kx", transform.mSkewX.ToString("0.###", mCulture));
            }
            if (transform.mSkewY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("ky", transform.mSkewY.ToString("0.###", mCulture));
            }
            if (transform.mScaleX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("sx", transform.mScaleX.ToString("0.###", mCulture));
            }
            if (transform.mScaleY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("sy", transform.mScaleY.ToString("0.###", mCulture));
            }
            if (transform.mFrame != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("f", transform.mFrame.ToString("0.###", mCulture));
            }
            if (transform.mAlpha != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
            {
                writer.WriteElementString("a", transform.mAlpha.ToString("0.###", mCulture));
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
