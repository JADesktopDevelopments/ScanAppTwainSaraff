using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Value}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal sealed class TwStr128
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 130)]
        public string Value;

        public override string ToString() => this.Value;

        public static implicit operator string(TwStr128 value) => value?.Value;

        public static implicit operator TwStr128(string value)
        {
            return new TwStr128() { Value = value };
        }
    }
}
