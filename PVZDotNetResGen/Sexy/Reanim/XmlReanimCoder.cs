using PVZDotNetResGen.Utils.Sure;
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
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                reanim.DoScale = (ReanimScaleType)sbyte.Parse(reader.Value);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "fps")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                reanim.Fps = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "track")
                        {
                            reanim.Tracks.Add(ReadReanimTrack(reader));
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
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                track.Name = reader.Value;
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "t")
                        {
                            track.Transforms.Add(ReadReanimTransform(reader));
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
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.TransX = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "y")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.TransY = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "kx")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.SkewX = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "ky")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.SkewY = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "sx")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.ScaleX = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "sy")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.ScaleY = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "f")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.Frame = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "a")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.Alpha = float.Parse(reader.Value, CultureInfo.InvariantCulture);
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "i")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.Image = reader.Value;
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "font")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.Font = reader.Value;
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
                            }
                        }
                        else if (reader.Name == "text")
                        {
                            if (!reader.IsEmptyElement)
                            {
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.Text);
                                transform.Text = reader.Value;
                                SureHelper.MakeSure(reader.Read());
                                SureHelper.MakeSure(reader.NodeType == XmlNodeType.EndElement);
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
                ConformanceLevel = ConformanceLevel.Fragment
            };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteElementString("doScale", ((sbyte)content.DoScale).ToString());
                writer.WriteRaw("\r\n");
                writer.WriteElementString("fps", content.Fps.ToString("0.###", CultureInfo.InvariantCulture));
                writer.WriteRaw("\r\n");
                List<ReanimatorTrack> tracks = content.Tracks;
                for (int i = 0; i < tracks.Count; i++)
                {
                    WriteReanimTrack(tracks[i], writer);
                }
            }
        }

        private void WriteReanimTrack(ReanimatorTrack track, XmlWriter writer)
        {
            writer.WriteStartElement("track");
            writer.WriteRaw("\r\n");
            writer.WriteElementString("name", track.Name);
            writer.WriteRaw("\r\n");
            List<ReanimatorTransform> transforms = track.Transforms;
            for (int i = 0; i < transforms.Count; i++)
            {
                WriteReanimTransform(transforms[i], writer);
            }

            writer.WriteEndElement();
            writer.WriteRaw("\r\n");
        }

        private void WriteReanimTransform(ReanimatorTransform transform, XmlWriter writer)
        {
            if (transform.IsNull())
            {
                writer.WriteRaw("<t></t>");
            }
            else
            {
                writer.WriteStartElement("t");
                if (transform.TransX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("x", transform.TransX.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.TransY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("y", transform.TransY.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.SkewX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("kx", transform.SkewX.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.SkewY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("ky", transform.SkewY.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.ScaleX != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("sx", transform.ScaleX.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.ScaleY != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("sy", transform.ScaleY.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.Frame != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("f", transform.Frame.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.Alpha != ReanimHelper.DEFAULT_FIELD_PLACEHOLDER)
                {
                    writer.WriteElementString("a", transform.Alpha.ToString("0.###", CultureInfo.InvariantCulture));
                }

                if (transform.Image != null)
                {
                    writer.WriteElementString("i", transform.Image);
                }

                if (transform.Font != null)
                {
                    writer.WriteElementString("font", transform.Font);
                }

                if (transform.Text != null)
                {
                    writer.WriteElementString("text", transform.Text);
                }

                writer.WriteEndElement();
            }

            writer.WriteRaw("\r\n");
        }
    }
}
