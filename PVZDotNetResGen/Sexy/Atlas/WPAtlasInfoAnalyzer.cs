using PVZDotNetResGen.Utils.JsonHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace PVZDotNetResGen.Sexy.Atlas
{
    public static class WPAtlasInfoAnalyzer
    {
        private static void IdLine(StreamReader sr, string line)
        {
            if (sr.ReadLine() != line)
            {
                throw new Exception();
            }
        }

        public static void UnpackAsJson(string csFilePath, string atlasFolderPath)
        {
            using (Stream stream = File.OpenRead(csFilePath))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (line != null)
                        {
                            if (line.StartsWith("        public override void Unpack") && line.EndsWith("AtlasImages()"))
                            {
                                List<SpriteItem> items = new List<SpriteItem>();
                                IdLine(sr, "        {");
                                IdLine(sr, "            UNPACK_INFO[] array = new UNPACK_INFO[]");
                                IdLine(sr, "            {");
                                string? nextLine;
                                while ((nextLine = sr.ReadLine()) != "            };" && nextLine != null)
                                {
                                    if (nextLine.StartsWith("            new UNPACK_INFO(AtlasResources."))
                                    {
                                        string data = nextLine["            new UNPACK_INFO(AtlasResources.".Length..];
                                        string[] unpack = data.Replace(", ", ",").Split(",");
                                        SpriteItem item = new()
                                        {
                                            mId = unpack[0],
                                            mX = int.Parse(unpack[1]),
                                            mY = int.Parse(unpack[2]),
                                            mWidth = int.Parse(unpack[3]),
                                            mHeight = int.Parse(unpack[4]),
                                            mRows = int.Parse(unpack[5]),
                                            mCols = int.Parse(unpack[6]),
                                            mAnim = Enum.Parse<AnimType>(unpack[7]["AnimType.AnimType_".Length..]),
                                            mFrameDelay = int.Parse(unpack[8]),
                                            mBeginDelay = int.Parse(unpack[9]),
                                            mEndDelay = int.Parse(unpack[10][..unpack[10].IndexOf(')')]),
                                        };
                                        items.Add(item);
                                    }
                                    else
                                    {
                                        throw new Exception(nextLine);
                                    }
                                }
                                nextLine = sr.ReadLine();
                                if (nextLine != null && nextLine.StartsWith("            mArrays[\"") && nextLine.EndsWith("\"] = array;"))
                                {
                                    string atlasName = nextLine["            mArrays[\"".Length..^"\"] = array;".Length];
                                    AOTJson.TrySerializeToFile(Path.Combine(atlasFolderPath, atlasName + ".json"), new AtlasJson { mAtlas = items });
                                }
                                else
                                {
                                    throw new Exception(nextLine);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Dictionary<string, (List<SpriteItem>, string)> UnpackAsDictionary(string csFilePath)
        {
            Dictionary<string, (List<SpriteItem>, string)> ans = [];
            using (Stream stream = File.OpenRead(csFilePath))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (line != null)
                        {
                            if (line.StartsWith("        public override void Unpack") && line.EndsWith("AtlasImages()"))
                            {
                                string atlasName = string.Empty;
                                List<SpriteItem> items = new List<SpriteItem>();
                                IdLine(sr, "        {");
                                IdLine(sr, "            UNPACK_INFO[] array = new UNPACK_INFO[]");
                                IdLine(sr, "            {");
                                string? nextLine;
                                while ((nextLine = sr.ReadLine()) != "            };" && nextLine != null)
                                {
                                    if (nextLine.StartsWith("            new UNPACK_INFO(AtlasResources."))
                                    {
                                        string data = nextLine["            new UNPACK_INFO(AtlasResources.".Length..];
                                        string[] unpack = data.Replace(", ", ",").Split(",");
                                        SpriteItem item = new()
                                        {
                                            mId = unpack[0],
                                            mX = int.Parse(unpack[1]),
                                            mY = int.Parse(unpack[2]),
                                            mWidth = int.Parse(unpack[3]),
                                            mHeight = int.Parse(unpack[4]),
                                            mRows = int.Parse(unpack[5]),
                                            mCols = int.Parse(unpack[6]),
                                            mAnim = Enum.Parse<AnimType>(unpack[7]["AnimType.AnimType_".Length..]),
                                            mFrameDelay = int.Parse(unpack[8]),
                                            mBeginDelay = int.Parse(unpack[9]),
                                            mEndDelay = int.Parse(unpack[10][..unpack[10].IndexOf(')')]),
                                        };
                                        items.Add(item);
                                    }
                                    else
                                    {
                                        throw new Exception(nextLine);
                                    }
                                }
                                int startIndex;
                                nextLine = sr.ReadLine();
                                if (nextLine != null && (startIndex = nextLine.IndexOf("            mArrays[\"")) != -1)
                                {
                                    nextLine = nextLine[(startIndex + "            mArrays[\"".Length)..];
                                    startIndex = nextLine.IndexOf('"');
                                    atlasName = nextLine[..startIndex];
                                }
                                nextLine = sr.ReadLine();
                                nextLine = sr.ReadLine();
                                nextLine = sr.ReadLine();
                                if (nextLine != null && (startIndex = nextLine.IndexOf("new Image(Resources.")) != -1)
                                {
                                    nextLine = nextLine[(startIndex + "new Image(Resources.".Length)..];
                                    startIndex = nextLine.IndexOf(',');
                                    nextLine = nextLine[..startIndex];
                                    ans.Add(nextLine, (items, atlasName));
                                }
                                else
                                {
                                    throw new Exception(nextLine);
                                }
                            }
                        }
                    }
                }
            }
            return ans;
        }

        public class AtlasJson : IJsonVersionCheckable
        {
            public static uint JsonVersion => 0;

            public required List<SpriteItem> mAtlas;
        }
    }
}
