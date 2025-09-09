
namespace WinFormsScanSaraff.Twain
{
    public enum TwPixelType : ushort
    {
        BW = 0,
        Gray = 1,
        RGB = 2,
        Palette = 3,
        CMY = 4,
        CMYK = 5,
        YUV = 6,
        YUVK = 7,
        CIEXYZ = 8,
        LAB = 9,
        SRGB = 10, // 0x000A
        SCRGB = 11, // 0x000B
        INFRARED = 16, // 0x0010
    }
}
