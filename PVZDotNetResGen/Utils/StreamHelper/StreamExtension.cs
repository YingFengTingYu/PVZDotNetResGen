using PVZDotNetResGen.Utils.MemoryHelper;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PVZDotNetResGen.Utils.StreamHelper
{
    internal static unsafe class StreamExtension
    {
        public static byte ReadUInt8(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[1];
            stream.Read(buffer);
            return buffer[0];
        }

        public static ushort ReadUInt16LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[2];
            stream.Read(buffer);
            return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        public static ushort ReadUInt16BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[2];
            stream.Read(buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public static uint ReadUInt32LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        public static uint ReadUInt32BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }

        public static ulong ReadUInt64LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[8];
            stream.Read(buffer);
            return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        public static ulong ReadUInt64BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[8];
            stream.Read(buffer);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public static sbyte ReadInt8(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[1];
            stream.Read(buffer);
            return (sbyte)buffer[0];
        }

        public static short ReadInt16LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[2];
            stream.Read(buffer);
            return BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        public static short ReadInt16BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[2];
            stream.Read(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public static int ReadInt32LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            return BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        public static int ReadInt32BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public static long ReadInt64LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[8];
            stream.Read(buffer);
            return BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        public static long ReadInt64BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[8];
            stream.Read(buffer);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public static float ReadFloat32LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            return BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        public static float ReadFloat32BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public static double ReadFloat64LE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[8];
            stream.Read(buffer);
            return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        public static double ReadFloat64BE(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[8];
            stream.Read(buffer);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }

        public static bool ReadBoolean(this Stream stream)
        {
            return stream.ReadUInt8() != 0;
        }

        public static char ReadCharLE(this Stream stream)
        {
            return (char)stream.ReadUInt16LE();
        }

        public static char ReadCharBE(this Stream stream)
        {
            return (char)stream.ReadUInt16BE();
        }

        public static int Read7BitEncodedInt32(this Stream stream)
        {
            // Unlike writing, we can't delegate to the 64-bit read on
            // 64-bit platforms. The reason for this is that we want to
            // stop consuming bytes if we encounter an integer overflow.

            uint result = 0;
            byte byteReadJustNow;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 5 bytes,
            // or the fifth byte is about to cause integer overflow.
            // This means that we can read the first 4 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 4;
            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = stream.ReadUInt8();
                result |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu)
                {
                    return (int)result; // early exit
                }
            }

            // Read the 5th byte. Since we already read 28 bits,
            // the value of this byte must fit within 4 bits (32 - 28),
            // and it must not have the high bit set.

            byteReadJustNow = stream.ReadUInt8();
            if (byteReadJustNow > 0b_1111u)
            {
                throw new FormatException("7BitEncodedInt32 Error");
            }

            result |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
            return (int)result;
        }

        public static long Read7BitEncodedInt64(this Stream stream)
        {
            ulong result = 0;
            byte byteReadJustNow;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 10 bytes,
            // or the tenth byte is about to cause integer overflow.
            // This means that we can read the first 9 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 9;
            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = stream.ReadUInt8();
                result |= (byteReadJustNow & 0x7Ful) << shift;

                if (byteReadJustNow <= 0x7Fu)
                {
                    return (long)result; // early exit
                }
            }

            // Read the 10th byte. Since we already read 63 bits,
            // the value of this byte must fit within 1 bit (64 - 63),
            // and it must not have the high bit set.

            byteReadJustNow = stream.ReadUInt8();
            if (byteReadJustNow > 0b_1u)
            {
                throw new FormatException("7BitEncodedInt64 Error");
            }

            result |= (ulong)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
            return (long)result;
        }

        public static string ReadString(this Stream stream, int size, Encoding encoding)
        {
            if (size <= 0)
            {
                return string.Empty;
            }
            if (size <= 0x800)
            {
                Span<byte> buffer = stackalloc byte[size];
                stream.Read(buffer);
                return encoding.GetString(buffer);
            }
            using (NativeMemoryOwner owner = new NativeMemoryOwner((uint)size))
            {
                Span<byte> buffer = owner.AsSpan();
                stream.Read(buffer);
                return encoding.GetString(buffer);
            }
        }

        public static void WriteUInt8(this Stream stream, byte value)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = value;
            stream.Write(buffer);
        }

        public static void WriteUInt16LE(this Stream stream, ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteUInt16BE(this Stream stream, ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteUInt32LE(this Stream stream, uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteUInt32BE(this Stream stream, uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteUInt64LE(this Stream stream, ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteUInt64BE(this Stream stream, ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteInt8(this Stream stream, sbyte value)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = (byte)value;
            stream.Write(buffer);
        }

        public static void WriteInt16LE(this Stream stream, short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteInt16BE(this Stream stream, short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteInt16BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteInt32LE(this Stream stream, int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteInt32BE(this Stream stream, int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteInt64LE(this Stream stream, long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteInt64BE(this Stream stream, long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteFloat32LE(this Stream stream, float value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteFloat32BE(this Stream stream, float value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteFloat64LE(this Stream stream, double value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteFloat64BE(this Stream stream, double value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteBoolean(this Stream stream, bool value)
        {
            stream.WriteUInt8(value ? (byte)1 : (byte)0);
        }

        public static void WriteCharLE(this Stream stream, char value)
        {
            stream.WriteUInt16LE(value);
        }

        public static void WriteCharBE(this Stream stream, char value)
        {
            stream.WriteUInt16BE(value);
        }

        public static void Write7BitEncodedInt32(this Stream stream, int value)
        {
            uint uValue = (uint)value;

            // Write out an int 7 bits at a time. The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            //
            // Using the constants 0x7F and ~0x7F below offers smaller
            // codegen than using the constant 0x80.

            while (uValue > 0x7Fu)
            {
                stream.WriteUInt8((byte)(uValue | ~0x7Fu));
                uValue >>= 7;
            }

            stream.WriteUInt8((byte)uValue);
        }

        public static void Write7BitEncodedInt64(this Stream stream, long value)
        {
            ulong uValue = (ulong)value;

            // Write out an int 7 bits at a time. The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            //
            // Using the constants 0x7F and ~0x7F below offers smaller
            // codegen than using the constant 0x80.

            while (uValue > 0x7Fu)
            {
                stream.WriteUInt8((byte)((uint)uValue | ~0x7Fu));
                uValue >>= 7;
            }

            stream.WriteUInt8((byte)uValue);
        }

        public static int WriteString(this Stream stream, string value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            int size = encoding.GetMaxByteCount(value.Length);
            if (size <= 0x800)
            {
                Span<byte> buffer = stackalloc byte[size];
                size = encoding.GetBytes(value, buffer);
                stream.Write(buffer[..size]);
                return size;
            }
            using (NativeMemoryOwner owner = new NativeMemoryOwner((uint)size))
            {
                Span<byte> buffer = owner.AsSpan();
                size = encoding.GetBytes(value, buffer);
                stream.Write(buffer[..size]);
                return size;
            }
        }

        public static int WriteStringWithUInt8Head(this Stream stream, string? value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                stream.WriteUInt8(0x0);
                return 0;
            }
            int size = encoding.GetMaxByteCount(value.Length);
            if (size <= 0x800)
            {
                Span<byte> buffer = stackalloc byte[size];
                size = encoding.GetBytes(value, buffer);
                stream.WriteUInt8((byte)size);
                stream.Write(buffer[..size]);
                return size;
            }
            using (NativeMemoryOwner owner = new NativeMemoryOwner((uint)size))
            {
                Span<byte> buffer = owner.AsSpan();
                size = encoding.GetBytes(value, buffer);
                stream.WriteUInt8((byte)size);
                stream.Write(buffer[..size]);
                return size;
            }
        }

        public static int WriteStringWithInt32LEHead(this Stream stream, string? value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                stream.WriteInt32LE(0x0);
                return 0;
            }
            int size = encoding.GetMaxByteCount(value.Length);
            if (size <= 0x800)
            {
                Span<byte> buffer = stackalloc byte[size];
                size = encoding.GetBytes(value, buffer);
                stream.WriteInt32LE(size);
                stream.Write(buffer[..size]);
                return size;
            }
            using (NativeMemoryOwner owner = new NativeMemoryOwner((uint)size))
            {
                Span<byte> buffer = owner.AsSpan();
                size = encoding.GetBytes(value, buffer);
                stream.WriteInt32LE(size);
                stream.Write(buffer[..size]);
                return size;
            }
        }

        public static int WriteStringWith7BitEncodedInt32Head(this Stream stream, string? value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                stream.Write7BitEncodedInt32(0x0);
                return 0;
            }
            int size = encoding.GetMaxByteCount(value.Length);
            if (size <= 0x800)
            {
                Span<byte> buffer = stackalloc byte[size];
                size = encoding.GetBytes(value, buffer);
                stream.Write7BitEncodedInt32(size);
                stream.Write(buffer[..size]);
                return size;
            }
            using (NativeMemoryOwner owner = new NativeMemoryOwner((uint)size))
            {
                Span<byte> buffer = owner.AsSpan();
                size = encoding.GetBytes(value, buffer);
                stream.Write7BitEncodedInt32(size);
                stream.Write(buffer[..size]);
                return size;
            }
        }

        public static long CopyLengthTo(this Stream src, Stream destination, long len)
        {
            const long BUFFER_LEN = 81920;
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)BUFFER_LEN);
            try
            {
                long readLen = 0;
                long bytesRead;
                while (readLen < len)
                {
                    bytesRead = src.Read(buffer, 0, (int)Math.Min(BUFFER_LEN, len - readLen));
                    if (bytesRead > 0)
                    {
                        destination.Write(buffer, 0, (int)Math.Min(bytesRead, len - readLen));
                        readLen += bytesRead;
                    }
                    else
                    {
                        return readLen;
                    }
                }
                return readLen;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static long EncryptCopyLengthTo(this Stream src, Stream destination, long len, byte key)
        {
            const long BUFFER_LEN = 81920;
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)BUFFER_LEN);
            try
            {
                long readLen = 0;
                long bytesRead;
                while (readLen < len)
                {
                    bytesRead = src.Read(buffer, 0, (int)Math.Min(BUFFER_LEN, len - readLen));
                    if (bytesRead > 0)
                    {
                        for (int i = 0; i < bytesRead; i++)
                        {
                            buffer[i] ^= key;
                        }
                        destination.Write(buffer, 0, (int)Math.Min(bytesRead, len - readLen));
                        readLen += bytesRead;
                    }
                    else
                    {
                        return readLen;
                    }
                }
                return readLen;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static nuint Read(this Stream stream, void* Pointer, nuint Length)
        {
            const nuint BUFFERSIZE = 0x40000000;
            nuint readLength = 0x0;
            int currentReadLength;
            while ((Length - readLength) >= BUFFERSIZE)
            {
                currentReadLength = stream.Read(new Span<byte>((void*)((nuint)Pointer + readLength), (int)BUFFERSIZE));
                readLength += (nuint)currentReadLength;
                if ((nuint)currentReadLength != BUFFERSIZE)
                {
                    return readLength;
                }
            }
            if (readLength != Length)
            {
                readLength += (nuint)stream.Read(new Span<byte>((void*)((nuint)Pointer + readLength), (int)(Length - readLength)));
            }
            return readLength;
        }

        public static nuint Read(this Stream stream, nuint Pointer, nuint Length)
        {
            const nuint BUFFERSIZE = 0x40000000;
            nuint readLength = 0x0;
            int currentReadLength;
            while ((Length - readLength) >= BUFFERSIZE)
            {
                currentReadLength = stream.Read(new Span<byte>((void*)(Pointer + readLength), (int)BUFFERSIZE));
                readLength += (nuint)currentReadLength;
                if ((nuint)currentReadLength != BUFFERSIZE)
                {
                    return readLength;
                }
            }
            if (readLength != Length)
            {
                readLength += (nuint)stream.Read(new Span<byte>((void*)(Pointer + readLength), (int)(Length - readLength)));
            }
            return readLength;
        }

        public static void Write(this Stream stream, void* Pointer, nuint Length)
        {
            const nuint BUFFERSIZE = 0x40000000;
            nuint writeLength = 0x0;
            while ((Length - writeLength) >= BUFFERSIZE)
            {
                stream.Write(new ReadOnlySpan<byte>((void*)((nuint)Pointer + writeLength), (int)BUFFERSIZE));
                writeLength += BUFFERSIZE;
            }
            if (writeLength != Length)
            {
                stream.Write(new ReadOnlySpan<byte>((void*)((nuint)Pointer + writeLength), (int)(Length - writeLength)));
            }
        }

        public static void Write(this Stream stream, nuint Pointer, nuint Length)
        {
            const nuint BUFFERSIZE = 0x40000000;
            nuint writeLength = 0x0;
            while ((Length - writeLength) >= BUFFERSIZE)
            {
                stream.Write(new ReadOnlySpan<byte>((void*)(Pointer + writeLength), (int)BUFFERSIZE));
                writeLength += BUFFERSIZE;
            }
            if (writeLength != Length)
            {
                stream.Write(new ReadOnlySpan<byte>((void*)(Pointer + writeLength), (int)(Length - writeLength)));
            }
        }
    }
}
