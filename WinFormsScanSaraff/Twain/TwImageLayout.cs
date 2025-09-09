using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwImageLayout
    {
        public TwFrame Frame;
        public uint DocumentNumber;
        public uint PageNumber;
        public uint FrameNumber;
    }
}
