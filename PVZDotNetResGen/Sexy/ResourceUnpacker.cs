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
using PVZDotNetResGen.Sexy.Image;
using PVZDotNetResGen.Sexy.Reanim;

namespace PVZDotNetResGen.Sexy
{
    public class ResourceUnpacker(string contentFolderPath, string codeFolderPath, string unpackFolderPath)
    {
        private readonly string mContentFolderPath = contentFolderPath;
        private readonly string mCodeFolderPath = codeFolderPath;
        private readonly string mUnpackFolderPath = unpackFolderPath;
        private string mDefaultPath = "/";
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
            packInfo.mGroups.Add("LoadingReanims");
            string[] reanims = Directory.GetFiles(GetContentPath("reanim"), "*", SearchOption.AllDirectories);
            foreach (string reanim in reanims)
            {
                DoLoadReanim("LoadingReanims", reanim);
            }
            packInfo.mGroups.Add("LoadingParticles");
            packInfo.mGroups.Add("LoadingTrails");
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
            string[] musics = Directory.GetFiles(GetContentPath("music"), "*", SearchOption.AllDirectories);
            foreach (string music in musics)
            {
                string unpackPath = GetUnpackPath(Path.Combine("music", Path.GetFileName(music)));
                EnsureParentFolderExist(unpackPath);
                File.Copy(music, unpackPath, true);
            }
            // 处理sys资源和program资源
            Debug.Assert(mProgramRes.Count == 0);
            Debug.Assert(mSysFontRes.Count == 0);
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
            if (ParseCommonResource(theElement, out ResBase<AtlasRes>? atlasRes, groupId, out string? path))
            {
                XmlAttributeCollection? attributes = theElement.Attributes;
                atlasRes.mUniversalProp.mWidth = 2048;
                atlasRes.mUniversalProp.mHeight = 2048;
                atlasRes.mUniversalProp.mExtrude = 1;
                atlasRes.mUniversalProp.mSurface = SurfaceFormat.Color;
                (atlasRes.mPCDXProp ??= new AtlasRes()).mSurface = SurfaceFormat.Dxt5;
                (atlasRes.mPCGLProp ??= new AtlasRes()).mSurface = SurfaceFormat.Dxt5;
                (atlasRes.mAndroidProp ??= new AtlasRes()).mSurface = SurfaceFormat.Rgba8Etc2;
                (atlasRes.mIOSProp ??= new AtlasRes()).mSurface = SurfaceFormat.Rgba8Etc2;
                if (attributes != null)
                {
                    atlasRes.mUniversalProp.mNoPal = attributes["nopal"] != null;
                    atlasRes.mUniversalProp.mA4R4G4B4 = attributes["a4r4g4b4"] != null;
                    atlasRes.mUniversalProp.mDDSurface = attributes["ddsurface"] != null;
                    atlasRes.mUniversalProp.mNoBits = attributes["nobits"] != null;
                    atlasRes.mUniversalProp.mNoBits2D = attributes["nobits2d"] != null;
                    atlasRes.mUniversalProp.mNoBits3D = attributes["nobits3d"] != null;
                    atlasRes.mUniversalProp.mA8R8G8B8 = attributes["a8r8g8b8"] != null;
                    atlasRes.mUniversalProp.mR5G6B5 = attributes["r5g6b5"] != null;
                    atlasRes.mUniversalProp.mA1R5G5B5 = attributes["a1r5g5b5"] != null;
                    atlasRes.mUniversalProp.mMinSubdivide = attributes["minsubdivide"] != null;
                    atlasRes.mUniversalProp.mNoAlpha = attributes["noalpha"] != null;
                    atlasRes.mUniversalProp.mAlphaColor = 16777215U;
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "surface")
                        {
                            atlasRes.mUniversalProp.mSurface = (SurfaceFormat)Enum.Parse(typeof(SurfaceFormat), current.InnerText, true);
                        }
                        else if (current.Name == "alphaimage")
                        {
                            atlasRes.mUniversalProp.mAlphaImage = mDefaultPath + current.InnerText;
                        }
                        else if (current.Name == "alphacolor")
                        {
                            atlasRes.mUniversalProp.mAlphaColor = Convert.ToUInt32(current.InnerText);
                        }
                        else if (current.Name == "variant")
                        {
                            atlasRes.mUniversalProp.mVariant = current.InnerText;
                        }
                        else if (current.Name == "alphagrid")
                        {
                            atlasRes.mUniversalProp.mAlphaGrid = current.InnerText;
                        }
                        else if (current.Name == "languageSpecific")
                        {
                            atlasRes.mUniversalProp.mLanguageSpecific = Convert.ToBoolean(current.InnerText);
                        }
                        else if (current.Name == "format")
                        {
                            atlasRes.mUniversalProp.mFormat = (TextureFormat)Enum.Parse(typeof(TextureFormat), current.InnerText, true);
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
            if (ParseCommonResource(theElement, out ResBase<ImageRes>? imageRes, groupId, out string? path))
            {
                XmlAttributeCollection? attributes = theElement.Attributes;
                imageRes.mUniversalProp.mSurface = SurfaceFormat.Color;
                (imageRes.mPCDXProp ??= new ImageRes()).mSurface = SurfaceFormat.Dxt5;
                (imageRes.mPCGLProp ??= new ImageRes()).mSurface = SurfaceFormat.Dxt5;
                (imageRes.mAndroidProp ??= new ImageRes()).mSurface = SurfaceFormat.Rgba8Etc2;
                (imageRes.mIOSProp ??= new ImageRes()).mSurface = SurfaceFormat.Rgba8Etc2;
                if (attributes != null)
                {
                    imageRes.mUniversalProp.mNoPal = attributes["nopal"] != null;
                    imageRes.mUniversalProp.mA4R4G4B4 = attributes["a4r4g4b4"] != null;
                    imageRes.mUniversalProp.mDDSurface = attributes["ddsurface"] != null;
                    imageRes.mUniversalProp.mNoBits = attributes["nobits"] != null;
                    imageRes.mUniversalProp.mNoBits2D = attributes["nobits2d"] != null;
                    imageRes.mUniversalProp.mNoBits3D = attributes["nobits3d"] != null;
                    imageRes.mUniversalProp.mA8R8G8B8 = attributes["a8r8g8b8"] != null;
                    imageRes.mUniversalProp.mR5G6B5 = attributes["r5g6b5"] != null;
                    imageRes.mUniversalProp.mA1R5G5B5 = attributes["a1r5g5b5"] != null;
                    imageRes.mUniversalProp.mMinSubdivide = attributes["minsubdivide"] != null;
                    imageRes.mUniversalProp.mNoAlpha = attributes["noalpha"] != null;
                    imageRes.mUniversalProp.mAlphaColor = 16777215U;
                    imageRes.mUniversalProp.mRows = 1;
                    imageRes.mUniversalProp.mCols = 1;
                    imageRes.mUniversalProp.mAnim = AnimType.None;
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "surface")
                        {
                            imageRes.mUniversalProp.mSurface = (SurfaceFormat)Enum.Parse(typeof(SurfaceFormat), current.InnerText, true);
                        }
                        else if (current.Name == "alphaimage")
                        {
                            imageRes.mUniversalProp.mAlphaImage = mDefaultPath + current.InnerText;
                        }
                        else if (current.Name == "alphacolor")
                        {
                            imageRes.mUniversalProp.mAlphaColor = Convert.ToUInt32(current.InnerText);
                        }
                        else if (current.Name == "variant")
                        {
                            imageRes.mUniversalProp.mVariant = current.InnerText;
                        }
                        else if (current.Name == "alphagrid")
                        {
                            imageRes.mUniversalProp.mAlphaGrid = current.InnerText;
                        }
                        else if (current.Name == "rows")
                        {
                            imageRes.mUniversalProp.mRows = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "cols")
                        {
                            imageRes.mUniversalProp.mCols = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "languageSpecific")
                        {
                            imageRes.mUniversalProp.mLanguageSpecific = Convert.ToBoolean(current.InnerText);
                        }
                        else if (current.Name == "format")
                        {
                            imageRes.mUniversalProp.mFormat = (TextureFormat)Enum.Parse(typeof(TextureFormat), current.InnerText, true);
                        }
                        else if (current.Name == "anim")
                        {
                            switch (current.InnerText)
                            {
                                case "none":
                                    imageRes.mUniversalProp.mAnim = AnimType.None;
                                    break;
                                case "once":
                                    imageRes.mUniversalProp.mAnim = AnimType.Once;
                                    break;
                                case "loop":
                                    imageRes.mUniversalProp.mAnim = AnimType.Loop;
                                    break;
                                case "pingpong":
                                    imageRes.mUniversalProp.mAnim = AnimType.PingPong;
                                    break;
                            }
                        }
                        else if (current.Name == "framedelay")
                        {
                            imageRes.mUniversalProp.mFrameDelay = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "begindelay")
                        {
                            imageRes.mUniversalProp.mBeginDelay = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "enddelay")
                        {
                            imageRes.mUniversalProp.mEndDelay = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "perframedelay")
                        {
                            imageRes.mUniversalProp.mPerFrameDelay = current.InnerText;
                        }
                        else if (current.Name == "framemap")
                        {
                            imageRes.mUniversalProp.mFrameMap = current.InnerText;
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
            if (ParseCommonResource(theElement, out ResBase<SoundRes>? soundRes, groupId, out string? path))
            {
                soundRes.mUniversalProp.mVolume = -1.0;
                soundRes.mUniversalProp.mPan = 0;
                XmlAttributeCollection? attributes = theElement.Attributes;
                if (attributes != null)
                {
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "volume")
                        {
                            soundRes.mUniversalProp.mVolume = Convert.ToDouble(current.InnerText);
                        }
                        if (current.Name == "pan")
                        {
                            soundRes.mUniversalProp.mPan = Convert.ToInt32(current.InnerText);
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
            if (ParseCommonResource(theElement, out ResBase<FontRes>? fontRes, groupId, out string? path))
            {
                XmlAttributeCollection? attributes = theElement.Attributes;
                if (attributes != null)
                {
                    fontRes.mUniversalProp.mSize = -1;
                    fontRes.mUniversalProp.mBold = false;
                    fontRes.mUniversalProp.mItalic = false;
                    fontRes.mUniversalProp.mShadow = false;
                    fontRes.mUniversalProp.mUnderline = false;
                    foreach (XmlAttribute current in attributes)
                    {
                        if (current.Name == "tags")
                        {
                            fontRes.mUniversalProp.mTags = current.InnerText;
                        }
                        else if (current.Name == "isDefault")
                        {
                            fontRes.mUniversalProp.mIsDefault = true;
                        }
                        else if (current.Name == "truetype")
                        {
                            fontRes.mUniversalProp.mTrueType = true;
                        }
                        else if (current.Name == "size")
                        {
                            fontRes.mUniversalProp.mSize = Convert.ToInt32(current.InnerText);
                        }
                        else if (current.Name == "bold")
                        {
                            fontRes.mUniversalProp.mBold = true;
                        }
                        else if (current.Name == "italic")
                        {
                            fontRes.mUniversalProp.mItalic = true;
                        }
                        else if (current.Name == "shadow")
                        {
                            fontRes.mUniversalProp.mShadow = true;
                        }
                        else if (current.Name == "underline")
                        {
                            fontRes.mUniversalProp.mUnderline = true;
                        }
                        else if (current.Name == "stroked")
                        {
                            fontRes.mUniversalProp.mStroke = Convert.ToInt32(current.InnerText);
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

        private bool ParseCommonResource<T>(XmlNode theElement, [MaybeNullWhen(false)] out ResBase<T> theRes, string groupId, out string? path) where T : PlatformProperties, new()
        {
            XmlAttributeCollection? attributes = theElement.Attributes;
            if (attributes == null)
            {
                theRes = null;
                path = null;
                return false;
            }
            theRes = new ResBase<T>
            {
                mId = mDefaultIdPrefix + attributes["id"]?.InnerText,
                mGroup = groupId,
                mUniversalProp = new T(),
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
                theRes.mUniversalProp.mUnloadGroup = Convert.ToInt32(unloadGroup);
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
                        mDefaultPath = current.Value + "/";
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
                    return XnbTexture2DCoder.Shared.ReadOne(Path.GetFileName(path), xnbStream);
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

        private void DoLoadAtlas(ResBase<AtlasRes> atlasRes, string path)
        {
            List<PathAndAtlasInfo> pathList = new List<PathAndAtlasInfo>();
            LoadAtlasPaths(path, atlasRes.mUniversalProp.mLanguageSpecific == true, pathList);
            foreach (PathAndAtlasInfo info in pathList)
            {
                string atlasPath = info.DestPath;
                string imgContentPath = GetContentPath(LoadImageExtension(info.Path, atlasRes.mUniversalProp.mFormat ?? TextureFormat.Png));
                if (File.Exists(imgContentPath))
                {
                    EnsureThisFolderExist(GetUnpackPath(atlasPath));
                    using (IDisposableBitmap bitmap = DecodeImageFromPath(imgContentPath, atlasRes.mUniversalProp.mFormat ?? TextureFormat.Png))
                    {
                        RefBitmap bitmapRef = bitmap.AsRefBitmap();
                        atlasRes.mUniversalProp.mWidth = GetCloserPOT(bitmapRef.Width);
                        atlasRes.mUniversalProp.mHeight = GetCloserPOT(bitmapRef.Height);
                        atlasRes.mUniversalProp.mAtlasName = info.Atlas[atlasRes.mId].Item2;
                        foreach (SpriteItem spirits in info.Atlas[atlasRes.mId].Item1)
                        {
                            string thisImgPath = Path.Combine(atlasPath, spirits.mId.ToLower());
                            string thisImgExPath = LoadImageExtension(thisImgPath, TextureFormat.Png);
                            ResBase<SubImageRes> subImageRes = new ResBase<SubImageRes>
                            {
                                mGroup = atlasRes.mGroup,
                                mId = spirits.mId,
                                mUniversalProp = new SubImageRes
                                {
                                    mParent = atlasRes.mId,
                                    mRows = spirits.mRows,
                                    mCols = spirits.mCols,
                                    mAnim = spirits.mAnim,
                                    mFrameDelay = spirits.mFrameDelay,
                                    mBeginDelay = spirits.mBeginDelay,
                                    mEndDelay = spirits.mEndDelay,
                                },
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
                    AOTJson.TrySerializeToFile<ResBase>(GetUnpackMetaPath(atlasPath), atlasRes);
                }
                else
                {
                    mAbsentRes.Add(path);
                }
            }
        }

        private void DoLoadImage(ResBase<ImageRes> imageRes, string path)
        {
            List<PathNoAtlasInfo> pathList = new List<PathNoAtlasInfo>();
            LoadImagePaths(path, imageRes.mUniversalProp.mLanguageSpecific == true, pathList);
            foreach (PathNoAtlasInfo info in pathList)
            {
                string imgContentPath = GetContentPath(LoadImageExtension(info.Path, imageRes.mUniversalProp.mFormat ?? TextureFormat.Png));
                if (File.Exists(imgContentPath))
                {
                    string imgPath = LoadImageExtension(info.DestPath, TextureFormat.Png);
                    string imgUnpackPath = GetUnpackPath(imgPath);
                    EnsureParentFolderExist(imgUnpackPath);
                    using (IDisposableBitmap bitmap = DecodeImageFromPath(imgContentPath, imageRes.mUniversalProp.mFormat ?? TextureFormat.Png))
                    {
                        bitmap.SaveAsPng(imgUnpackPath);
                    }
                    imageRes.mDiskFormat = DiskFormat.Png;
                    AOTJson.TrySerializeToFile<ResBase>(GetUnpackMetaPath(info.DestPath), imageRes);
                }
                else
                {
                    mAbsentRes.Add(path);
                }
            }
        }

        private void DoLoadReanim(string groupName, string path)
        {
            ResBase<ReanimRes> reanimRes = new ResBase<ReanimRes> { mDiskFormat = DiskFormat.Reanim, mGroup = groupName, mId = "REANIM_" + Path.GetFileNameWithoutExtension(path).ToUpper(), mUniversalProp = new ReanimRes() };
            string reanimPath = Path.Combine("reanim", Path.GetFileNameWithoutExtension(path));
            if (File.Exists(path))
            {
                string reanimUnpackPath = GetUnpackPath(reanimPath + ".reanim");
                EnsureParentFolderExist(reanimUnpackPath);
                ReanimatorDefinition reanim = XnbReanimCoder.Shared.Decode(path);
                XmlReanimCoder.Shared.Encode(reanim, reanimUnpackPath);
                AOTJson.TrySerializeToFile<ResBase>(GetUnpackMetaPath(reanimPath), reanimRes);
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }

        private void DoLoadParticleAndTrail(string groupName, string path)
        {
            ResBase<ParticleRes> particleRes = new ResBase<ParticleRes> { mDiskFormat = DiskFormat.Xnb, mGroup = groupName, mId = "PARTICLE_" + Path.GetFileNameWithoutExtension(path).ToUpper(), mUniversalProp = new ParticleRes() };
            string particlePath = Path.Combine("particles", Path.GetFileName(path));
            if (File.Exists(path))
            {
                string particleUnpackPath = GetUnpackPath(particlePath);
                EnsureParentFolderExist(particleUnpackPath);
                File.Copy(path, particleUnpackPath, true);
                AOTJson.TrySerializeToFile<ResBase>(GetUnpackMetaPath(particlePath), particleRes);
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }

        private void DoLoadSound(ResBase<SoundRes> soundRes, string path)
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
                    ResBase<SoundRes>? repeatRes = AOTJson.TryDeserializeFromFile<ResBase>(metaPath) as ResBase<SoundRes>;
                    if (repeatRes != null)
                    {
                        (repeatRes.mSameIds ??= []).Add(soundRes);
                        AOTJson.TrySerializeToFile<ResBase>(metaPath, repeatRes);
                        finished = true;
                    }
                }
                else
                {
                    mExistedPath.Add(soundUnpackPath);
                }
                if (!finished)
                {
                    AOTJson.TrySerializeToFile<ResBase>(metaPath, soundRes);
                }
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }

        private void DoLoadFont(ResBase<FontRes> fontRes, string path)
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
                    ResBase<FontRes>? repeatRes = AOTJson.TryDeserializeFromFile<ResBase>(metaPath) as ResBase<FontRes>;
                    if (repeatRes != null)
                    {
                        (repeatRes.mSameIds ??= []).Add(fontRes);
                        AOTJson.TrySerializeToFile<ResBase>(metaPath, repeatRes);
                        finished = true;
                    }
                }
                else
                {
                    mExistedPath.Add(fontUnpackPath);
                }
                if (!finished)
                {
                    AOTJson.TrySerializeToFile<ResBase>(metaPath, fontRes);
                }
            }
            else
            {
                mAbsentRes.Add(path);
            }
        }
    }
}
