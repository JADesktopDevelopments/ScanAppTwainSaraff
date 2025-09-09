using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    public enum TwAL : ushort
    {
        Alarm,
        FeederError,
        FeederWarning,
        BarCode,
        DoubleFeed,
        Jam,
        PatchCode,
        Power,
        Skew,
    }
}
