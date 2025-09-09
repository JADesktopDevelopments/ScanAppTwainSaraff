using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwEnumeration : ITwArray
    {
        [MarshalAs(UnmanagedType.U2)]
        private TwType _ItemType;
        private uint _numItems;
        private uint _currentIndex;
        private uint _defaultIndex;

        public TwType ItemType
        {
            get => this._ItemType;
            set => this._ItemType = value;
        }

        public uint NumItems
        {
            get => this._numItems;
            set => this._numItems = value;
        }

        public uint CurrentIndex
        {
            get => this._currentIndex;
            set => this._currentIndex = value;
        }

        public uint DefaultIndex
        {
            get => this._defaultIndex;
            set => this._defaultIndex = value;
        }
    }
}
