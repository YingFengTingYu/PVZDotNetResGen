using PVZDotNetResGen.Sexy;
using PVZDotNetResGen.Sexy.Atlas;
using PVZDotNetResGen.Sexy.Image;
using PVZDotNetResGen.Sexy.Reanim;
using PVZDotNetResGen.Utils.Graphics;
using PVZDotNetResGen.Utils.Graphics.Bitmap;
using PVZDotNetResGen.Utils.XnbContent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PVZDotNetResGen.Tests
{
    public class UnpackTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        public void ExtractAtlasInfoFromCode(string inPath, string outFolder)
        {
            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }
            WPAtlasInfoAnalyzer.UnpackAsJson(inPath, outFolder);
        }

        [TestCase("D:\\CSharp\\pvz_combination\\Lawn_PCDX\\bin\\Debug\\net6.0-windows\\Content", "D:\\CSharp\\pvz_combination\\Lawn_Shared\\Sexy\\Resource", "D:\\整合版解包")]
        public void UnpackResources(string inFolder, string codeFolder, string outFolder)
        {
            ResourceUnpacker unpacker = new ResourceUnpacker(inFolder, codeFolder, outFolder);
            IEnumerator<bool> updater = unpacker.Update();
            while (updater.MoveNext())
            {

            }
        }

        [TestCase("D:\\CSharp\\pvz_combination_resources\\unpack",
            "D:\\CSharp\\pvz_combination\\Lawn_Shared\\Sexy\\Resource",
            "D:\\CSharp\\pvz_combination\\Lawn_PCDX\\bin\\Debug\\net6.0-windows\\Content",
            "D:\\CSharp\\pvz_combination_resources\\cache\\pcdx",
            BuildPlatform.PCDX)]
        [TestCase("D:\\CSharp\\pvz_combination_resources\\unpack",
            "D:\\CSharp\\pvz_combination\\Lawn_Shared\\Sexy\\Resource",
            "D:\\CSharp\\pvz_combination\\Lawn_Android\\Assets\\Content",
            "D:\\CSharp\\pvz_combination_resources\\cache\\android",
            BuildPlatform.Android)]
        public void PackResources(string unpackFolder, string codeFolder, string contentFolder, string tempFolder, BuildPlatform platform)
        {
            ResourcePacker packer = new ResourcePacker(contentFolder, codeFolder, unpackFolder, tempFolder, platform);
            IEnumerator<bool> updater = packer.Update();
            while (updater.MoveNext())
            {

            }
        }

        public void DecodeXnbTexture(string inPath, string outPath)
        {
            using (FileStream inStream = File.OpenRead(inPath))
            {
                using IDisposableBitmap bitmap = (IDisposableBitmap)XnbHelper.Decode(Path.GetFileName(inPath), inStream).PrimaryResource;
                bitmap.SaveAsPng(outPath);
            }
        }

        public void EncodeXnbTexture(string inPath, string outPath)
        {
            using (StbBitmap bitmap = new StbBitmap(inPath))
            {
                using (FileStream outStream = File.Create(outPath))
                {
                    XnbTexture2DCoder.SurfaceFormat = SurfaceFormat.Rgba8Etc2;
                    XnbHelper.Encode(new XnbContent(bitmap, 0), Path.GetFileName(outPath), outStream);
                }
            }
        }

        public void DecodeXnbReanim(string inPath, string outPath)
        {
            ReanimatorDefinition reanim = XnbReanimCoder.Shared.Decode(inPath);
            XmlReanimCoder.Shared.Encode(reanim, outPath);
        }

        public void EncodeXnbReanim(string inPath, string outPath)
        {
            ReanimatorDefinition reanim = XmlReanimCoder.Shared.Decode(inPath);
            Console.WriteLine(JsonSerializer.Serialize(reanim, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true,
            }));
            XnbReanimCoder.Shared.Encode(reanim, outPath);
        }
    }
}