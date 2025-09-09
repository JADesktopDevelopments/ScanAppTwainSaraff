
namespace WinFormsScanSaraff.Twain
{
    internal class __TwEnumeration : __TwArray, __ITwEnumeration, __ITwArray
    {
        private TwEnumeration _data;

        internal __TwEnumeration(TwEnumeration data, IntPtr items)
          : base((ITwArray)data, items)
        {
            this._data = data;
        }

        public int CurrentIndex => (int)this._data.CurrentIndex;

        public int DefaultIndex => (int)this._data.DefaultIndex;
    }
}
