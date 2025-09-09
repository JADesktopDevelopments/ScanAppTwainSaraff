using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{ToBool()}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct TwBool
    {
        public ushort Value;

        private bool ToBool() => this.Value > (ushort)0;

        public static implicit operator bool(TwBool value) => value.ToBool();

        public static implicit operator TwBool(bool value)
        {
            return new TwBool()
            {
                Value = value ? (ushort)1 : (ushort)0
            };
        }
    }
}
