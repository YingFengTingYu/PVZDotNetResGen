using PVZDotNetResGen.Sexy;
using PVZDotNetResGen.Utils.JsonHelper;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg.Downloader;

namespace PVZDotNetResGen.Cmd
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var unpackOption = new Option<DirectoryInfo?>(
                name: "--unpack",
                description: "unpack folder");

            unpackOption.AddAlias("-u");

            var contentOption = new Option<DirectoryInfo?>(
                name: "--content",
                description: "content folder");

            contentOption.AddAlias("-c");

            var sourceOption = new Option<DirectoryInfo?>(
                name: "--source",
                description: "Sexy/Resources folder");

            sourceOption.AddAlias("-s");

            var tempOption = new Option<DirectoryInfo?>(
                name: "--temp",
                description: "temp folder");

            tempOption.AddAlias("-t");

            var platformOption = new Option<string?>(
                name: "--platform",
                description: "build platform: PCDX,PCGL,Android,IOS,WebGL");

            platformOption.AddAlias("-p");

            var rootCommand = new RootCommand("PVZDotNetResGen v1.0 author: YingFengTingTu");

            var unpackCommand = new Command("unpack", "Unpack content folder");

            rootCommand.Add(unpackCommand);

            unpackCommand.AddOption(contentOption);
            unpackCommand.AddOption(unpackOption);
            unpackCommand.AddOption(sourceOption);

            unpackCommand.SetHandler((DirectoryInfo? content, DirectoryInfo? unpack, DirectoryInfo? source) =>
            {
                if (content != null && unpack != null && source != null)
                {
                    Console.WriteLine("Start unpacking...");
                    ResourceUnpacker unpacker = new ResourceUnpacker(content.FullName, source.FullName, unpack.FullName);
                    IEnumerator<bool> updater = unpacker.Update();
                    while (updater.MoveNext())
                    {

                    }
                    Console.WriteLine("Finish unpacking!");
                }
            }, contentOption, unpackOption, sourceOption);

            var packCommand = new Command("pack", "Pack content folder");

            rootCommand.Add(packCommand);

            packCommand.AddOption(contentOption);
            packCommand.AddOption(unpackOption);
            packCommand.AddOption(sourceOption);
            packCommand.AddOption(tempOption);

            packCommand.SetHandler((DirectoryInfo? content, DirectoryInfo? unpack, DirectoryInfo? source, DirectoryInfo? temp, string? platformString) =>
            {
                if (content != null && unpack != null && source != null && temp != null)
                {
                    Console.WriteLine("Start packing...");
                    BuildPlatform platform;
                    switch (platformString)
                    {
                        case "PCDX":
                            platform = BuildPlatform.PCDX; break;
                        case "PCGL":
                            platform = BuildPlatform.PCGL; break;
                        case "Android":
                            platform = BuildPlatform.Android; break;
                        case "IOS":
                            platform = BuildPlatform.IOS; break;
                        case "WebGL":
                            platform = BuildPlatform.WebGL; break;
                        default:
                            Console.WriteLine("Unknown platform {0}. The program will use PCDX.");
                            platform = BuildPlatform.PCDX; break;
                    }
                    ResourcePacker packer = new ResourcePacker(content.FullName, source.FullName, unpack.FullName, temp.FullName, platform);
                    IEnumerator<bool> updater = packer.Update();
                    while (updater.MoveNext())
                    {

                    }
                    Console.WriteLine("Finish packing!");
                }
            }, contentOption, unpackOption, sourceOption, tempOption, platformOption);

            var downloadCommand = new Command("download", "Download ffmpeg from ffbinaries");

            rootCommand.Add(downloadCommand);

            downloadCommand.SetHandler(() =>
            {
                return FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            });

            return await rootCommand.InvokeAsync(args);
        }
    }
}
