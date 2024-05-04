﻿using PVZDotNetResGen.Sexy.Image;
using PVZDotNetResGen.Utils.Graphics;
using PVZDotNetResGen.Utils.Graphics.Bitmap;
using PVZDotNetResGen.Utils.JsonHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
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
        private HashSet<string> mExistedImageId = [];

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
                    }
                    else if (resBase is ResBase<AtlasRes> atlasResBase)
                    {
                        if (mExistedImageId.Contains(atlasResBase.mId))
                        {
                            ParseAtlasResource(null, atlasResBase, metaFile);
                        }
                        else
                        {
                            XmlElement resNode = xmlDocResources.CreateElement("Image");
                            ParseAtlasResource(resNode, atlasResBase, metaFile);
                            mXmlNodeList[atlasResBase.mGroup].AppendChild(resNode);
                            mExistedImageId.Add(atlasResBase.mId);
                        }
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
                yield return false;
            }
            xmlDocResources.Save(GetContentPath("resources.xml"));
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

        private bool ParseAtlasResource(XmlElement? theElement, ResBase<AtlasRes> imageRes, string metaPath)
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
                if (buildInfo == null || !Directory.Exists(unpackPath))
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
                else
                {
                    rebuild = true;
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
                    foreach (var png in Directory.GetFiles(unpackPath, "*.png", SearchOption.AllDirectories))
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
                    MaxRectsBinPack binPack = new MaxRectsBinPack(width, height, false);
                    int sizeOld = sizeList.Count;
                    binPack.Insert(sizeList, ansList, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit);
                    if (sizeOld > ansList.Count)
                    {
                        throw new Exception($"The size of atlas {metaPath} is too small!");
                    }
                    using (IDisposableBitmap bitmap = new NativeBitmap(width, height))
                    {
                        RefBitmap atlasBitmapRef = bitmap.AsRefBitmap();
                        for (int i = 0; i < ansList.Count; i++)
                        {
                            MaxRectsBinPack.BinRect rect = ansList[i];
                            Debug.Assert(rect.id != null);
                            using (StbBitmap pngBitmap = new StbBitmap(rect.id))
                            {
                                pngBitmap.AsRefBitmap().CopyTo(atlasBitmapRef, 0, 0, rect.x + extrude, rect.y + extrude);
                            }
                        }
                        EncodeImageToPath(bitmap, tempPath, aInGameFormat, aSurfaceFormat);
                    }
                    buildInfo.mFormat = aInGameFormat;
                    buildInfo.mSurface = aSurfaceFormat;
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
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
                    File.Copy(unpackPath, tempPath, true);
                    buildInfo.mDiskFormat = reanimRes.mDiskFormat;
                    buildInfo.mHash = GetHash(unpackPath);
                    AOTJson.TrySerializeToFile(tempMetaPath, buildInfo);
                }
            }
            string contentPath = GetContentPath(path);
            EnsureParentFolderExist(contentPath);
            File.Copy(GetTempPath(path), contentPath, true);
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
                    return XnbTexture2D.Shared.ReadOne(Path.GetFileName(path), xnbStream);
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
                    XnbTexture2D textureWriter = new XnbTexture2D();
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
