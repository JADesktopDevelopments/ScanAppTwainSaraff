using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwExtImageInfo
    {
        public uint NumInfos;

        public static IntPtr ToPtr(TwInfo[] info)
        {
            int num1 = Marshal.SizeOf(typeof(TwExtImageInfo));
            int num2 = Marshal.SizeOf(typeof(TwInfo));
            IntPtr ptr = Marshal.AllocHGlobal(num1 + num2 * info.Length);
            Marshal.StructureToPtr((object)new TwExtImageInfo()
            {
                NumInfos = (uint)info.Length
            }, ptr, true);
            for (int index = 0; index < info.Length; ++index)
                Marshal.StructureToPtr((object)info[index], (IntPtr)(ptr.ToInt64() + (long)num1 + (long)(num2 * index)), true);
            return ptr;
        }
    }
}
