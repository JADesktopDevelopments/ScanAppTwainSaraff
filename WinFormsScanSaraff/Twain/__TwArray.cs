using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    internal class __TwArray : __ITwArray
    {
        private ITwArray _data;
        private object[] _items;

        internal __TwArray(ITwArray data, IntPtr items)
        {
            this._data = data;
            this._items = new object[(int)this._data.NumItems];
            long index = 0;
            long num1 = 0;
            long num2 = (long)TwTypeHelper.SizeOf(this._data.ItemType);
            while (index < (long)this._data.NumItems)
            {
                this._items[index] = TwTypeHelper.CastToCommon(this._data.ItemType, Marshal.PtrToStructure((IntPtr)(items.ToInt64() + num1), TwTypeHelper.TypeOf(this._data.ItemType)));
                ++index;
                num1 += num2;
            }
        }

        public TwType ItemType => this._data.ItemType;

        public uint NumItems => this._data.NumItems;

        public object[] Items => this._items;
    }
}
