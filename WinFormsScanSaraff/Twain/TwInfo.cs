using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("InfoId = {InfoId}, ItemType = {ItemType}, ReturnCode = {ReturnCode}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwInfo : IDisposable
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwEI InfoId;
        [MarshalAs(UnmanagedType.U2)]
        public TwType ItemType;
        public ushort NumItems;
        [MarshalAs(UnmanagedType.U2)]
        public TwRC ReturnCode;
        public IntPtr Item;

        private bool _IsValue
        {
            get
            {
                return this.ItemType != TwType.Handle && TwTypeHelper.SizeOf(this.ItemType) * (int)this.NumItems <= TwTypeHelper.SizeOf(TwType.Handle);
            }
        }

        public object GetValue()
        {
            object[] objArray = new object[(int)this.NumItems];
            if (this._IsValue)
            {
                long index = 0;
                long int64 = this.Item.ToInt64();
                long num = (1L << TwTypeHelper.SizeOf(this.ItemType) * 7 << TwTypeHelper.SizeOf(this.ItemType)) - 1L;
                while (index < (long)this.NumItems)
                {
                    objArray[index] = TwTypeHelper.CastToCommon(this.ItemType, TwTypeHelper.ValueToTw<long>(this.ItemType, int64 & num));
                    ++index;
                    int64 >>= TwTypeHelper.SizeOf(this.ItemType) * 8;
                }
            }
            else
            {
                IntPtr ptr = Twain32._Memory.Lock(this.Item);
                try
                {
                    for (int index = 0; index < (int)this.NumItems; ++index)
                    {
                        if (this.ItemType != TwType.Handle)
                        {
                            objArray[index] = TwTypeHelper.CastToCommon(this.ItemType, Marshal.PtrToStructure((IntPtr)((long)ptr + (long)(TwTypeHelper.SizeOf(this.ItemType) * index)), TwTypeHelper.TypeOf(this.ItemType)));
                        }
                        else
                        {
                            objArray[index] = (object)Marshal.PtrToStringAnsi(ptr);
                            ptr = (IntPtr)((long)ptr + (long)objArray[index].ToString().Length + 1L);
                        }
                    }
                }
                finally
                {
                    Twain32._Memory.Unlock(this.Item);
                }
            }
            return objArray.Length != 1 ? (object)objArray : objArray[0];
        }

        public void Dispose()
        {
            if (!(this.Item != IntPtr.Zero) || this._IsValue)
                return;
            Twain32._Memory.Free(this.Item);
            this.Item = IntPtr.Zero;
        }
    }
}
