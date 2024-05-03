using PVZDotNetResGen.Utils.JsonHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;

namespace PVZDotNetResGen.Sexy
{
    public class ResourcePacker(string contentFolderPath, string codeFolderPath, string unpackFolderPath, string buildCacheFolderPath)
    {
        private readonly string mContentFolderPath = contentFolderPath;
        private readonly string mCodeFolderPath = codeFolderPath;
        private readonly string mUnpackFolderPath = unpackFolderPath;
        private readonly string mBuildCacheFolderPath = buildCacheFolderPath;
        private string mDefaultPath = "/";
        private string mDefaultIdPrefix = "";
        private Dictionary<string, XmlNode> mXmlNodeList = [];

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
                        //XmlElement resNode = xmlDocResources.CreateElement("Image");
                        //ParseImageResource(resNode, imageResBase, GetRecordedPathFromUnpackMetaPath(metaFile));
                        //mXmlNodeList[imageResBase.mGroup].AppendChild(resNode);
                    }
                    else if (resBase is ResBase<AtlasRes> atlasResBase)
                    {

                    }
                    else if (resBase is ResBase<ReanimRes> reanimResBase)
                    {
                        XmlElement resNode = xmlDocResources.CreateElement("Reanim");
                        ParseReanimResource(resNode, reanimResBase, metaFile);
                        mXmlNodeList[reanimResBase.mGroup].AppendChild(resNode);
                    }
                    else if (resBase is ResBase<ParticleRes> particleResBase)
                    {
                        XmlElement resNode = xmlDocResources.CreateElement("Particle");
                        ParseParticleResource(resNode, particleResBase, metaFile);
                        mXmlNodeList[particleResBase.mGroup].AppendChild(resNode);
                    }
                    else if (resBase is ResBase<TrailRes> trailResBase)
                    {
                        XmlElement resNode = xmlDocResources.CreateElement("Trail");
                        ParseTrailResource(resNode, trailResBase, metaFile);
                        mXmlNodeList[trailResBase.mGroup].AppendChild(resNode);
                    }
                    else if (resBase is ResBase<SoundRes> soundResBase)
                    {
                        XmlElement resNode = xmlDocResources.CreateElement("Sound");
                        ParseSoundResource(resNode, soundResBase, metaFile);
                        mXmlNodeList[soundResBase.mGroup].AppendChild(resNode);
                    }
                    else if (resBase is ResBase<FontRes> fontResBase)
                    {
                        XmlElement resNode = xmlDocResources.CreateElement("Font");
                        ParseFontResource(resNode, fontResBase, metaFile);
                        mXmlNodeList[fontResBase.mGroup].AppendChild(resNode);
                    }
                }
            }
            yield return false;


        }

        private bool ParseCommonResource<T>(XmlElement theElement, ResBase<T> theRes, string path) where T : PlatformProperties, new()
        {
            theElement.SetAttribute("id", theRes.mId);
            theElement.SetAttribute("path", path);
            if (theRes.mUniversalProp.mUnloadGroup != null)
            {
                theElement.SetAttribute("unloadGroup", Convert.ToString(theRes.mUniversalProp.mUnloadGroup));
            }
            return true;
        }

        private bool ParseImageResource(XmlElement theElement, ResBase<ImageRes> imageRes, string path)
        {
            if (ParseCommonResource(theElement, imageRes, path))
            {
                ParseImageResourceByProp(theElement, imageRes.mUniversalProp);
            }
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

        private bool ParseReanimResource(XmlElement theElement, ResBase<ReanimRes> reanimRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (ParseCommonResource(theElement, reanimRes, RemoveXnbExtension(path)))
            {
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null)
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
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = reanimRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            return true;
        }

        private bool ParseParticleResource(XmlElement theElement, ResBase<ParticleRes> particleRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (ParseCommonResource(theElement, particleRes, RemoveXnbExtension(path)))
            {
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null)
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
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = particleRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            return true;
        }

        private bool ParseTrailResource(XmlElement theElement, ResBase<TrailRes> trailRes, string metaPath)
        {
            string path = GetRecordedPathFromUnpackMetaPath(metaPath);
            if (ParseCommonResource(theElement, trailRes, RemoveXnbExtension(path)))
            {
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null)
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
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = trailRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
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
                if (buildInfo == null)
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
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = soundRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
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
            if (ParseCommonResource(theElement, fontRes, RemoveXnbExtension(path)))
            {
                ParseFontResourceByProp(theElement, fontRes.mUniversalProp);
                string tempMetaPath = GetTempMetaPath(path);
                string tempPath = GetTempPath(path);
                string unpackPath = GetUnpackPath(path);
                bool rebuild = false;
                BuildInfo? buildInfo = AOTJson.TryDeserializeFromFile<BuildInfo>(tempMetaPath);
                if (buildInfo == null)
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
                    buildInfo = new BuildInfo();
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = fontRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
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
