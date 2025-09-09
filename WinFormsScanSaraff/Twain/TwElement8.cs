using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("RGB=({Channel1},{Channel2},{Channel3}), Index={Index}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct TwElement8
    {
        public byte Index;
        public byte Channel1;
        public byte Channel2;
        public byte Channel3;

        public static implicit operator Color(TwElement8 element)
        {
            return Color.FromArgb((int)element.Channel1, (int)element.Channel2, (int)element.Channel3);
        }

        public static implicit operator TwElement8(Color color)
        {
            return new TwElement8()
            {
                Channel1 = color.R,
                Channel2 = color.G,
                Channel3 = color.B
            };
        }
    }
}
