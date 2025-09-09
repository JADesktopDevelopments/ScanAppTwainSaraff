using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Info}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct TwVersion
    {
        public ushort MajorNum;
        public ushort MinorNum;
        [MarshalAs(UnmanagedType.U2)]
        public TwLanguage Language;
        [MarshalAs(UnmanagedType.U2)]
        public TwCountry Country;
        public TwStr32 Info;
    }
}
