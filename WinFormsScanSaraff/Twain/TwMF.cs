
namespace WinFormsScanSaraff.Twain
{
    [Flags]
    internal enum TwMF : uint
    {
        AppOwns = 1,
        DsmOwns = 2,
        DSOwns = 4,
        Pointer = 8,
        Handle = 16, // 0x00000010
    }
}
