using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PVZDotNetResGen.Utils.Compression;
using PVZDotNetResGen.Utils.StreamHelper;

namespace PVZDotNetResGen.Utils.XnbContent;

public static class XnbHelper
{
    private const byte CONTENT_COMPRESSED_LZX = 0x80;
    private const byte CONTENT_COMPRESSED_LZ4 = 0x40;

    private static readonly FrozenSet<char> TargetPlatformIdentifiers =
        new HashSet<char>
        {
            'w', // Windows (XNA & DirectX)
            'x', // Xbox360 (XNA)
            'i', // iOS
            'a', // Android
            'd', // DesktopGL
            'X', // MacOSX
            'W', // WindowsStoreApp
            'n', // NativeClient
            'M', // WindowsPhone8
            'r', // RaspberryPi
            'P', // PlayStation4
            '5', // PlayStation5
            'O', // XboxOne
            'S', // Nintendo Switch
            'G', // Google Stadia
            'b', // WebAssembly and Bridge.NET

            // NOTE: There are additional identifiers for consoles that 
            // are not defined in this repository.  Be sure to ask the
            // console port maintainers to ensure no collisions occur.


            // Legacy identifiers... these could be reused in the
            // future if we feel enough time has passed.

            'm', // WindowsPhone7.0 (XNA)
            'p', // PlayStationMobile
            'v', // PSVita
            'g', // Windows (OpenGL)
            'l', // Linux
        }.ToFrozenSet();

    public static XnbContent Decode(string originalAssetName, Stream stream)
    {
        // The first 4 bytes should be the "XNB" header used to detect an invalid file
        byte x = stream.ReadUInt8();
        byte n = stream.ReadUInt8();
        byte b = stream.ReadUInt8();
        byte platform = stream.ReadUInt8();

        if (x != 'X' || n != 'N' || b != 'B' || !TargetPlatformIdentifiers.Contains((char)platform))
        {
            throw new Exception(
                "Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
        }

        // The XNA Game Studio 4.0 xnb version is 5
        byte version = stream.ReadUInt8();
        byte flags = stream.ReadUInt8();
        bool compressedLzx = (flags & CONTENT_COMPRESSED_LZX) != 0;
        bool compressedLz4 = (flags & CONTENT_COMPRESSED_LZ4) != 0;
        if (version != 5)
        {
            throw new Exception("Invalid XNB version");
        }

        // The next int32 is the length of the XNB file
        int xnbLength = stream.ReadInt32LE();

        Stream decompressedStream = stream;
        if (compressedLzx || compressedLz4)
        {
            // Decompress the xnb
            int decompressedSize = stream.ReadInt32LE();

            if (compressedLzx)
            {
                int compressedSize = xnbLength - 14;
                decompressedStream = new LzxDecoderStream(stream, decompressedSize, compressedSize);
            }
            else if (compressedLz4)
            {
                decompressedStream = new Lz4DecoderStream(stream);
            }
        }

        int numberOfReaders = decompressedStream.Read7BitEncodedInt32();
        IXnbContentCoder[] coders = new IXnbContentCoder[numberOfReaders];

        for (int i = 0; i < numberOfReaders; i++)
        {
            // This string tells us what reader we need to decode the following data
            // string readerTypeString = reader.ReadString();
            string originalReaderTypeString =
                decompressedStream.ReadString(decompressedStream.Read7BitEncodedInt32(), System.Text.Encoding.UTF8);
            Debug.Assert(XnbCoderManager.Get(originalReaderTypeString, out coders[i]!));

            // I think the next 4 bytes refer to the "Version" of the type reader,
            // although it always seems to be zero
            decompressedStream.ReadInt32LE();
        }

        int sharedResourceCount = decompressedStream.Read7BitEncodedInt32();

        object primary = coders[decompressedStream.Read7BitEncodedInt32() - 1].ReadContent(decompressedStream, originalAssetName, version);

        XnbContent content = new XnbContent(primary, sharedResourceCount);

        // Initialize any new readers.
        for (int i = 0; i < sharedResourceCount; i++)
        {
            int typeReaderIndex = decompressedStream.Read7BitEncodedInt32();
            if (typeReaderIndex == 0)
            {
                content.SharedResources[i] = null;
            }
            else
            {
                content.SharedResources[i] = coders[typeReaderIndex - 1]
                    .ReadContent(decompressedStream, originalAssetName, version);
            }
        }

        return content;
    }

    public static void Encode(XnbContent content, string originalAssetName, Stream stream, bool compressedLz4 = false, char platform = 'm')
    {
        // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
        stream.WriteUInt8((byte)'X');
        stream.WriteUInt8((byte)'N');
        stream.WriteUInt8((byte)'B');
        
        Debug.Assert(TargetPlatformIdentifiers.Contains(platform));
        stream.WriteUInt8((byte)platform); // platform

        stream.WriteUInt8(5); // version
        byte flags = 0;
        if (compressedLz4)
        {
            flags |= CONTENT_COMPRESSED_LZ4;
        }

        stream.WriteUInt8(flags); // flags

        using (MemoryStream memoryStream = new MemoryStream())
        {
            List<IXnbContentCoder> coders = [];
            Dictionary<Type, int> indexMap = [];

            // Get all content coder type

            Debug.Assert(XnbCoderManager.Get(content.PrimaryResource.GetType(), out IXnbContentCoder? mainCoder));
            indexMap.Add(content.PrimaryResource.GetType(), 0);
            coders.Add(mainCoder);

            for (int i = 0; i < content.SharedResources.Length; i++)
            {
                object? shared = content.SharedResources[i];
                if (shared != null)
                {
                    Type type = shared.GetType();
                    Debug.Assert(XnbCoderManager.Get(type, out IXnbContentCoder? coder));
                    if (!coders.Contains(coder))
                    {
                        indexMap.Add(type, coders.Count);
                        coders.Add(coder);
                    }
                }
            }

            // Write coder info
            memoryStream.Write7BitEncodedInt32(coders.Count);
            for (int i = 0; i < coders.Count; i++)
            {
                // This string tells us what reader we need to decode the following data
                // string readerTypeString = reader.ReadString();
                memoryStream.WriteStringWith7BitEncodedInt32Head(coders[i].ReaderTypeString,
                    System.Text.Encoding.UTF8);

                // I think the next 4 bytes refer to the "Version" of the type reader,
                // although it always seems to be zero
                memoryStream.WriteInt32LE(0);
            }

            memoryStream.Write7BitEncodedInt32(content.SharedResources.Length);
            
            memoryStream.Write7BitEncodedInt32(1);
            coders[0].WriteContent(content.PrimaryResource, memoryStream, originalAssetName, 5);

            // Initialize any new readers.
            for (int i = 0; i < content.SharedResources.Length; i++)
            {
                object? shared = content.SharedResources[i];
                if (shared == null)
                {
                    memoryStream.Write7BitEncodedInt32(0);
                }
                else
                {
                    int readerIndex = indexMap[shared.GetType()];
                    memoryStream.Write7BitEncodedInt32(readerIndex + 1);
                    coders[readerIndex].WriteContent(shared, memoryStream, originalAssetName, 5);
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            if (compressedLz4)
            {
                // Compress stream
                var maxLength = LZ4Codec.MaximumOutputLength((int)memoryStream.Length);
                var outputArray = ArrayPool<byte>.Shared.Rent(maxLength * 2);
                try
                {
                    int resultLength = LZ4Codec.Encode32HC(memoryStream.GetBuffer(), 0, (int)memoryStream.Length,
                        outputArray, 0, maxLength);
                    Debug.Assert(resultLength > 0);
                    stream.WriteInt32LE((int)stream.Position + resultLength + 8);
                    stream.WriteInt32LE((int)memoryStream.Length);
                    stream.Write(outputArray, 0, resultLength);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(outputArray);
                }
            }
            else
            {
                stream.WriteInt32LE((int)stream.Position + (int)memoryStream.Length + 4);
                memoryStream.CopyTo(stream);
            }
        }
    }
}