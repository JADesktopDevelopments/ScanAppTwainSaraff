
namespace WinFormsScanSaraff.Twain
{
    [Flags]
    internal enum TwDG : uint
    {
        Control = 1,
        Image = 2,
        Audio = 4,
        DSM2 = 268435456, // 0x10000000
        APP2 = 536870912, // 0x20000000
        DS2 = 1073741824, // 0x40000000
    }
}
