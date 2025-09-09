using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwSetupFileXfer
    {
        public TwStr255 FileName;
        [MarshalAs(UnmanagedType.U2)]
        public TwFF Format;
        public IntPtr VrefNum;
    }
}
