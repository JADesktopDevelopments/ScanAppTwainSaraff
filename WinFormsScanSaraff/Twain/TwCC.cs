
namespace WinFormsScanSaraff.Twain
{
    public enum TwCC : ushort
    {
        Success = 0,
        Bummer = 1,
        LowMemory = 2,
        NoDS = 3,
        MaxConnections = 4,
        OperationError = 5,
        BadCap = 6,
        BadProtocol = 9,
        BadValue = 10, // 0x000A
        SeqError = 11, // 0x000B
        BadDest = 12, // 0x000C
        CapUnsupported = 13, // 0x000D
        CapBadOperation = 14, // 0x000E
        CapSeqError = 15, // 0x000F
        Denied = 16, // 0x0010
        FileExists = 17, // 0x0011
        FileNotFound = 18, // 0x0012
        NotEmpty = 19, // 0x0013
        PaperJam = 20, // 0x0014
        PaperDoubleFeed = 21, // 0x0015
        FileWriteError = 22, // 0x0016
        CheckDeviceOnline = 23, // 0x0017
        InterLock = 24, // 0x0018
        DamagedCorner = 25, // 0x0019
        FocusError = 26, // 0x001A
        DocTooLight = 27, // 0x001B
        DocTooDark = 28, // 0x001C
        NoMedia = 29, // 0x001D
    }
}
