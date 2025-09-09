using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{ToFloat()}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct TwFix32
    {
        public short Whole;
        public ushort Frac;

        private float ToFloat() => (float)this.Whole + (float)this.Frac / 65536f;

        public static implicit operator TwFix32(float f)
        {
            int num = (int)((double)f * 65536.0 + 0.5);
            return new TwFix32()
            {
                Whole = (short)(num >> 16),
                Frac = (ushort)(num & (int)ushort.MaxValue)
            };
        }

        public static explicit operator TwFix32(uint value)
        {
            return new TwFix32()
            {
                Whole = (short)((int)value & (int)ushort.MaxValue),
                Frac = (ushort)(value >> 16)
            };
        }

        public static implicit operator float(TwFix32 value) => value.ToFloat();

        public static explicit operator uint(TwFix32 value)
        {
            return (uint)(ushort)value.Whole + ((uint)value.Frac << 16);
        }
    }
}
