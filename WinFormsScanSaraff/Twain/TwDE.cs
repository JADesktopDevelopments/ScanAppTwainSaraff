using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    public enum TwDE : ushort
    {
        CheckAutomaticCapture = 0,
        CheckBattery = 1,
        CheckDeviceOnline = 2,
        CheckFlash = 3,
        CheckPowerSupply = 4,
        CheckResolution = 5,
        DeviceAdded = 6,
        DeviceOffline = 7,
        DeviceReady = 8,
        DeviceRemoved = 9,
        ImageCaptured = 10, // 0x000A
        ImageDeleted = 11, // 0x000B
        PaperDoubleFeed = 12, // 0x000C
        PaperJam = 13, // 0x000D
        LampFailure = 14, // 0x000E
        PowerSave = 15, // 0x000F
        PowerSaveNotify = 16, // 0x0010
        CustomEvents = 32768, // 0x8000
    }
}
