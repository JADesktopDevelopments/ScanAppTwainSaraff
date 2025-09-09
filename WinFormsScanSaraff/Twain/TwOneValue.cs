using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwOneValue
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwType ItemType;
        public uint Item;
    }
}
