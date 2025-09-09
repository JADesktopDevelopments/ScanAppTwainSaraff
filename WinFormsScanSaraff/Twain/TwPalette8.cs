using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwPalette8
    {
        public ushort NumColors;
        [MarshalAs(UnmanagedType.U2)]
        public TwPA PaletteType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public TwElement8[] Colors;

        public static implicit operator Twain32.ColorPalette(TwPalette8 palette)
        {
            return Twain32.ColorPalette.Create(palette);
        }

        public static implicit operator TwPalette8(Twain32.ColorPalette palette)
        {
            TwPalette8 twPalette8 = new TwPalette8()
            {
                PaletteType = palette.PaletteType,
                NumColors = (ushort)palette.Colors.Length,
                Colors = new TwElement8[256]
            };
            for (int index = 0; index < palette.Colors.Length; ++index)
                twPalette8.Colors[index] = (TwElement8)palette.Colors[index];
            return twPalette8;
        }
    }
}
