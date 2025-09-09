using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Value}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
    internal sealed class TwUni512
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string Value;

        public override string ToString() => this.Value;

        public static implicit operator string(TwUni512 value) => value?.Value;

        public static implicit operator TwUni512(string value)
        {
            return new TwUni512() { Value = value };
        }
    }
}
