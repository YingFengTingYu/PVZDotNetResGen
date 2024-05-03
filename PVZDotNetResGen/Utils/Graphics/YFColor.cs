using System.Runtime.InteropServices;

namespace PVZDotNetResGen.Utils.Graphics
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x4)]
    public unsafe struct YFColor
    {
        public static YFColor Transparent => new YFColor(0x0, 0x0, 0x0, 0x0);

        public static YFColor Black => new YFColor(0x0, 0x0, 0x0, 0xFF);

        [FieldOffset(0x0)]
        public byte mRed;

        [FieldOffset(0x1)]
        public byte mGreen;

        [FieldOffset(0x2)]
        public byte mBlue;

        [FieldOffset(0x3)]
        public byte mAlpha;

        public YFColor(byte red, byte green, byte blue, byte alpha)
        {
            mRed = red;
            mGreen = green;
            mBlue = blue;
            mAlpha = alpha;
        }

        public YFColor(byte red, byte green, byte blue)
        {
            mRed = red;
            mGreen = green;
            mBlue = blue;
            mAlpha = 0xFF;
        }

        public override readonly string ToString()
        {
            return $"#{mAlpha:x2}{mRed:x2}{mGreen:x2}{mBlue:x2}";
        }

        public static explicit operator YFColor(uint color)
        {
            return *(YFColor*)&color;
        }

        public static explicit operator uint(YFColor color)
        {
            return *(uint*)&color;
        }
    }
}
