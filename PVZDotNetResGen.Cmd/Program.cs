using PVZDotNetResGen.Sexy;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

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

            packCommand.SetHandler((DirectoryInfo? content, DirectoryInfo? unpack, DirectoryInfo? source, DirectoryInfo? temp) =>
            {
                if (content != null && unpack != null && source != null && temp != null)
                {
                    Console.WriteLine("Start packing...");
                    ResourcePacker packer = new ResourcePacker(content.FullName, source.FullName, unpack.FullName, temp.FullName);
                    IEnumerator<bool> updater = packer.Update();
                    while (updater.MoveNext())
                    {

                    }
                    Console.WriteLine("Finish packing!");
                }
            }, contentOption, unpackOption, sourceOption, tempOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
