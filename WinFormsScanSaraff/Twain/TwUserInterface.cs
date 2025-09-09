using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwUserInterface
    {
        public TwBool ShowUI;
        public TwBool ModalUI;
        public IntPtr ParentHand;
    }
}
