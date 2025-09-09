using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Value}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal sealed class TwStr1024
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1026)]
        public string Value;

        public override string ToString() => this.Value;

        public static implicit operator string(TwStr1024 value) => value?.Value;

        public static implicit operator TwStr1024(string value)
        {
            return new TwStr1024() { Value = value };
        }
    }
}
