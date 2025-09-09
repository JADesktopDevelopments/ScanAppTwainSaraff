using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Value}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal sealed class TwStr64
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 66)]
        public string Value;

        public override string ToString() => this.Value;

        public static implicit operator string(TwStr64 value) => value?.Value;

        public static implicit operator TwStr64(string value)
        {
            return new TwStr64() { Value = value };
        }
    }
}
