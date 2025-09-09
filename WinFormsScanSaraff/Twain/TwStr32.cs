using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Value}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal sealed class TwStr32
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string Value;

        public override string ToString() => this.Value;

        public static implicit operator string(TwStr32 value) => value?.Value;

        public static implicit operator TwStr32(string value)
        {
            return new TwStr32() { Value = value };
        }
    }
}
