
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwEvent
    {
        public IntPtr EventPtr;
        [MarshalAs(UnmanagedType.U2)]
        public TwMSG Message;
    }
}
