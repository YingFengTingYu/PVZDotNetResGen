using PVZDotNetResGen.Sexy.Atlas;
using PVZDotNetResGen.Sexy.Image;
using PVZDotNetResGen.Sexy.Reanim;
using PVZDotNetResGen.Utils.Graphics;
using PVZDotNetResGen.Utils.Graphics.Bitmap;
using PVZDotNetResGen.Utils.JsonHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace PVZDotNetResGen.Sexy
{
    public class ResourcePacker(string contentFolderPath, string codeFolderPath, string unpackFolderPath, string buildCacheFolderPath, BuildPlatform platform = BuildPlatform.Invalid)
    {
        private readonly string mContentFolderPath = contentFolderPath;
        private readonly string mCodeFolderPath = codeFolderPath;
        private readonly string mUnpackFolderPath = unpackFolderPath;
        private readonly string mBuildCacheFolderPath = buildCacheFolderPath;
        private Dictionary<string, XmlNode> mXmlNodeList = [];
        private HashSet<string> mExistedImageId = [];
        private Dictionary<string, Dictionary<string, List<SpriteItem>>> mSubImages = [];
        private readonly BuildPlatform mPlatform = platform;
        private bool mBuildInAtlasInfo = true;

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

        public string GetTempPath(string path)
        {
            return Path.Combine(mBuildCacheFolderPath, path);
        }

        public string GetTempMetaPath(string path)
        {
            return Path.Combine(mBuildCacheFolderPath, path + ".build.json");
        }

        public string GetUnpackMetaPathForSubImage(string path)
        {
            return Path.Combine(mUnpackFolderPath, "resources", path + ".subimage.json");
        }

        public string GetRecordedPathFromUnpackMetaPath(string path)
        {
            string resourcesPath = Path.Combine(mUnpackFolderPath, "resources") + "\\";
            Debug.Assert(path.StartsWith(resourcesPath, StringComparison.CurrentCultureIgnoreCase));
            Debug.Assert(path.EndsWith(".meta.json", StringComparison.CurrentCultureIgnoreCase));
            return path[resourcesPath.Length..^".meta.json".Length];
        }

        public IEnumerator<bool> Update()
        {
            PackInfo packInfo = AOTJson.TryDeserializeFromFile<PackInfo>(GetInfoPath("pack.json")) ?? throw new Exception("Cannot read packInfo.json");
            string[] metaFiles = Directory.GetFiles(Path.Combine(mUnpackFolderPath, "resources"), "*.meta.json", SearchOption.AllDirectories);
            Array.Sort(metaFiles);
            XmlDocument xmlDocResources = new XmlDocument();
            XmlElement root = xmlDocResources.CreateElement(null, "ResourceManifest", null);
            xmlDocResources.AppendChild(root);
            foreach (string group in packInfo.mGroups)
            {
                XmlElement xmlElement = xmlDocResources.CreateElement("Resources");
                SetStringIfExist(xmlElement, "id", group);
                root.AppendChild(xmlElement);
                mXmlNodeList.Add(group, xmlElement);
            }
            foreach (string metaFile in metaFiles)
            {
                ResBase? resBase = AOTJson.TryDeserializeFromFile<ResBase>(metaFile);
                if (resBase != null)
                {
                    if (resBase is ResBase<ImageRes> imageResBase)
                    {
                        imageResBase.MergePlatform(mPlatform);
                        if (mExistedImageId.Contains(imageResBase.mId))
                        {
                            ParseImageResource(null, imageResBase, metaFile);
                        }
                        else
                        {
                            XmlElement resNode = xmlDocResources.CreateElement("Image");
                            ParseImageResource(resNode, imageResBase, metaFile);
                            mXmlNodeList[imageResBase.mGroup].AppendChild(resNode);
                            mExistedImageId.Add(imageResBase.mId);
                        }
                        var sameIds = imageResBase.mSameIds;
                        if (sameIds != null && sameIds.Count > 0)
                        {
                            foreach (var sameIdRes in sameIds)
                            {
                                sameIdRes.MergePlatform(mPlatform);
                                if (mExistedImageId.Contains(sameIdRes.mId))
                                {
                                    ParseImageResource(null, sameIdRes, metaFile);
                                }
                                else
                                {
                                    XmlElement resNode = xmlDocResources.CreateElement("Image");
                                    ParseImageResource(resNode, sameIdRes, metaFile);
                                    mXmlNodeList[sameIdRes.mGroup].AppendChild(resNode);
                                    mExistedImageId.Add(sameIdRes.mId);
                                }
                            }
                        }
                    }
                    else if (resBase is ResBase<AtlasRes> atlasResBase)
                    {
                        atlasResBase.MergePlatform(mPlatform);
                        if (mExistedImageId.Contains(atlasResBase.mId))
                        {
                            ParseAtlasResource(null, atlasResBase, metaFile, packInfo);
                        }
                        else
                        {
                            XmlElement resNode = xmlDocResources.CreateElement("Image");
                            ParseAtlasResource(resNode, atlasResBase, metaFile, packInfo);
                            mXmlNodeList[atlasResBase.mGroup].AppendChild(resNode);
                            mExistedImageId.Add(atlasResBase.mId);
                        }
                        var sameIds = atlasResBase.mSameIds;
                        if (sameIds != null && sameIds.Count > 0)
                        {
                            foreach (var sameIdRes in sameIds)
                            {
                                sameIdRes.MergePlatform(mPlatform);
                                if (mExistedImageId.Contains(sameIdRes.mId))
                                {
                                    ParseAtlasResource(null, sameIdRes, metaFile, packInfo);
                                }
                                else
                                {
                                    XmlElement resNode = xmlDocResources.CreateElement("Image");
                                    ParseAtlasResource(resNode, sameIdRes, metaFile, packInfo);
                                    mXmlNodeList[sameIdRes.mGroup].AppendChild(resNode);
                                    mExistedImageId.Add(sameIdRes.mId);
                                }
                            }
                        }
                    }
                    else if (resBase is ResBase<ReanimRes> reanimResBase)
                    {
                        reanimResBase.MergePlatform(mPlatform);
                        ParseReanimResource(reanimResBase, metaFile);
                    }
                    else if (resBase is ResBase<ParticleRes> particleResBase)
                    {
                        particleResBase.MergePlatform(mPlatform);
                        ParseParticleResource(particleResBase, metaFile);
                    }
                    else if (resBase is ResBase<TrailRes> trailResBase)
                    {
                        trailResBase.MergePlatform(mPlatform);
                        ParseTrailResource(trailResBase, metaFile);
                    }
                    else if (resBase is ResBase<SoundRes> soundResBase)
                    {
                        soundResBase.MergePlatform(mPlatform);
                        XmlElement resNode = xmlDocResources.CreateElement("Sound");
                        ParseSoundResource(resNode, soundResBase, metaFile);
                        mXmlNodeList[soundResBase.mGroup].AppendChild(resNode);
                        var sameIds = soundResBase.mSameIds;
                        if (sameIds != null && sameIds.Count > 0)
                        {
                            foreach (var sameIdRes in sameIds)
                            {
                                sameIdRes.MergePlatform(mPlatform);
                                XmlElement resNode2 = xmlDocResources.CreateElement("Sound");
                                ParseSoundResource(resNode2, sameIdRes, metaFile);
                                mXmlNodeList[sameIdRes.mGroup].AppendChild(resNode2);
                            }
                        }
                    }
                    else if (resBase is ResBase<FontRes> fontResBase)
                    {
                        fontResBase.MergePlatform(mPlatform);
                        XmlElement resNode = xmlDocResources.CreateElement("Font");
                        ParseFontResource(resNode, fontResBase, metaFile);
                        mXmlNodeList[fontResBase.mGroup].AppendChild(resNode);
                        var sameIds = fontResBase.mSameIds;
                        if (sameIds != null && sameIds.Count > 0)
                        {
                            foreach (var sameIdRes in sameIds)
                            {
                                sameIdRes.MergePlatform(mPlatform);
                                XmlElement resNode2 = xmlDocResources.CreateElement("Font");
                                ParseFontResource(resNode2, sameIdRes, metaFile);
                                mXmlNodeList[sameIdRes.mGroup].AppendChild(resNode2);
                            }
                        }
                    }
                }
                yield return false;
            }
            CreateCodeTo();
            foreach (var pair in mSubImages)
            {
                if (mBuildInAtlasInfo)
                {
                    CreateCodeAtlasInfoTo(pair.Key);
                }
                else
                {
                    CreateJsonAtlasInfoTo(pair.Key);
                }
            }
            xmlDocResources.Save(GetContentPath("resources.xml"));
        }

        private void CreateCodeTo()
        {
            string path = Path.Combine(mCodeFolderPath, $"AtlasResources.cs");
            var thisSubImages = mSubImages.First().Value;
            EnsureParentFolderExist(path);
            const int startIndex = 10001;
            List<string> idList = [];
            foreach (var pair in thisSubImages)
            {
                foreach (var sub in pair.Value)
                {
                    idList.Add(sub.mId);
                }
            }
            idList.Sort();
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("""
                    using System;
                    using Sexy;

                    namespace Sexy
                    {
                        public/*internal*/ class AtlasResources
                        {
                            public void ExtractResources()
                            {
                    """);
                foreach (var pair in thisSubImages)
                {
                    sw.Write("            Unpack");
                    sw.Write(pair.Key);
                    sw.WriteLine("AtlasImages();");
                }
                sw.WriteLine("""
                            }

                    """);
                foreach (var pair in thisSubImages)
                {
                    sw.Write("        public virtual void Unpack");
                    sw.Write(pair.Key);
                    sw.WriteLine("AtlasImages()");
                    sw.WriteLine("""
                            {
                            }

                    """);
                }
                sw.WriteLine("""
                            public static Image GetImageInAtlasById(int theId)
                            {
                                switch (theId)
                                {
                    """);
                for (int i = 0; i < idList.Count; i++)
                {
                    sw.Write("            case ");
                    sw.Write(i + startIndex);
                    sw.WriteLine(":");
                    sw.Write("                return AtlasResources.");
                    sw.Write(idList[i]);
                    sw.WriteLine(";");
                }
                sw.WriteLine("""
                                default:
                            return Resources.GetImageById(theId);
                        }
                    }

                    public static int GetAtlasIdByStringId(string theStringId)
                    {
                        for (int i = 0; i < AtlasResources.table.Length; i++)
                        {
                            if (theStringId == AtlasResources.table[i].mStringId)
                            {
                                return AtlasResources.table[i].mImageId;
                            }
                        }
                        return (int)Resources.GetIdByStringId(theStringId);
                    }

                    public static int GetIdByImageInAtlas(Image theImage)
                    {
                    """);
                for (int i = 0; i < idList.Count; i++)
                {
                    sw.Write("            if (theImage == AtlasResources.");
                    sw.Write(idList[i]);
                    sw.WriteLine(")");
                    sw.WriteLine("            {");
                    sw.Write("                return ");
                    sw.Write(i + startIndex);
                    sw.WriteLine(";");
                    sw.WriteLine("            }");
                }
                sw.WriteLine("""
                        int idByImage = (int)Resources.GetIdByImage(theImage);
                        if (idByImage == 249)
                        {
                            return -1;
                        }
                        return idByImage;
                    }

                    public static AtlasResources mAtlasResources;

                    """);
                for (int i = 0; i < idList.Count; i++)
                {
                    sw.Write("        public static Image ");
                    sw.Write(idList[i]);
                    sw.WriteLine(";");
                    sw.WriteLine();
                }
                sw.WriteLine("""
                    private static AtlasResources.AtlasStringTable[] table = new AtlasResources.AtlasStringTable[]
                    {
                    """);
                for (int i = 0; i < idList.Count; i++)
                {
                    sw.Write("        new AtlasResources.AtlasStringTable(\"");
                    sw.Write(idList[i]);
                    sw.Write("\", ");
                    sw.Write(i + startIndex);
                    sw.WriteLine("),");
                }
                sw.WriteLine("""
                    };
                    
                    public enum AtlasImageId
                    {
                        __ATLAS_BASE_ID = 10000,
                    """);
                for (int i = 0; i < idList.Count; i++)
                {
                    sw.Write("            ");
                    sw.Write(idList[i]);
                    sw.WriteLine("_ID,");
                }
                sw.WriteLine("""
                            }

                            public class AtlasStringTable
                            {
                                public AtlasStringTable(string strId, int imgId)
                                {
                                    mStringId = strId;
                                    mImageId = imgId;
                                }

                                public string mStringId;

                                public int mImageId;
                            }
                        }
                    }
                    """);
            }
        }

        private void CreateJsonAtlasInfoTo(string res)
        {
            string path = GetContentPath(Path.Combine("atlases", res));
            var thisSubImages = mSubImages[res];
            foreach (var pair in thisSubImages)
            {
                AtlasInfo atlasInfo = new AtlasInfo
                {
                    mSubImages = pair.Value,
                };
                string jsonPath = Path.Combine(path, pair.Key + ".atlas.json");
                EnsureParentFolderExist(jsonPath);
                AOTJson.TrySerializeToFile(jsonPath, atlasInfo);
            }
        }

        private void CreateCodeAtlasInfoTo(string res)
        {
            string path = Path.Combine(mCodeFolderPath, $"AtlasResources_{res}.cs");
            var thisSubImages = mSubImages[res];
            EnsureParentFolderExist(path);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write("""
                    using System;
                    using System.Collections.Generic;
                    using Sexy;

                    namespace Sexy
                    {
                        public/*internal*/ class 
                    """);
                sw.Write(Path.GetFileNameWithoutExtension(path));
                sw.WriteLine(" : AtlasResources");
                sw.WriteLine("    {");
                foreach (var pair in thisSubImages)
                {
                    var list = pair.Value;
                    sw.Write("        public override void Unpack");
                    sw.Write(pair.Key);
                    sw.WriteLine("AtlasImages()");
                    sw.WriteLine("        {");
                    sw.WriteLine("            UNPACK_INFO[] array = new UNPACK_INFO[]");
                    sw.WriteLine("            {");
                    for (int listIndex = 0; listIndex < list.Count; listIndex++)
                    {
                        var item = list[listIndex];
                        sw.Write("            new UNPACK_INFO(AtlasResources.");
                        sw.Write(item.mId);
                        sw.Write(", ");
                        sw.Write(item.mX);
                        sw.Write(", ");
                        sw.Write(item.mY);
                        sw.Write(", ");
                        sw.Write(item.mWidth);
                        sw.Write(", ");
                        sw.Write(item.mHeight);
                        sw.Write(", ");
                        sw.Write(item.mRows);
                        sw.Write(", ");
                        sw.Write(item.mCols);
                        sw.Write(", AnimType.AnimType_");
                        sw.Write(item.mAnim);
                        sw.Write(", ");
                        sw.Write(item.mFrameDelay);
                        sw.Write(", ");
                        sw.Write(item.mBeginDelay);
                        sw.Write(", ");
                        sw.Write(item.mEndDelay);
                        if (listIndex != list.Count - 1)
                        {
                            sw.WriteLine("),");
                        }
                        else
                        {
                            sw.WriteLine(")");
                        }
                    }
                    sw.WriteLine("            };");
                    sw.Write("            mArrays[\"");
                    sw.Write(pair.Key);
                    sw.WriteLine("\"] = array;");
                    sw.WriteLine("            for (int i = 0; i < array.Length; i++)");
                    sw.WriteLine("            {");
                    sw.Write("                array[i].mpImage = new Image(Resources.IMAGE_");
                    sw.Write(pair.Key.ToUpper());
                    sw.WriteLine(", array[i].mX, array[i].mY, array[i].mWidth, array[i].mHeight);");
                    sw.WriteLine("                array[i].mpImage.mNumRows = array[i].mRows;");
                    sw.WriteLine("                array[i].mpImage.mNumCols = array[i].mCols;");
                    sw.WriteLine("            }");
                    sw.WriteLine("            int num = 0;");
                    for (int listIndex = 0; listIndex < list.Count; listIndex++)
                    {
                        sw.Write("            AtlasResources.");
                        sw.Write(list[listIndex].mId);
                        sw.WriteLine(" = array[num].mpImage;");
                        sw.WriteLine("            num++;");
                    }
                    sw.WriteLine("        }");
                    sw.WriteLine();
                }
                sw.WriteLine("""
                            public static Dictionary<string, UNPACK_INFO[]> mArrays = new Dictionary<string, UNPACK_INFO[]>();
                        }
                    }
                    """);
            }
        }

        private bool ParseCommonResource<T>(XmlElement theElement, ResBase<T> theRes, string path) where T : PlatformProperties, new()
        {
            theElement.SetAttribute("id", theRes.mId);
            theElement.SetAttribute("path", path.Replace('\\', '/'));
            if (theRes.mUniversalProp.mUnloadGroup != null)
            {
                theElement.SetAttribute("unloadGroup", Convert.ToString(theRes.mUniversalProp.mUnloadGroup));
            }
            return true;
        }

        private bool ParseImageResource(XmlElement? theElement, ResBase<ImageRes> imageRes, string metaPath)
        {
            GetImagePathsFromNativeUnpackMetaPath(metaPath, imageRes.mDiskFormat, imageRes.mUniversalProp.mFormat ?? TextureFormat.Png, out string recordedPath, out string contentPath, out string unpackPath, out string tempPath, out string tempMetaPath);
            bool cont = true;
            if (theElement != null)
            {
                if (ParseCommonResource(theElement, imageRes, recordedPath))
                {
                    cont = ParseImageResourceByProp(theElement, imageRes.mUniversalProp);
                }
            }
            if (cont)
            {
                bool rebuild = false;
                BuildImageInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildImageInfo>(tempMetaPath);
                if (buildInfo == null || !File.Exists(unpackPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mDiskFormat != imageRes.mDiskFormat)
                {
                    rebuild = true;
                }
                else if (buildInfo.mFormat != (imageRes.mUniversalProp.mFormat ?? TextureFormat.Png))
                {
                    rebuild = true;
                }
                else if (buildInfo.mFormat == TextureFormat.Content && buildInfo.mSurface != (imageRes.mUniversalProp.mSurface ?? SurfaceFormat.Bgra4444))
                {
                    rebuild = true;
                }
                else if (buildInfo.mHash != GetHash(unpackPath))
                {
                    rebuild = true;
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildImageInfo();
                    DiskFormat aDiskFormat = imageRes.mDiskFormat;
                    TextureFormat aInGameFormat = imageRes.mUniversalProp.mFormat ?? TextureFormat.Png;
                    SurfaceFormat aSurfaceFormat = imageRes.mUniversalProp.mSurface ?? SurfaceFormat.Bgra4444;
                    using (IDisposableBitmap bitmap = DecodeImageFromPath(unpackPath, aDiskFormat))
                    {
                        EncodeImageToPath(bitmap, tempPath, aInGameFormat, aSurfaceFormat);
                    }
                    buildInfo.mDiskFormat = aDiskFormat;
                    buildInfo.mFormat = aInGameFormat;
                    buildInfo.mSurface = aSurfaceFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            EnsureParentFolderExist(contentPath);
            File.Copy(tempPath, contentPath, true);
            return true;
        }

        private bool ParseImageResourceByProp(XmlElement theElement, ImageRes imageRes)
        {
            SetBooleanAsExist(theElement, "nopal", imageRes.mNoPal);
            SetBooleanAsExist(theElement, "a4r4g4b4", imageRes.mA4R4G4B4);
            SetBooleanAsExist(theElement, "ddsurface", imageRes.mDDSurface);
            SetBooleanAsExist(theElement, "nobits", imageRes.mNoBits);
            SetBooleanAsExist(theElement, "nobits2d", imageRes.mNoBits2D);
            SetBooleanAsExist(theElement, "nobits3d", imageRes.mNoBits3D);
            SetBooleanAsExist(theElement, "a8r8g8b8", imageRes.mA8R8G8B8);
            SetBooleanAsExist(theElement, "r5g6b5", imageRes.mR5G6B5);
            SetBooleanAsExist(theElement, "a1r5g5b5", imageRes.mA1R5G5B5);
            SetBooleanAsExist(theElement, "minsubdivide", imageRes.mMinSubdivide);
            SetBooleanAsExist(theElement, "noalpha", imageRes.mNoAlpha);
            SetValueTypeIfExist(theElement, "surface", imageRes.mSurface);
            if (imageRes.mAlphaImage != null)
            {
                string alphaImage = imageRes.mAlphaImage;
                if (alphaImage.StartsWith('/'))
                {
                    alphaImage = alphaImage[1..];
                }
                theElement.SetAttribute("alphaimage", alphaImage);
            }
            SetValueTypeIfExist(theElement, "alphacolor", imageRes.mAlphaColor);
            SetStringIfExist(theElement, "variant", imageRes.mVariant);
            SetStringIfExist(theElement, "alphagrid", imageRes.mAlphaGrid);
            SetValueTypeIfExist(theElement, "rows", imageRes.mRows);
            SetValueTypeIfExist(theElement, "cols", imageRes.mCols);
            SetValueTypeIfExist(theElement, "languageSpecific", imageRes.mLanguageSpecific);
            SetValueTypeIfExist(theElement, "format", imageRes.mFormat);
            if (imageRes.mAnim != null)
            {
                switch (imageRes.mAnim)
                {
                    case AnimType.None:
                        theElement.SetAttribute("anim", "none");
                        break;
                    case AnimType.Once:
                        theElement.SetAttribute("anim", "once");
                        break;
                    case AnimType.Loop:
                        theElement.SetAttribute("anim", "loop");
                        break;
                    case AnimType.PingPong:
                        theElement.SetAttribute("anim", "pingpong");
                        break;
                }
            }
            SetValueTypeIfExist(theElement, "framedelay", imageRes.mFrameDelay);
            SetValueTypeIfExist(theElement, "begindelay", imageRes.mBeginDelay);
            SetValueTypeIfExist(theElement, "enddelay", imageRes.mEndDelay);
            SetStringIfExist(theElement, "perframedelay", imageRes.mPerFrameDelay);
            SetStringIfExist(theElement, "framemap", imageRes.mFrameMap);
            return true;
        }

        private bool ParseAtlasResource(XmlElement? theElement, ResBase<AtlasRes> imageRes, string metaPath, PackInfo packinfo)
        {
            GetAtlasPathsFromNativeUnpackMetaPath(metaPath, imageRes.mUniversalProp.mFormat ?? TextureFormat.Png, out string recordedPath, out string contentPath, out string unpackPath, out string tempPath, out string tempMetaPath);
            bool cont = true;
            if (theElement != null)
            {
                if (ParseCommonResource(theElement, imageRes, recordedPath))
                {
                    cont = ParseAtlasResourceByProp(theElement, imageRes.mUniversalProp);
                }
            }
            if (cont)
            {
                bool rebuild = false;
                BuildAtlasInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildAtlasInfo>(tempMetaPath);
                if (buildInfo == null || !Directory.Exists(unpackPath) || !File.Exists(tempPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mWidth != imageRes.mUniversalProp.mWidth || buildInfo.mHeight != imageRes.mUniversalProp.mHeight)
                {
                    rebuild = true;
                }
                else if (buildInfo.mFormat != (imageRes.mUniversalProp.mFormat ?? TextureFormat.Png))
                {
                    rebuild = true;
                }
                else if (buildInfo.mFormat == TextureFormat.Content && buildInfo.mSurface != (imageRes.mUniversalProp.mSurface ?? SurfaceFormat.Bgra4444))
                {
                    rebuild = true;
                }
                else if (buildInfo.mSubImages == null)
                {
                    rebuild = true;
                }
                else if (buildInfo.mSubImages.Count != Directory.GetFiles(unpackPath, "*.png", SearchOption.TopDirectoryOnly).Length)
                {
                    rebuild = true;
                }
                else
                {
                    foreach (var subImagesInfo in buildInfo.mSubImages)
                    {
                        string png = Path.Combine(unpackPath, subImagesInfo.mId?.ToLower() + ".png");
                        if (!File.Exists(png) || subImagesInfo.mHash != GetHash(png))
                        {
                            rebuild = true;
                            break;
                        }
                    }
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildAtlasInfo();
                    TextureFormat aInGameFormat = imageRes.mUniversalProp.mFormat ?? TextureFormat.Png;
                    SurfaceFormat aSurfaceFormat = imageRes.mUniversalProp.mSurface ?? SurfaceFormat.Bgra4444;
                    int width = imageRes.mUniversalProp.mWidth ?? 2048;
                    int height = imageRes.mUniversalProp.mHeight ?? 2048;
                    int extrude = imageRes.mUniversalProp.mExtrude ?? 1;
                    // 创建所有没有meta的图像的meta
                    List<MaxRectsBinPack.BinRect> sizeList = new List<MaxRectsBinPack.BinRect>();
                    List<MaxRectsBinPack.BinRect> ansList = new List<MaxRectsBinPack.BinRect>();
                    foreach (var png in Directory.GetFiles(unpackPath, "*.png", SearchOption.TopDirectoryOnly))
                    {
                        string pngMetaPath = Path.ChangeExtension(png, ".subimage.json");
                        ResBase<SubImageRes>? subImageRes = AOTJson.TryDeserializeFromFile<ResBase>(pngMetaPath) as ResBase<SubImageRes>;
                        if (subImageRes == null)
                        {
                            subImageRes = new ResBase<SubImageRes> { mGroup = imageRes.mGroup, mId = Path.GetFileNameWithoutExtension(png).ToUpper(), mUniversalProp = new SubImageRes { mParent = imageRes.mId, mCols = 1, mRows = 1, mAnim = AnimType.None, mBeginDelay = 0, mEndDelay = 0, mFrameDelay = 0 } };
                            AOTJson.TrySerializeToFile<ResBase>(pngMetaPath, subImageRes);
                        }
                        if (!BitmapHelper.Peek(png, out int pngWidth, out int pngHeight))
                        {
                            throw new Exception("not a png file: " + png);
                        }
                        sizeList.Add(new MaxRectsBinPack.BinRect { width = pngWidth + 2 * extrude, height = pngHeight + 2 * extrude, id = png });
                    }
                    sizeList.Sort(static (a, b) =>
                    {
                        int thisArea = a.width * a.height;
                        int otherArea = b.width * b.height;
                        if (thisArea > otherArea)
                            return -1;
                        if (thisArea == otherArea)
                            return 0;
                        return 1;
                    });
                    MaxRectsBinPack binPack = new MaxRectsBinPack(width, height, false);
                    int sizeOld = sizeList.Count;
                    binPack.Insert(sizeList, ansList, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit);
                    if (sizeOld > ansList.Count)
                    {
                        throw new Exception($"The size of atlas {metaPath} is too small!");
                    }
                    buildInfo.mSubImages = [];
                    buildInfo.mWidth = width;
                    buildInfo.mHeight = height;
                    using (IDisposableBitmap bitmap = new NativeBitmap(width, height))
                    {
                        RefBitmap atlasBitmapRef = bitmap.AsRefBitmap();
                        for (int i = 0; i < ansList.Count; i++)
                        {
                            MaxRectsBinPack.BinRect rect = ansList[i];
                            Debug.Assert(rect.id != null);
                            BuildAtlasInfo.SubImageBuildInfo buildImageInfo = new BuildAtlasInfo.SubImageBuildInfo();
                            buildImageInfo.mId = Path.GetFileNameWithoutExtension(rect.id).ToUpper();
                            buildImageInfo.mX = rect.x + extrude;
                            buildImageInfo.mY = rect.y + extrude;
                            using (StbBitmap pngBitmap = new StbBitmap(rect.id))
                            {
                                pngBitmap.AsRefBitmap().CopyTo(atlasBitmapRef, 0, 0, rect.x + extrude, rect.y + extrude, pngBitmap.Width, pngBitmap.Height, extrude);
                                buildImageInfo.mWidth = pngBitmap.Width;
                                buildImageInfo.mHeight = pngBitmap.Height;
                            }
                            buildImageInfo.mHash = GetHash(rect.id);
                            buildInfo.mSubImages.Add(buildImageInfo);
                        }
                        EncodeImageToPath(bitmap, tempPath, aInGameFormat, aSurfaceFormat);
                    }
                    buildInfo.mFormat = aInGameFormat;
                    buildInfo.mSurface = aSurfaceFormat;
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
                // 构建代码
                string atlasName = imageRes.mUniversalProp.mAtlasName ?? string.Empty;
                if (!mSubImages.ContainsKey(atlasName))
                {
                    List<SpriteItem> subImages = [];
                    var buildInfoSubImages = buildInfo?.mSubImages;
                    if (buildInfoSubImages != null)
                    {
                        foreach (var buildInfoSubImage in buildInfoSubImages)
                        {
                            string png = Path.Combine(unpackPath, buildInfoSubImage.mId?.ToLower() + ".png");
                            string pngMetaPath = Path.ChangeExtension(png, ".subimage.json");
                            ResBase<SubImageRes>? subImageRes = AOTJson.TryDeserializeFromFile<ResBase>(pngMetaPath) as ResBase<SubImageRes>;
                            if (subImageRes == null)
                            {
                                subImageRes = new ResBase<SubImageRes> { mGroup = imageRes.mGroup, mId = Path.GetFileNameWithoutExtension(png).ToUpper(), mUniversalProp = new SubImageRes { mParent = imageRes.mId, mCols = 1, mRows = 1, mAnim = AnimType.None, mBeginDelay = 0, mEndDelay = 0, mFrameDelay = 0 } };
                                AOTJson.TrySerializeToFile<ResBase>(pngMetaPath, subImageRes);
                            }
                            subImages.Add(new SpriteItem
                            {
                                mId = buildInfoSubImage.mId ?? string.Empty,
                                mX = buildInfoSubImage.mX,
                                mY = buildInfoSubImage.mY,
                                mWidth = buildInfoSubImage.mWidth,
                                mHeight = buildInfoSubImage.mHeight,
                                mRows = subImageRes.mUniversalProp.mRows ?? 1,
                                mCols = subImageRes.mUniversalProp.mCols ?? 1,
                                mAnim = subImageRes.mUniversalProp.mAnim ?? AnimType.None,
                                mBeginDelay = subImageRes.mUniversalProp.mBeginDelay ?? 0,
                                mEndDelay = subImageRes.mUniversalProp.mEndDelay ?? 0,
                                mFrameDelay = subImageRes.mUniversalProp.mFrameDelay ?? 0,
                            });
                        }
                    }
                    string res = string.Empty;
                    string replacedUnpackPath = unpackPath.Replace('\\', '/');
                    for (int i = 0; i < packinfo.mResLocs.Count; i++)
                    {
                        string thisRes = packinfo.mResLocs[i].mResPath;
                        if (replacedUnpackPath.Contains('/' + thisRes + '/', StringComparison.CurrentCulture))
                        {
                            res = thisRes;
                        }
                    }
                    if (!mSubImages.TryGetValue(res, out Dictionary<string, List<SpriteItem>>? value))
                    {
                        value = ([]);
                        mSubImages.Add(res, value);
                    }
                    value.TryAdd(atlasName, subImages);
                }
            }
            EnsureParentFolderExist(contentPath);
            File.Copy(tempPath, contentPath, true);
            return true;
        }

        private bool ParseAtlasResourceByProp(XmlElement theElement, AtlasRes atlasRes)
        {
            SetBooleanAsExist(theElement, "nopal", atlasRes.mNoPal);
            SetBooleanAsExist(theElement, "a4r4g4b4", atlasRes.mA4R4G4B4);
            SetBooleanAsExist(theElement, "ddsurface", atlasRes.mDDSurface);
            SetBooleanAsExist(theElement, "nobits", atlasRes.mNoBits);
            SetBooleanAsExist(theElement, "nobits2d", atlasRes.mNoBits2D);
            SetBooleanAsExist(theElement, "nobits3d", atlasRes.mNoBits3D);
            SetBooleanAsExist(theElement, "a8r8g8b8", atlasRes.mA8R8G8B8);
            SetBooleanAsExist(theElement, "r5g6b5", atlasRes.mR5G6B5);
            SetBooleanAsExist(theElement, "a1r5g5b5", atlasRes.mA1R5G5B5);
            SetBooleanAsExist(theElement, "minsubdivide", atlasRes.mMinSubdivide);
            SetBooleanAsExist(theElement, "noalpha", atlasRes.mNoAlpha);
            SetValueTypeIfExist(theElement, "surface", atlasRes.mSurface);
            if (atlasRes.mAlphaImage != null)
            {
                string alphaImage = atlasRes.mAlphaImage;
                if (alphaImage.StartsWith('/'))
                {
                    alphaImage = alphaImage[1..];
                }
                theElement.SetAttribute("alphaimage", alphaImage);
            }
            SetValueTypeIfExist(theElement, "alphacolor", atlasRes.mAlphaColor);
            SetStringIfExist(theElement, "variant", atlasRes.mVariant);
            SetStringIfExist(theElement, "alphagrid", atlasRes.mAlphaGrid);
            SetValueTypeIfExist(theElement, "languageSpecific", atlasRes.mLanguageSpecific);
            SetValueTypeIfExist(theElement, "format", atlasRes.mFormat);
            SetValueTypeIfExist(theElement, "atlasWidth", atlasRes.mWidth);
            SetValueTypeIfExist(theElement, "atlasHeight", atlasRes.mHeight);
            SetValueTypeIfExist(theElement, "atlasExtrude", atlasRes.mExtrude);
            return true;
        }

        private bool ParseReanimResource(ResBase<ReanimRes> reanimRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            string tempPath = GetTempPath(path) + ".xnb";
            if (true)
            {
                string tempMetaPath = GetTempMetaPath(path);
                string unpackPath = LoadImageExtension(GetUnpackPath(path), reanimRes.mDiskFormat);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null || !File.Exists(unpackPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mDiskFormat != reanimRes.mDiskFormat)
                {
                    rebuild = true;
                }
                else
                {
                    if (buildInfo.mHash != GetHash(unpackPath))
                    {
                        rebuild = true;
                    }
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildInfo();
                    ReanimatorDefinition reanim = XmlReanimCoder.Shared.Decode(unpackPath);
                    XnbReanimCoder.Shared.Encode(reanim, tempPath);
                    buildInfo.mDiskFormat = reanimRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            string contentPath = GetContentPath(path);
            EnsureParentFolderExist(contentPath);
            File.Copy(tempPath, contentPath, true);
            return true;
        }

        private bool ParseParticleResource(ResBase<ParticleRes> particleRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (true)
            {
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null || !File.Exists(unpackPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mDiskFormat != particleRes.mDiskFormat)
                {
                    rebuild = true;
                }
                else
                {
                    if (buildInfo.mHash != GetHash(unpackPath))
                    {
                        rebuild = true;
                    }
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = particleRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            string contentPath = GetContentPath(path);
            EnsureParentFolderExist(contentPath);
            File.Copy(GetTempPath(path), contentPath, true);
            return true;
        }

        private bool ParseTrailResource(ResBase<TrailRes> trailRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (true)
            {
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null || !File.Exists(unpackPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mDiskFormat != trailRes.mDiskFormat)
                {
                    rebuild = true;
                }
                else
                {
                    if (buildInfo.mHash != GetHash(unpackPath))
                    {
                        rebuild = true;
                    }
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = trailRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            string contentPath = GetContentPath(path);
            EnsureParentFolderExist(contentPath);
            File.Copy(GetTempPath(path), contentPath, true);
            return true;
        }

        private bool ParseSoundResource(XmlElement theElement, ResBase<SoundRes> soundRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (ParseCommonResource(theElement, soundRes, RemoveXnbExtension(path)))
            {
                ParseSoundResourceByProp(theElement, soundRes.mUniversalProp);
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null || !File.Exists(unpackPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mDiskFormat != soundRes.mDiskFormat)
                {
                    rebuild = true;
                }
                else
                {
                    if (buildInfo.mHash != GetHash(unpackPath))
                    {
                        rebuild = true;
                    }
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = soundRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            string contentPath = GetContentPath(path);
            EnsureParentFolderExist(contentPath);
            File.Copy(GetTempPath(path), contentPath, true);
            return true;
        }

        private bool ParseSoundResourceByProp(XmlElement theElement, SoundRes imageRes)
        {
            SetValueTypeIfExist(theElement, "volume", imageRes.mVolume);
            SetValueTypeIfExist(theElement, "pan", imageRes.mPan);
            return true;
        }

        private bool ParseFontResource(XmlElement theElement, ResBase<FontRes> fontRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (ParseCommonResource(theElement, fontRes, "Content/" + RemoveXnbExtension(path)))
            {
                ParseFontResourceByProp(theElement, fontRes.mUniversalProp);
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null || !File.Exists(unpackPath))
                {
                    rebuild = true;
                }
                else if (buildInfo.mDiskFormat != fontRes.mDiskFormat)
                {
                    rebuild = true;
                }
                else
                {
                    if (buildInfo.mHash != GetHash(unpackPath))
                    {
                        rebuild = true;
                    }
                }
                if (rebuild)
                {
                    EnsureParentFolderExist(tempPath);
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = fontRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            string contentPath = GetContentPath(path);
            EnsureParentFolderExist(contentPath);
            File.Copy(GetTempPath(path), contentPath, true);
            return true;
        }

        private bool ParseFontResourceByProp(XmlElement theElement, FontRes fontRes)
        {
            SetBooleanAsExist(theElement, "isDefault", fontRes.mIsDefault);
            SetBooleanAsExist(theElement, "truetype", fontRes.mTrueType);
            SetStringIfExist(theElement, "tags", fontRes.mTags);
            SetValueTypeIfExist(theElement, "size", fontRes.mSize);
            SetBooleanAsExist(theElement, "bold", fontRes.mBold);
            SetBooleanAsExist(theElement, "italic", fontRes.mItalic);
            SetBooleanAsExist(theElement, "shadow", fontRes.mShadow);
            SetBooleanAsExist(theElement, "underline", fontRes.mUnderline);
            SetValueTypeIfExist(theElement, "stroked", fontRes.mStroke);
            return true;
        }

        private static string RemoveXnbExtension(string path)
        {
            if (path.EndsWith(".xnb", StringComparison.CurrentCultureIgnoreCase))
            {
                return path[..^".xnb".Length];
            }
            return path;
        }

        private void EnsureParentFolderExist(string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private void GetAtlasPathsFromNativeUnpackMetaPath(string metaPath, TextureFormat inGameFormat, out string recordedPath, out string contentPath, out string unpackPath, out string tempPath, out string tempMetaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            tempMetaPath = GetTempMetaPath(path);
            tempPath = LoadImageExtension(GetTempPath(path), inGameFormat);
            unpackPath = GetUnpackPath(path);
            recordedPath = Path.Combine("images", Path.GetFileNameWithoutExtension(path));
            contentPath = LoadImageExtension(GetContentPath(('/' + path).Replace('\\', '/').Replace("/universal/", "/").Replace("/atlases/", "/images/").Replace('/', Path.DirectorySeparatorChar)[1..]), inGameFormat);
        }

        private void GetImagePathsFromNativeUnpackMetaPath(string metaPath, DiskFormat diskFormat, TextureFormat inGameFormat, out string recordedPath, out string contentPath, out string unpackPath, out string tempPath, out string tempMetaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            tempMetaPath = GetTempMetaPath(path);
            tempPath = LoadImageExtension(GetTempPath(path), inGameFormat);
            unpackPath = LoadImageExtension(GetUnpackPath(path), diskFormat);
            recordedPath = Path.Combine("images", Path.GetFileName(path));
            contentPath = LoadImageExtension(GetContentPath(path.Replace('\\', '/').Replace("/universal/", "/").Replace('/', Path.DirectorySeparatorChar)), inGameFormat);
        }

        private static string LoadImageExtension(string path, DiskFormat format)
        {
            if (format == DiskFormat.Xnb)
            {
                return path + ".xnb";
            }
            return path + "." + format.ToString().ToLower();
        }

        private static string LoadImageExtension(string path, TextureFormat format)
        {
            if (format == TextureFormat.Content)
            {
                return path + ".xnb";
            }
            return path + "." + format.ToString().ToLower();
        }

        private static IDisposableBitmap DecodeImageFromPath(string path, DiskFormat format)
        {
            if (format == DiskFormat.Xnb)
            {
                using (FileStream xnbStream = File.OpenRead(path))
                {
                    return XnbTexture2DCoder.Shared.ReadOne(Path.GetFileName(path), xnbStream);
                }
            }
            return new StbBitmap(path);
        }

        private static void EncodeImageToPath(IDisposableBitmap bitmap, string path, TextureFormat format, SurfaceFormat surface)
        {
            if (format == TextureFormat.Content)
            {
                using (FileStream outStream = File.Create(path))
                {
                    XnbTexture2DCoder textureWriter = new XnbTexture2DCoder();
                    textureWriter.mSurfaceFormat = surface;
                    textureWriter.WriteOne(bitmap, Path.GetFileName(path), outStream);
                }
            }
            else if (format == TextureFormat.Png)
            {
                bitmap.SaveAsPng(path);
            }
            else if (format == TextureFormat.Jpg)
            {
                bitmap.SaveAsJpg(path);
            }
        }

        private static string GetHash(string file)
        {
            using (SHA1 hash = SHA1.Create())
            {
                using (Stream fileStream = File.OpenRead(file))
                {
                    byte[] hashValue = hash.ComputeHash(fileStream);
                    return BitConverter.ToString(hashValue).Replace("-", string.Empty);
                }
            }
        }

        private void SetStringIfExist(XmlElement theElement, string name, string? value)
        {
            if (value != null)
            {
                theElement.SetAttribute(name, value);
            }
        }

        private void SetValueTypeIfExist<T>(XmlElement theElement, string name, T? value) where T : struct
        {
            if (value != null)
            {
                theElement.SetAttribute(name, value.Value.ToString()?.ToLower());
            }
        }

        private void SetBooleanAsExist(XmlElement theElement, string name, bool? value)
        {
            if (value == true)
            {
                theElement.SetAttribute(name, null);
            }
            else if (value == false)
            {
                theElement.RemoveAttribute(name);
            }
        }
    }
}
