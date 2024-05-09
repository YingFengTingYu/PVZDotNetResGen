using System.IO;
using System.Collections.Generic;
using System;
using PVZDotNetResGen.Utils.StreamHelper;
using System.Diagnostics;

namespace PVZDotNetResGen.Utils.XnbContent
{
    public abstract class XnbBase<T>
    {
        private const byte ContentCompressedLzx = 0x80;
        private const byte ContentCompressedLz4 = 0x40;

        private static readonly List<char> targetPlatformIdentifiers =
        [
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
        ];

        protected abstract string ReaderTypeString { get; }

        public T[] ReadArray(string originalAssetName, Stream stream)
        {
            // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
            byte x = stream.ReadUInt8();
            byte n = stream.ReadUInt8();
            byte b = stream.ReadUInt8();
            byte platform = stream.ReadUInt8();

            if (x != 'X' || n != 'N' || b != 'B' || !targetPlatformIdentifiers.Contains((char)platform))
            {
                throw new Exception("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
            }

            byte version = stream.ReadUInt8();
            byte flags = stream.ReadUInt8();

            bool compressedLzx = (flags & ContentCompressedLzx) != 0;
            bool compressedLz4 = (flags & ContentCompressedLz4) != 0;
            if (version != 5 && version != 4)
            {
                throw new Exception("Invalid XNB version");
            }

            // The next int32 is the length of the XNB file
            int xnbLength = stream.ReadInt32LE();

            Stream? decompressedStream = stream;
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
            T[] arr = new T[numberOfReaders];
            for (int i = 0; i < numberOfReaders; i++)
            {
                // This string tells us what reader we need to decode the following data
                // string readerTypeString = reader.ReadString();
                string originalReaderTypeString = decompressedStream.ReadString(decompressedStream.Read7BitEncodedInt32(), encoding: System.Text.Encoding.UTF8);
                Debug.Assert(originalReaderTypeString == ReaderTypeString);

                // I think the next 4 bytes refer to the "Version" of the type reader,
                // although it always seems to be zero
                decompressedStream.ReadInt32LE();
            }

            int sharedResourceCount = decompressedStream.Read7BitEncodedInt32();

            // Initialize any new readers.
            for (int i = 0; i < numberOfReaders; i++)
            {
                int typeReaderIndex = decompressedStream.Read7BitEncodedInt32();
                arr[i] = ReadContent(decompressedStream, originalAssetName, version);
            }

            return arr;
        }

        public T ReadOne(string originalAssetName, Stream stream)
        {
            // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
            byte x = stream.ReadUInt8();
            byte n = stream.ReadUInt8();
            byte b = stream.ReadUInt8();
            byte platform = stream.ReadUInt8();

            if (x != 'X' || n != 'N' || b != 'B' || !targetPlatformIdentifiers.Contains((char)platform))
            {
                throw new Exception("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
            }

            byte version = stream.ReadUInt8();
            byte flags = stream.ReadUInt8();

            bool compressedLzx = (flags & ContentCompressedLzx) != 0;
            bool compressedLz4 = (flags & ContentCompressedLz4) != 0;
            if (version != 5 && version != 4)
            {
                throw new Exception("Invalid XNB version");
            }

            // The next int32 is the length of the XNB file
            int xnbLength = stream.ReadInt32LE();

            Stream? decompressedStream = stream;
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
            Debug.Assert(numberOfReaders == 1);
            // This string tells us what reader we need to decode the following data
            // string readerTypeString = reader.ReadString();
            string originalReaderTypeString = decompressedStream.ReadString(decompressedStream.Read7BitEncodedInt32(), encoding: System.Text.Encoding.UTF8);
            Debug.Assert(originalReaderTypeString == ReaderTypeString);

            // I think the next 4 bytes refer to the "Version" of the type reader,
            // although it always seems to be zero
            decompressedStream.ReadInt32LE();

            int sharedResourceCount = decompressedStream.Read7BitEncodedInt32();
            int typeReaderIndex = decompressedStream.Read7BitEncodedInt32();

            // Initialize any new readers.
            return ReadContent(decompressedStream, originalAssetName, version);
        }

        public abstract T ReadContent(Stream stream, string originalAssetName, byte version);

        public void WriteArray(T[] array, string originalAssetName, Stream stream)
        {
            // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
            stream.WriteUInt8((byte)'X');
            stream.WriteUInt8((byte)'N');
            stream.WriteUInt8((byte)'B');
            stream.WriteUInt8((byte)'m'); // platform

            stream.WriteUInt8(5); // version
            stream.WriteUInt8(0); // flags

            // The next int32 is the length of the XNB file
            long xnbLengthPosition = stream.Position;
            stream.WriteInt32LE(0);

            Stream? compressedStream = stream;

            compressedStream.Write7BitEncodedInt32(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                // This string tells us what reader we need to decode the following data
                // string readerTypeString = reader.ReadString();
                compressedStream.Write7BitEncodedInt32(ReaderTypeString.Length);
                compressedStream.WriteString(ReaderTypeString, encoding: System.Text.Encoding.UTF8);

                // I think the next 4 bytes refer to the "Version" of the type reader,
                // although it always seems to be zero
                compressedStream.WriteInt32LE(0);
            }

            compressedStream.Write7BitEncodedInt32(0);

            // Initialize any new readers.
            for (int i = 0; i < array.Length; i++)
            {
                compressedStream.Write7BitEncodedInt32(1);
                WriteContent(array[i], compressedStream, originalAssetName, 5);
            }
            stream.Seek(xnbLengthPosition, SeekOrigin.Begin);
            stream.WriteInt32LE((int)stream.Length);
            stream.Seek(0, SeekOrigin.End);
        }

        public void WriteOne(T content, string originalAssetName, Stream stream)
        {
            // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
            stream.WriteUInt8((byte)'X');
            stream.WriteUInt8((byte)'N');
            stream.WriteUInt8((byte)'B');
            stream.WriteUInt8((byte)'m'); // platform

            stream.WriteUInt8(5); // version
            stream.WriteUInt8(0); // flags

            // The next int32 is the length of the XNB file
            long xnbLengthPosition = stream.Position;
            stream.WriteInt32LE(0);

            Stream? compressedStream = stream;

            compressedStream.Write7BitEncodedInt32(1);
            // This string tells us what reader we need to decode the following data
            // string readerTypeString = reader.ReadString();
            compressedStream.Write7BitEncodedInt32(ReaderTypeString.Length);
            compressedStream.WriteString(ReaderTypeString, encoding: System.Text.Encoding.UTF8);

            // I think the next 4 bytes refer to the "Version" of the type reader,
            // although it always seems to be zero
            compressedStream.WriteInt32LE(0);

            compressedStream.Write7BitEncodedInt32(0);

            // Initialize any new readers.
            compressedStream.Write7BitEncodedInt32(1);
            WriteContent(content, compressedStream, originalAssetName, 5);
            stream.Seek(xnbLengthPosition, SeekOrigin.Begin);
            stream.WriteInt32LE((int)stream.Length);
            stream.Seek(0, SeekOrigin.End);
        }

        public abstract void WriteContent(T content, Stream stream, string originalAssetName, byte version);
    }
}
