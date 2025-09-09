using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{ToRectangle()}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct TwFrame
    {
        public TwFix32 Left;
        public TwFix32 Top;
        public TwFix32 Right;
        public TwFix32 Bottom;

        private RectangleF ToRectangle()
        {
            return new RectangleF((float)this.Left, (float)this.Top, (float)this.Right - (float)this.Left, (float)this.Bottom - (float)this.Top);
        }

        public static implicit operator RectangleF(TwFrame value) => value.ToRectangle();

        public static implicit operator TwFrame(RectangleF value)
        {
            return new TwFrame()
            {
                Left = (TwFix32)value.Left,
                Top = (TwFix32)value.Top,
                Right = (TwFix32)value.Right,
                Bottom = (TwFix32)value.Bottom
            };
        }
    }
}
