using PVZDotNetResGen.Sexy;
using PVZDotNetResGen.Sexy.Atlas;
using PVZDotNetResGen.Sexy.Image;
using PVZDotNetResGen.Utils.Graphics;
using PVZDotNetResGen.Utils.Graphics.Bitmap;
using System.Collections.Generic;
using System.IO;

namespace PVZDotNetResGen.Tests
{
    public class UnpackTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TestCase("files/AtlasResources_480x800.csf", "files/UnpackTests/CreateAtlasJson")]
        public void ExtractAtlasInfoFromCode(string inPath, string outFolder)
        {
            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }
            WPAtlasInfoAnalyzer.UnpackAsJson(inPath, outFolder);
        }

        [TestCase("D:\\CSharp\\PVZ.NET\\Lawn_PCDX\\Content", "D:\\CSharp\\PVZ.NET\\Lawn_Shared\\Sexy\\Resource", "files/UnpackTests/Unpack")]
        public void UnpackResources(string inFolder, string codeFolder, string outFolder)
        {
            ResourceUnpacker unpacker = new ResourceUnpacker(inFolder, codeFolder, outFolder);
            IEnumerator<bool> updater = unpacker.Update();
            while (updater.MoveNext())
            {

            }
        }

        [TestCase("files/UnpackTests/Unpack", "D:\\test\\code", "D:\\test\\real", "D:\\test\\temp")]
        public void PackResources(string unpackFolder, string codeFolder, string contentFolder, string tempFolder)
        {
            Directory.CreateDirectory("files/UnpackTests");
            ResourcePacker packer = new ResourcePacker(contentFolder, codeFolder, unpackFolder, tempFolder);
            IEnumerator<bool> updater = packer.Update();
            while (updater.MoveNext())
            {

            }
        }

        [TestCase("D:\\PlantsZombies.xnb", "D:\\PlantsZombies.png")]
        public void DecodeXnbTexture(string inPath, string outPath)
        {
            using (FileStream inStream = File.OpenRead(inPath))
            {
                XnbTexture2DCoder textureReader = new XnbTexture2DCoder();
                using IDisposableBitmap bitmap = textureReader.ReadOne(Path.GetFileName(inPath), inStream);
                bitmap.SaveAsPng(outPath);
            }
        }

        [TestCase("D:\\PlantsZombies.png", "D:\\PlantsZombiesWind.xnb")]
        public void EncodeXnbTexture(string inPath, string outPath)
        {
            using (StbBitmap bitmap = new StbBitmap(inPath))
            {
                using (FileStream outStream = File.Create(outPath))
                {
                    XnbTexture2DCoder textureWriter = new XnbTexture2DCoder();
                    textureWriter.mSurfaceFormat = SurfaceFormat.Rgba8Etc2;
                    textureWriter.WriteOne(bitmap, Path.GetFileName(outPath), outStream);
                }
            }
        }
    }
}