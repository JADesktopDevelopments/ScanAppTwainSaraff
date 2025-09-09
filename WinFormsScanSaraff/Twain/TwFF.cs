using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    public enum TwFF : ushort
    {
        Tiff = 0,
        Pict = 1,
        Bmp = 2,
        Xbm = 3,
        Jfif = 4,
        Fpx = 5,
        TiffMulti = 6,
        Png = 7,
        Spiff = 8,
        Exif = 9,
        Pdf = 10, // 0x000A
        Jp2 = 11, // 0x000B
        Jpx = 13, // 0x000D
        Dejavu = 14, // 0x000E
        PdfA = 15, // 0x000F
        PdfA2 = 16, // 0x0010
        PdfRaster = 17, // 0x0011
    }
}
