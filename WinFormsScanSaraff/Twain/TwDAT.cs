using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    internal enum TwDAT : ushort
    {
        Null = 0,
        Capability = 1,
        Event = 2,
        Identity = 3,
        Parent = 4,
        PendingXfers = 5,
        SetupMemXfer = 6,
        SetupFileXfer = 7,
        Status = 8,
        UserInterface = 9,
        XferGroup = 10, // 0x000A
        TwunkIdentity = 11, // 0x000B
        CustomDSData = 12, // 0x000C
        DeviceEvent = 13, // 0x000D
        FileSystem = 14, // 0x000E
        PassThru = 15, // 0x000F
        Callback = 16, // 0x0010
        StatusUtf8 = 17, // 0x0011
        Callback2 = 18, // 0x0012
        ImageInfo = 257, // 0x0101
        ImageLayout = 258, // 0x0102
        ImageMemXfer = 259, // 0x0103
        ImageNativeXfer = 260, // 0x0104
        ImageFileXfer = 261, // 0x0105
        CieColor = 262, // 0x0106
        GrayResponse = 263, // 0x0107
        RGBResponse = 264, // 0x0108
        JpegCompression = 265, // 0x0109
        Palette8 = 266, // 0x010A
        ExtImageInfo = 267, // 0x010B
        IccProfile = 1025, // 0x0401
        ImageMemFileXfer = 1026, // 0x0402
        EntryPoint = 1027, // 0x0403
    }
}
