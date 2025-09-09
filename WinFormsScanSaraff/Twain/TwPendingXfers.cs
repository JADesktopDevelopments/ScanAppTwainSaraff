using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwPendingXfers
    {
        public ushort Count;
        public uint EOJ;
    }
}
