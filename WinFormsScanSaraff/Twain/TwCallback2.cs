using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwCallback2
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public CallBackProc CallBackProc;
        public UIntPtr RefCon;
        public short Message;
    }
}
