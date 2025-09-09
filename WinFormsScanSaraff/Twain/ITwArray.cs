using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    internal interface ITwArray
    {
        TwType ItemType { get; set; }

        uint NumItems { get; set; }
    }
}
