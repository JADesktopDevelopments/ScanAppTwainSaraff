using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwSetupMemXfer
    {
        public uint MinBufSize;
        public uint MaxBufSize;
        public uint Preferred;
    }
}
