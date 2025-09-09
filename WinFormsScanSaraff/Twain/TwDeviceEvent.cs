using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwDeviceEvent
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwDE Event;
        private ushort reserved;
        public TwStr255 DeviceName;
        public uint BatteryMinutes;
        public short BatteryPercentAge;
        public int PowerSupply;
        public TwFix32 XResolution;
        public TwFix32 YResolution;
        public uint FlashUsed2;
        public uint AutomaticCapture;
        public uint TimeBeforeFirstCapture;
        public uint TimeBetweenCaptures;
    }
}
