using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwOneCustumValue
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwType ItemType;
    }
}
