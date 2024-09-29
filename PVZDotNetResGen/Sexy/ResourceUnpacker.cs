using PVZDotNetResGen.Sexy.Atlas;
using PVZDotNetResGen.Utils.Graphics;
using PVZDotNetResGen.Utils.Graphics.Bitmap;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using PathNoAtlasInfo = (string Path, string DestPath);
using PathAndAtlasInfo = (string Path, string DestPath, System.Collections.Generic.Dictionary<string, (System.Collections.Generic.List<PVZDotNetResGen.Sexy.Atlas.SpriteItem>, string)> Atlas);
using System.Diagnostics;
using PVZDotNetResGen.Utils.JsonHelper;
using PVZDotNetResGen.Sexy.Reanim;
using PVZDotNetResGen.Utils.XnbContent;
using Xabe.FFmpeg;
using PVZDotNetResGen.Sexy.Music;
using PVZDotNetResGen.Utils.Sure;

namespace PVZDotNetResGen.Sexy
{
    public class ResourceUnpacker(string contentFolderPath, string codeFolderPath, string unpackFolderPath)
    {
        private readonly string mContentFolderPath = contentFolderPath;
        private readonly string mCodeFolderPath = codeFolderPath;
        private readonly string mUnpackFolderPath = unpackFolderPath;
        private string mDefaultPath = "";
        private string mDefaultIdPrefix = "";
        private readonly List<ResBase> mProgramRes = [];
        private readonly List<ResBase> mSysFontRes = [];
        private readonly List<ResLocPair> mResLocs = [];
        private readonly List<string> mAbsentRes = [];
        private readonly HashSet<string> mIsAtlas = [];
        private readonly HashSet<string> mExistedPath = [];

        private class ResLocPair
        {
            public required string mResPath;
            public readonly List<string> mLocs = [];
            public Dictionary<string, (List<SpriteItem>, string)> mAtlasInfo = [];
        }

        public string GetContentPath(string path)
        {
            return Path.Combine(mContentFolderPath, path);
        }

        public string GetInfoPath(string path)
        {
            return Path.Combine(mUnpackFolderPath, "infos", path);
        }

        public string GetUnpackPath(string path)
        {
            return Path.Combine(mUnpackFolderPath, "resources", path);
        }

        public string GetUnpackMetaPath(string path)
        {
            return Path.Combine(mUnpackFolderPath, "resources", path + ".meta.json");
        }

        public string GetUnpackMetaPathForSubImage(string path)
        {
            return Path.Combine(mUnpackFolderPath, "resources", path + ".subimage.json");
        }

        public IEnumerator<bool> Update()
        {
            if (!Directory.Exists(mUnpackFolderPath))
            {
                Directory.CreateDirectory(mUnpackFolderPath);
            }
            string imagesPath = GetContentPath("images");
            foreach (string res in Directory.GetDirectories(imagesPath))
            {
                ResLocPair pair = new ResLocPair { mResPath = Path.GetFileName(res) };
                foreach (string loc in Directory.GetDirectories(res))
                {
                    pair.mLocs.Add(Path.GetFileName(loc));
                }
                pair.mAtlasInfo = WPAtlasInfoAnalyzer.UnpackAsDictionary(Path.Combine(mCodeFolderPath, "AtlasResources_" + pair.mResPath + ".cs"));
                foreach (var spritePair in pair.mAtlasInfo)
                {
                    mIsAtlas.Add(spritePair.Key);
                }
                mResLocs.Add(pair);
            }
            yield return false;
            // 加载xml并解包文件
            PackInfo packInfo = new PackInfo();
            packInfo.mResLocs = new List<ResLocInfo>(mResLocs.Count);
            for (int i = 0; i < mResLocs.Count; i++)
            {
                packInfo.mResLocs.Add(new ResLocInfo { mResPath = mResLocs[i].mResPath, mLocs = [.. mResLocs[i].mLocs] });
            }
            XmlDocument xmlDocResources = new XmlDocument();
            xmlDocResources.Load(Path.Combine(mContentFolderPath, "resources.xml"));
            yield return false;
            XmlNode? root = xmlDocResources.SelectSingleNode("/ResourceManifest");
            if (root != null)
            {
                XmlNodeList list = root.ChildNodes;
                for (int i = 0; i < list.Count; i++)
                {
                    XmlNode? node = list[i];
                    if (node != null && node.Name == "Resources")
                    {
                        string? groupId = node.Attributes?["id"]?.InnerText;
                        if (groupId != null)
                        {
                            XmlNodeList resList = node.ChildNodes;
                            for (int j = 0; j < resList.Count; j++)
                            {
                                XmlNode? resNode = resList[j];
                                if (resNode != null)
                                {
                                    switch (resNode.Name)
                                    {
                                        case "SetDefaults":
                                            ParseSetDefaults(resNode);
                                            break;
                                        case "Image":
                                            string imgId = mDefaultIdPrefix + resNode.Attributes?["id"]?.InnerText;
                                            if (mIsAtlas.Contains(imgId))
                                            {
                                                ParseAtlasResource(resNode, groupId);
                                            }
                                            else
                                            {
                                                ParseImageResource(resNode, groupId);
                                            }
                                            break;
                                        case "Sound":
                                            ParseSoundResource(resNode, groupId);
                                            break;
                                        case "Font":
                                            ParseFontResource(resNode, groupId);
                                            break;
                                    }
                                }
                            }
                            packInfo.mGroups.Add(groupId);
                            yield return false;
                        }
                    }
                }
            }
            // 处理动画和粒子特效
            string[] reanims = Directory.GetFiles(GetContentPath("reanim"), "*", SearchOption.AllDirectories);
            foreach (string reanim in reanims)
            {
                DoLoadReanim("LoadingReanims", reanim);
            }
            string[] particles = Directory.GetFiles(GetContentPath("particles"), "*", SearchOption.AllDirectories);
            foreach (string particle in particles)
            {
                DoLoadParticleAndTrail("LoadingParticles", particle);
            }
            // 处理txt
            string[] txts = Directory.GetFiles(mContentFolderPath, "LawnStrings_*.txt", SearchOption.TopDirectoryOnly);
            foreach (string txt in txts)
            {
                string unpackPath = GetUnpackPath(Path.GetFileName(txt));
                EnsureParentFolderExist(unpackPath);
                File.Copy(txt, unpackPath, true);
            }
            // 处理music
            string[] musics = Directory.GetFiles(GetContentPath("music"), "*.xnb", SearchOption.AllDirectories);
            foreach (string music in musics)
            {
                DoLoadMusic(music);
            }
            // 处理sys资源和program资源
            SureHelper.MakeSure(mProgramRes.Count == 0);
            SureHelper.MakeSure(mSysFontRes.Count == 0);
            if (mAbsentRes.Count != 0)
            {
                foreach (string absentRes in mAbsentRes)
                {
                    Console.WriteLine("File does not exist:" + absentRes);
                }
            }
            // 处理group
            string jsonPath = GetInfoPath("pack.json");
            EnsureParentFolderExist(jsonPath);
            AOTJson.TrySerializeToFile(jsonPath, packInfo);
        }

        private bool ParseAtlasResource(XmlNode theElement, string groupId)
        {
            if (ParseCommonResource(theElement, out AtlasRes? atlasRes, groupId, out string? path))
            {
                XmlAttributeCollection? attributes = theElement.Attributes;
                atlasRes.mWidth = 2048;
                atlasRes.mHeight = 2048;
                atlasRes.mExtrude = 1;
                atlasRes.mSurface = new PlatformSurfaceFormat { Default = SurfaceFormat.Color };
                atlasRes.mFormat = TextureFormat.Png;
                if (attributes != null)
                {
                    atlasRes.mNoPal = attributes["nopal"] != null;
                    atlasRes.mA4R4G4B4 = attributes["a4r4g4b4"] != null;
                    atlasRes.mDDSurface = attributes["ddsurface"] != null;
                    atlasRes.mNoBits = attributes["nobits"] != null;
                    atlasRes.mNoBits2D = attributes["nobits2d"] != null;
                    atlasRes.mNoBits3D = attributes["nobits3d"] != null;
                    atlasRes.mA8R8G8B8 = attributes["a8r8g8b8"] != null;
                    atlasRes.mR5G6B5 = attributes["r5g6b5"] != null;
                    atlasRes.mA1R5G5B5 = attributes["a1r5g5b5"] != null;
                    atlasRes.mMinSubdivide = attributes["minsubdivide"] != null;
                    atlasRes.mNoAlpha = attributes["noalpha"] != null;
                    atlasRes.mAlphaColor = 16777215U;
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "surface")
                        {
                            atlasRes.mSurface.Default = (SurfaceFormat)Enum.Parse(typeof(SurfaceFormat), current.InnerText, true);
                        }
                        else if (current.Name == "alphaimage")
                        {
                            atlasRes.mAlphaImage = mDefaultPath + current.InnerText;
                        }
                        else if (current.Name == "alphacolor")
                        {
                            atlasRes.mAlphaColor = Convert.ToUInt32(current.InnerText);
                        }
                        else if (current.Name == "variant")
                        {
                            atlasRes.mVariant = current.InnerText;
                        }
                        else if (current.Name == "alphagrid")
                        {
                            atlasRes.mAlphaGrid = current.InnerText;
                        }
                        else if (current.Name == "languageSpecific")
                        {
                            atlasRes.mLanguageSpecific = Convert.ToBoolean(current.InnerText);
                        }
                        else if (current.Name == "format")
                        {
                            atlasRes.mFormat = (TextureFormat)Enum.Parse(typeof(TextureFormat), current.InnerText, true);
                        }
                    }
                }
                if (path != null)
                {
                    DoLoadAtlas(atlasRes, path);
                }
                else
                {
                    mProgramRes.Add(atlasRes);
                }
                return true;
            }
            return false;
        }

        private bool ParseImageResource(XmlNode theElement, string groupId)
        {
            if (ParseCommonResource(theElement, out ImageRes? imageRes, groupId, out string? path))
            {
                XmlAttributeCollection? attributes = theElement.Attributes;
                imageRes.mSurface = new PlatformSurfaceFormat { Default = SurfaceFormat.Color };
                imageRes.mFormat = TextureFormat.Png;
                if (attributes != null)
                {
                    imageRes.mNoPal = attributes["nopal"] != null;
                    imageRes.mA4R4G4B4 = attributes["a4r4g4b4"] != null;
                    imageRes.mDDSurface = attributes["ddsurface"] != null;
                    imageRes.mNoBits = attributes["nobits"] != null;
                    imageRes.mNoBits2D = attributes["nobits2d"] != null;
                    imageRes.mNoBits3D = attributes["nobits3d"] != null;
                    imageRes.mA8R8G8B8 = attributes["a8r8g8b8"] != null;
                    imageRes.mR5G6B5 = attributes["r5g6b5"] != null;
                    imageRes.mA1R5G5B5 = attributes["a1r5g5b5"] != null;
                    imageRes.mMinSubdivide = attributes["minsubdivide"] != null;
                    imageRes.mNoAlpha = attributes["noalpha"] != null;
                    imageRes.mAlphaColor = 16777215U;
                    imageRes.mRows = 1;
                    imageRes.mCols = 1;
                    imageRes.mAnim = AnimType.None;
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "surface")
                        {
                            imageRes.mSurface.Default = (SurfaceFormat)Enum.Parse(typeof(SurfaceFormat), current.InnerText, true);
                        }
                        else if (current.Name == "alphaimage")
                        {
                            imageRes.mAlphaImage = mDefaultPath + current.InnerText;
                        }
                        else if (current.Name == "alphacolor")
                        {
                            imageRes.mAlphaColor = Convert.ToUInt32(current.InnerText);
                        }
                        else if (current.Name == "variant")
                        {
                            imageRes.mVariant = current.InnerText;
                        }
                        else if (current.Name == "alphagrid")
                        {
                            imageRes.mAlphaGrid = current.InnerText;
                        }
                        else if (current.Name == "rows")
                        {
                            imageRes.mRows = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "cols")
                        {
                            imageRes.mCols = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "languageSpecific")
                        {
                            imageRes.mLanguageSpecific = Convert.ToBoolean(current.InnerText);
                        }
                        else if (current.Name == "format")
                        {
                            imageRes.mFormat = (TextureFormat)Enum.Parse(typeof(TextureFormat), current.InnerText, true);
                        }
                        else if (current.Name == "anim")
                        {
                            switch (current.InnerText)
                            {
                                case "none":
                                    imageRes.mAnim = AnimType.None;
                                    break;
                                case "once":
                                    imageRes.mAnim = AnimType.Once;
                                    break;
                                case "loop":
                                    imageRes.mAnim = AnimType.Loop;
                                    break;
                                case "pingpong":
                                    imageRes.mAnim = AnimType.PingPong;
                                    break;
                            }
                        }
                        else if (current.Name == "framedelay")
                        {
                            imageRes.mFrameDelay = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "begindelay")
                        {
                            imageRes.mBeginDelay = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "enddelay")
                        {
                            imageRes.mEndDelay = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "perframedelay")
                        {
                            imageRes.mPerFrameDelay = current.InnerText;
                        }
                        else if (current.Name == "framemap")
                        {
                            imageRes.mFrameMap = current.InnerText;
                        }
                        else if (current.Name == "invscale")
                        {
                            imageRes.mInvScale = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "x")
                        {
                            imageRes.mX = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "y")
                        {
                            imageRes.mY = Convert.ToInt32(current.InnerText);
                        }
                    }
                }
                if (path != null)
                {
                    DoLoadImage(imageRes, path);
                }
                else
                {
                    mProgramRes.Add(imageRes);
                }
                return true;
            }
            return false;
        }

        private bool ParseSoundResource(XmlNode theElement, string groupId)
        {
            if (ParseCommonResource(theElement, out SoundRes? soundRes, groupId, out string? path))
            {
                soundRes.mVolume = -1.0;
                soundRes.mPan = 0;
                XmlAttributeCollection? attributes = theElement.Attributes;
                if (attributes != null)
                {
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "volume")
                        {
                            soundRes.mVolume = Convert.ToDouble(current.InnerText);
                        }
                        if (current.Name == "pan")
                        {
                            soundRes.mPan = Convert.ToInt32(current.InnerText);
                        }
                    }
                }
                if (path != null)
                {
                    DoLoadSound(soundRes, path);
                }
                else
                {
                    mProgramRes.Add(soundRes);
                }
                return true;
            }
            return false;
        }

        private bool ParseFontResource(XmlNode theElement, string groupId)
        {
            if (ParseCommonResource(theElement, out FontRes? fontRes, groupId, out string? path))
            {
                XmlAttributeCollection? attributes = theElement.Attributes;
                if (attributes != null)
                {
                    fontRes.mSize = -1;
                    fontRes.mBold = false;
                    fontRes.mItalic = false;
                    fontRes.mShadow = false;
                    fontRes.mUnderline = false;
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "tags")
                        {
                            fontRes.mTags = current.InnerText;
                        }
                        else if (current.Name == "isDefault")
                        {
                            fontRes.mIsDefault = true;
                        }
                        else if (current.Name == "truetype")
                        {
                            fontRes.mTrueType = true;
                        }
                        else if (current.Name == "size")
                        {
                            fontRes.mSize = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "bold")
                        {
                            fontRes.mBold = true;
                        }
                        else if (current.Name == "italic")
                        {
                            fontRes.mItalic = true;
                        }
                        else if (current.Name == "shadow")
                        {
                            fontRes.mShadow = true;
                        }
                        else if (current.Name == "underline")
                        {
                            fontRes.mUnderline = true;
                        }
                        else if (current.Name == "stroked")
                        {
                            fontRes.mStroke = Convert.ToInt32(current.InnerText);
                        }
                    }
                }
                bool sysFont = false;
                if (path != null)
                {
                    sysFont = path[..5] == "!sys:";
                }
                if (sysFont)
                {
                    mSysFontRes.Add(fontRes);
                }
                else if (path != null)
                {
                    DoLoadFont(fontRes, path);
                }
                else
                {
                    mProgramRes.Add(fontRes);
                }
                return true;
            }
            return false;
        }

        private bool ParseCommonResource<T>(XmlNode theElement, [NotNullWhen(true)] out T? theRes, string groupId, out string? path) where T : ResBase, new()
        {
            XmlAttributeCollection? attributes = theElement.Attributes;
            if (attributes == null)
            {
                theRes = null;
                path = null;
                return false;
            }
            theRes = new T
            {
                mId = mDefaultIdPrefix + attributes["id"]?.InnerText,
                mGroup = groupId,
            };
            theRes.mGroup = groupId;
            string? pathRaw = attributes["path"]?.InnerText;
            if (pathRaw == null)
            {
                path = pathRaw;
            }
            else if (pathRaw.Length > 0 && pathRaw[0] == '!')
            {
                path = pathRaw;
                if (pathRaw == "!program")
                {
                    path = null;
                }
            }
            else
            {
                path = mDefaultPath + pathRaw;
            }
            string? unloadGroup = attributes["unloadGroup"]?.InnerText;
            if (unloadGroup != null)
            {
                theRes.mUnloadGroup = Convert.ToInt32(unloadGroup);
            }
            return true;
        }

        private bool ParseSetDefaults(XmlNode theElement)
        {
            XmlAttributeCollection? attributes = theElement.Attributes;
            if (attributes != null)
            {
                foreach (XmlAttribute current in attributes)
                {
                    if (current.Name == "path")
                    {
                        if (string.IsNullOrEmpty(current.Value))
                        {
                            mDefaultPath = "";
                        }
                        else
                        {
                            mDefaultPath = current.Value + "/";
                        }
                    }
                    if (current.Name == "idprefix")
                    {
                        mDefaultIdPrefix = current.Value;
                    }
                }
            }
            return true;
        }

        private void LoadImagePaths(string path, bool languageSpecific, List<PathNoAtlasInfo> pathList)
        {
            string? folderName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            if (languageSpecific)
            {
                if (folderName != null)
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        foreach (string loc in pair.mLocs)
                        {
                            pathList.Add((Path.Combine(folderName, pair.mResPath, loc, fileName), Path.Combine(folderName, pair.mResPath, loc, fileName)));
                        }
                    }
                }
                else
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        foreach (string loc in pair.mLocs)
                        {
                            pathList.Add((Path.Combine(pair.mResPath, loc, fileName), Path.Combine(pair.mResPath, loc, fileName)));
                        }
                    }
                }
            }
            else
            {
                if (folderName != null)
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        pathList.Add((Path.Combine(folderName, pair.mResPath, fileName), Path.Combine(folderName, pair.mResPath, "universal", fileName)));
                    }
                }
                else
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        pathList.Add((Path.Combine(pair.mResPath, fileName), Path.Combine(pair.mResPath, "universal", fileName)));
                    }
                }
            }
        }

        private void LoadAtlasPaths(string path, bool languageSpecific, List<PathAndAtlasInfo> pathList)
        {
            string? folderName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            if (languageSpecific)
            {
                if (folderName != null)
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        foreach (string loc in pair.mLocs)
                        {
                            pathList.Add((Path.Combine(folderName, pair.mResPath, loc, fileName), Path.Combine("atlases", pair.mResPath, loc, fileName), pair.mAtlasInfo));
                        }
                    }
                }
                else
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        foreach (string loc in pair.mLocs)
                        {
                            pathList.Add((Path.Combine(pair.mResPath, loc, fileName), Path.Combine("atlases", pair.mResPath, loc, fileName), pair.mAtlasInfo));
                        }
                    }
                }
            }
            else
            {
                if (folderName != null)
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        pathList.Add((Path.Combine(folderName, pair.mResPath, fileName), Path.Combine("atlases", pair.mResPath, "universal", fileName), pair.mAtlasInfo));
                    }
                }
                else
                {
                    foreach (ResLocPair pair in mResLocs)
                    {
                        pathList.Add((Path.Combine(pair.mResPath, fileName), Path.Combine("atlases", pair.mResPath, "universal", fileName), pair.mAtlasInfo));
                    }
                }
            }
        }

        private static string LoadImageExtension(string path, TextureFormat format)
        {
            if (format == TextureFormat.Content)
            {
                return path + ".xnb";
            }
            return path + "." + format.ToString().ToLower();
        }

        private static string LoadXnbExtension(string path)
        {
            return path + ".xnb";
        }

        private void EnsureParentFolderExist(string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private void EnsureThisFolderExist(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        private static string RemoveContentProfix(string path)
        {
            const string CONTENT_PROFIX = "Content/";
            if (path.StartsWith(CONTENT_PROFIX))
            {
                return path[CONTENT_PROFIX.Length..];
            }
            return path;
        }

        private static IDisposableBitmap DecodeImageFromPath(string path, TextureFormat format)
        {
            if (format == TextureFormat.Content)
            {
                using (FileStream xnbStream = File.OpenRead(path))
                {
                    return (IDisposableBitmap)XnbHelper.Decode(Path.GetFileName(path), xnbStream).PrimaryResource;
                }
            }
            return new StbBitmap(path);
        }

        private static int GetCloserPOT(int value)
        {
            int s = 1;
            while (s < value)
            {
                s <<= 1;
            }
            return s;
        }

        private void DoLoadAtlas(AtlasRes atlasRes, string path)
        {
            List<PathAndAtlasInfo> pathList = new List<PathAndAtlasInfo>();
            LoadAtlasPaths(path, atlasRes.mLanguageSpecific, pathList);
            foreach (PathAndAtlasInfo info in pathList)
            {
                string atlasPath = info.DestPath;
                string imgContentPath = GetContentPath(LoadImageExtension(info.Path, atlasRes.mFormat));
                if (File.Exists(imgContentPath))
                {
                    EnsureThisFolderExist(GetUnpackPath(atlasPath));
                    using (IDisposableBitmap bitmap = DecodeImageFromPath(imgContentPath, atlasRes.mFormat))
                    {
                        RefBitmap bitmapRef = bitmap.AsRefBitmap();
                        atlasRes.mWidth = GetCloserPOT(bitmapRef.Width);
                        atlasRes.mHeight = GetCloserPOT(bitmapRef.Height);
                        atlasRes.mAtlasName = info.Atlas[atlasRes.mId!].Item2;
                        foreach (SpriteItem spirits in info.Atlas[atlasRes.mId!].Item1)
                        {
                            string thisImgPath = Path.Combine(atlasPath, spirits.mId.ToLower());
                            string thisImgExPath = LoadImageExtension(thisImgPath, TextureFormat.Png);
                            SubImageRes subImageRes = new SubImageRes
                            {
                                mGroup = atlasRes.mGroup,
                                mId = spirits.mId,
                                mParent = atlasRes.mId!,
                                mRows = spirits.mRows,
                                mCols = spirits.mCols,
                                mAnim = spirits.mAnim,
                                mFrameDelay = spirits.mFrameDelay,
                                mBeginDelay = spirits.mBeginDelay,
                                mEndDelay = spirits.mEndDelay,
                            };
                            // 保存图片
                            using (MemoryPoolBitmap subBitmap = new MemoryPoolBitmap(spirits.mWidth, spirits.mHeight))
                            {
                                int width = spirits.mWidth;
                                int height = spirits.mHeight;
                                bool warning = false;
                                if ((spirits.mX + width) > bitmapRef.Width)
                                {
                                    width = bitmapRef.Width - spirits.mX;
                                    warning = true;
                                }
                                if ((spirits.mY + height) > bitmapRef.Height)
                                {
                                    height = bitmapRef.Height - spirits.mY;
                                    warning = true;
                                }
                                if (warning)
                                {
                                    Console.WriteLine("This sub image may need repairing: {0}", spirits.mId);
                                }
                                bitmapRef.CopyTo(subBitmap.AsRefBitmap(), spirits.mX, spirits.mY, 0, 0, width, height);
                                subBitmap.SaveAsPng(GetUnpackPath(thisImgExPath));
                            }
                            subImageRes.mDiskFormat = DiskFormat.Png;
                            AOTJson.TrySerializeToFile<ResBase>(GetUnpackMetaPathForSubImage(thisImgPath), subImageRes);
                        }
                    }
                    AOTJson.TrySerializeListToFile<ResBase>(GetUnpackMetaPath(atlasPath), [atlasRes]);
                }
                else
                {
                    mAbsentRes.Add(path);
                }
            }
        }

        private void DoLoadImage(ImageRes imageRes, string path)
        {
            List<PathNoAtlasInfo> pathList = new List<PathNoAtlasInfo>();
            LoadImagePaths(path, imageRes.mLanguageSpecific == true, pathList);
            foreach (PathNoAtlasInfo info in pathList)
            {
                string imgContentPath = GetContentPath(LoadImageExtension(info.Path, imageRes.mFormat));
                if (File.Exists(imgContentPath))
                {
                    string imgPath = LoadImageExtension(info.DestPath, TextureFormat.Png);
                    string imgUnpackPath = GetUnpackPath(imgPath);
                    EnsureParentFolderExist(imgUnpackPath);
                    using (IDisposableBitmap bitmap = DecodeImageFromPath(imgContentPath, imageRes.mFormat))
                    {
                        bitmap.SaveAsPng(imgUnpackPath);
                    }
                    imageRes.mDiskFormat = DiskFormat.Png;
                    AOTJson.TrySerializeListToFile<ResBase>(GetUnpackMetaPath(info.DestPath), [imageRes]);
                }
                else
                {
                    mAbsentRes.Add(path);
                }
            }
        }

        private void DoLoadReanim(string groupName, string path)
        {
            string reanimPath = Path.Combine("reanim", Path.GetFileNameWithoutExtension(path));
            if (File.Exists(path))
            {
                string reanimUnpackPath = GetUnpackPath(reanimPath + ".reanim");
                EnsureParentFolderExist(reanimUnpackPath);
                ReanimatorDefinition reanim = XnbReanimCoder.Shared.Decode(path);
                XmlReanimCoder.Shared.Encode(reanim, reanimUnpackPath);
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }

        private void DoLoadParticleAndTrail(string groupName, string path)
        {
            string particlePath = Path.Combine("particles", Path.GetFileName(path));
            if (File.Exists(path))
            {
                string particleUnpackPath = GetUnpackPath(particlePath);
                EnsureParentFolderExist(particleUnpackPath);
                File.Copy(path, particleUnpackPath, true);
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }

        private void DoLoadMusic(string path)
        {
            XnbContent content;
            using (Stream xnbStream = File.OpenRead(path))
            {
                content = XnbHelper.Decode(Path.GetFileNameWithoutExtension(path), xnbStream);
            }
            string unpackPath = Path.ChangeExtension(GetUnpackPath(Path.Combine("music", Path.GetFileName(path))), ".wav");
            EnsureParentFolderExist(unpackPath);
            var snippet = FFmpeg.Conversions.FromSnippet.Convert(Path.Combine(Path.GetDirectoryName(path)!, ((Song)content.PrimaryResource).Name!), unpackPath).Result;
            IConversionResult result = snippet.Start().Result;
        }

        private void DoLoadSound(SoundRes soundRes, string path)
        {
            string soundPath = LoadXnbExtension(path);
            string soundContentPath = GetContentPath(soundPath);
            if (File.Exists(soundContentPath))
            {
                string soundUnpackPath = GetUnpackPath(soundPath);
                EnsureParentFolderExist(soundUnpackPath);
                File.Copy(soundContentPath, soundUnpackPath, true);
                soundRes.mDiskFormat = DiskFormat.Xnb;
                string metaPath = GetUnpackMetaPath(soundPath);
                bool finished = false;
                if (mExistedPath.Contains(soundUnpackPath))
                {
                    Console.WriteLine("Repeat sound res:" + path);
                    List<ResBase?>? repeatRes = AOTJson.TryDeserializeListFromFile<ResBase>(metaPath);
                    if (repeatRes != null)
                    {
                        repeatRes.Add(soundRes);
                        AOTJson.TrySerializeListToFile<ResBase>(metaPath, repeatRes);
                        finished = true;
                    }
                }
                else
                {
                    mExistedPath.Add(soundUnpackPath);
                }
                if (!finished)
                {
                    AOTJson.TrySerializeListToFile<ResBase>(metaPath, [soundRes]);
                }
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }

        private void DoLoadFont(FontRes fontRes, string path)
        {
            string fontPath = RemoveContentProfix(path);
            string fontContentPath = GetContentPath(fontPath);
            if (File.Exists(fontContentPath))
            {
                string fontUnpackPath = GetUnpackPath(fontPath);
                EnsureParentFolderExist(fontUnpackPath);
                File.Copy(fontContentPath, fontUnpackPath, true);
                fontRes.mDiskFormat = DiskFormat.None;
                string metaPath = GetUnpackMetaPath(fontPath);
                bool finished = false;
                if (mExistedPath.Contains(fontUnpackPath))
                {
                    Console.WriteLine("Repeat font res:" + path);
                    List<ResBase?>? repeatRes = AOTJson.TryDeserializeListFromFile<ResBase>(metaPath);
                    if (repeatRes != null)
                    {
                        repeatRes.Add(fontRes);
                        AOTJson.TrySerializeListToFile<ResBase>(metaPath, repeatRes);
                        finished = true;
                    }
                }
                else
                {
                    mExistedPath.Add(fontUnpackPath);
                }
                if (!finished)
                {
                    AOTJson.TrySerializeListToFile<ResBase>(metaPath, [fontRes]);
                }
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }
    }
}
