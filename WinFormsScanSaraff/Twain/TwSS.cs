using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    public enum TwSS : ushort
    {
        None = 0,
        A4 = 1,
        A4Letter = 1,
        B5Letter = 2,
        JISB5 = 2,
        USLetter = 3,
        USLegal = 4,
        A5 = 5,
        B4 = 6,
        ISOB4 = 6,
        B6 = 7,
        ISOB6 = 7,
        USLedger = 9,
        USExecutive = 10, // 0x000A
        A3 = 11, // 0x000B
        B3 = 12, // 0x000C
        ISOB3 = 12, // 0x000C
        A6 = 13, // 0x000D
        C4 = 14, // 0x000E
        C5 = 15, // 0x000F
        C6 = 16, // 0x0010
        _4A0 = 17, // 0x0011
        _2A0 = 18, // 0x0012
        A0 = 19, // 0x0013
        A1 = 20, // 0x0014
        A2 = 21, // 0x0015
        A7 = 22, // 0x0016
        A8 = 23, // 0x0017
        A9 = 24, // 0x0018
        A10 = 25, // 0x0019
        ISOB0 = 26, // 0x001A
        ISOB1 = 27, // 0x001B
        ISOB2 = 28, // 0x001C
        ISOB5 = 29, // 0x001D
        ISOB7 = 30, // 0x001E
        ISOB8 = 31, // 0x001F
        ISOB9 = 32, // 0x0020
        ISOB10 = 33, // 0x0021
        JISB0 = 34, // 0x0022
        JISB1 = 35, // 0x0023
        JISB2 = 36, // 0x0024
        JISB3 = 37, // 0x0025
        JISB4 = 38, // 0x0026
        JISB6 = 39, // 0x0027
        JISB7 = 40, // 0x0028
        JISB8 = 41, // 0x0029
        JISB9 = 42, // 0x002A
        JISB10 = 43, // 0x002B
        C0 = 44, // 0x002C
        C1 = 45, // 0x002D
        C2 = 46, // 0x002E
        C3 = 47, // 0x002F
        C7 = 48, // 0x0030
        C8 = 49, // 0x0031
        C9 = 50, // 0x0032
        C10 = 51, // 0x0033
        USStatement = 52, // 0x0034
        BusinessCard = 53, // 0x0035
    }
}
