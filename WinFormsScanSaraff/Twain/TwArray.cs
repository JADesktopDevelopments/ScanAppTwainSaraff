using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwArray : ITwArray
    {
        [MarshalAs(UnmanagedType.U2)]
        private TwType _itemType;
        private uint _numItems;

        public TwType ItemType
        {
            get => this._itemType;
            set => this._itemType = value;
        }

        public uint NumItems
        {
            get => this._numItems;
            set => this._numItems = value;
        }
    }
}
