using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwMemory
    {
        [MarshalAs(UnmanagedType.U4)]
        public TwMF Flags;
        public uint Length;
        public IntPtr TheMem;
    }
}
