using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwStatus
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwCC ConditionCode;
        public ushort Reserved;
    }
}
