using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Value}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal sealed class TwStr255
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Value;

        public override string ToString() => this.Value;

        public static implicit operator string(TwStr255 value) => value?.Value;

        public static implicit operator TwStr255(string value)
        {
            return new TwStr255() { Value = value };
        }
    }
}
