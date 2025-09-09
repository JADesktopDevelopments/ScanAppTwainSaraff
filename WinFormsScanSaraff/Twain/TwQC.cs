
namespace WinFormsScanSaraff.Twain
{
    [Flags]
    public enum TwQC : ushort
    {
        Get = 1,
        Set = 2,
        GetDefault = 4,
        GetCurrent = 8,
        Reset = 16, // 0x0010
        SetConstraint = 32, // 0x0020
        ConstrainAble = 64, // 0x0040
        GetHelp = 256, // 0x0100
        GetLabel = 512, // 0x0200
        GetLabelEnum = 1024, // 0x0400
    }
}
