using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwRange
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwType ItemType;
        public uint MinValue;
        public uint MaxValue;
        public uint StepSize;
        public uint DefaultValue;
        public uint CurrentValue;
    }
}
