using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwEntryPoint
    {
        public int Size;
        public IntPtr DSM_Entry;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public DSM_MemoryAllocate MemoryAllocate;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public DSM_MemoryFree MemoryFree;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public DSM_MemoryLock MemoryLock;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public DSM_MemoryUnlock MemoryUnlock;

        public TwEntryPoint() => this.Size = Marshal.SizeOf((object)this);
    }
}
