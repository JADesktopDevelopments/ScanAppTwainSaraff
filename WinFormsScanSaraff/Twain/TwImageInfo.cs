
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwImageInfo
    {
        public TwFix32 XResolution;
        public TwFix32 YResolution;
        public int ImageWidth;
        public int ImageLength;
        public short SamplesPerPixel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] BitsPerSample;
        public short BitsPerPixel;
        public TwBool Planar;
        [MarshalAs(UnmanagedType.U2)]
        public TwPixelType PixelType;
        [MarshalAs(UnmanagedType.U2)]
        public TwCompression Compression;
    }
}
