﻿using System;
using System.Runtime.InteropServices;

namespace PVZDotNetResGen.Utils.MemoryHelper
{
    internal struct NativeMemoryOwner : IDisposable
    {
        public unsafe void* Pointer;
        public uint Size;
        private bool _disposed;

        public readonly unsafe Span<byte> AsSpan()
        {
            return new Span<byte>(Pointer, (int)Math.Min(Size, int.MaxValue - 1));
        }

        public unsafe NativeMemoryOwner(uint memSize)
        {
            Pointer = NativeMemory.Alloc(memSize);
            Size = memSize;
            _disposed = false;
        }

        public readonly unsafe void Fill(byte value)
        {
            NativeMemory.Fill(Pointer, Size, value);
        }

        public readonly unsafe void Clear()
        {
            NativeMemory.Clear(Pointer, Size);
        }

        public unsafe void Realloc(uint memSize)
        {
            Pointer = NativeMemory.Realloc(Pointer, memSize);
            Size = memSize;
        }

        public unsafe void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                NativeMemory.Free(Pointer);
            }
        }
    }
}
