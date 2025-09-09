using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    internal interface __ITwArray
    {
        TwType ItemType { get; }

        uint NumItems { get; }

        object[] Items { get; }
    }
}
