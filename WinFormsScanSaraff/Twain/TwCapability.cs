using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static WinFormsScanSaraff.Twain.Twain32;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwCapability : IDisposable
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwCap Cap;
        [MarshalAs(UnmanagedType.U2)]
        public TwOn ConType;
        public IntPtr Handle;

        private TwCapability()
        {
        }

        public TwCapability(TwCap cap)
        {
            this.Cap = cap;
            this.ConType = TwOn.DontCare;
        }

        public TwCapability(TwCap cap, uint value, TwType type)
        {
            this.Cap = cap;
            this.ConType = TwOn.One;
            this._SetValue<TwOneValue>(new TwOneValue()
            {
                ItemType = type,
                Item = value
            });
        }

        public TwCapability(TwCap cap, string value, TwType type)
        {
            this.Cap = cap;
            this.ConType = TwOn.One;
            int num = Marshal.SizeOf(typeof(TwOneCustumValue));
            this.Handle = Twain32._Memory.Alloc(num + Marshal.SizeOf(TwTypeHelper.TypeOf(type)));
            IntPtr ptr = Twain32._Memory.Lock(this.Handle);
            try
            {
                Marshal.StructureToPtr((object)new TwOneCustumValue()
                {
                    ItemType = type
                }, ptr, true);
                Marshal.StructureToPtr(TwTypeHelper.CastToTw(type, (object)value), (IntPtr)(ptr.ToInt64() + (long)num), true);
            }
            finally
            {
                Twain32._Memory.Unlock(this.Handle);
            }
        }

        public TwCapability(TwCap cap, TwRange range)
        {
            this.Cap = cap;
            this.ConType = TwOn.Range;
            this._SetValue<TwRange>(range);
        }

        public TwCapability(TwCap cap, TwArray array, object[] arrayValue)
        {
            this.Cap = cap;
            this.ConType = TwOn.Array;
            int num1 = Marshal.SizeOf(typeof(TwArray));
            int num2 = Marshal.SizeOf(TwTypeHelper.TypeOf(array.ItemType));
            this.Handle = Twain32._Memory.Alloc(num1 + num2 * arrayValue.Length);
            IntPtr ptr1 = Twain32._Memory.Lock(this.Handle);
            try
            {
                Marshal.StructureToPtr((object)array, ptr1, true);
                long index = 0;
                long ptr2 = ptr1.ToInt64() + (long)num1;
                while (index < (long)arrayValue.Length)
                {
                    Marshal.StructureToPtr(TwTypeHelper.CastToTw(array.ItemType, arrayValue[index]), (IntPtr)ptr2, true);
                    ++index;
                    ptr2 += (long)num2;
                }
            }
            finally
            {
                Twain32._Memory.Unlock(this.Handle);
            }
        }

        public TwCapability(TwCap cap, TwEnumeration enumeration, object[] enumerationValue)
        {
            this.Cap = cap;
            this.ConType = TwOn.Enum;
            int num1 = Marshal.SizeOf(typeof(TwEnumeration));
            int num2 = Marshal.SizeOf(TwTypeHelper.TypeOf(enumeration.ItemType));
            this.Handle = Twain32._Memory.Alloc(num1 + num2 * enumerationValue.Length);
            IntPtr ptr1 = Twain32._Memory.Lock(this.Handle);
            try
            {
                Marshal.StructureToPtr((object)enumeration, ptr1, true);
                long index = 0;
                long ptr2 = ptr1.ToInt64() + (long)num1;
                while (index < (long)enumerationValue.Length)
                {
                    Marshal.StructureToPtr(TwTypeHelper.CastToTw(enumeration.ItemType, enumerationValue[index]), (IntPtr)ptr2, true);
                    ++index;
                    ptr2 += (long)num2;
                }
            }
            finally
            {
                Twain32._Memory.Unlock(this.Handle);
            }
        }

        public object GetValue()
        {
            IntPtr ptr = Twain32._Memory.Lock(this.Handle);
            try
            {
                switch (this.ConType)
                {
                    case TwOn.Array:
                        return (object)new __TwArray((ITwArray)Marshal.PtrToStructure(ptr, typeof(TwArray)), (IntPtr)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(TwArray))));
                    case TwOn.Enum:
                        return (object)new __TwEnumeration((TwEnumeration)Marshal.PtrToStructure(ptr, typeof(TwEnumeration)), (IntPtr)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(TwEnumeration))));
                    case TwOn.One:
                        TwOneCustumValue structure = Marshal.PtrToStructure(ptr, typeof(TwOneCustumValue)) as TwOneCustumValue;
                        switch (structure.ItemType)
                        {
                            case TwType.Str32:
                            case TwType.Str64:
                            case TwType.Str128:
                            case TwType.Str255:
                            case TwType.Str1024:
                            case TwType.Uni512:
                                return (object)Marshal.PtrToStructure((IntPtr)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(TwOneCustumValue))), TwTypeHelper.TypeOf(structure.ItemType)).ToString();
                            default:
                                return Marshal.PtrToStructure(ptr, typeof(TwOneValue));
                        }
                    case TwOn.Range:
                        return Marshal.PtrToStructure(ptr, typeof(TwRange));
                    default:
                        return (object)null;
                }
            }
            finally
            {
                Twain32._Memory.Unlock(this.Handle);
            }
        }

        public void Dispose()
        {
            if (!(this.Handle != IntPtr.Zero))
                return;
            Twain32._Memory.Free(this.Handle);
            this.Handle = IntPtr.Zero;
        }

        private void _SetValue<T>(T value)
        {
            this.Handle = Twain32._Memory.Alloc(Marshal.SizeOf(typeof(T)));
            IntPtr ptr = Twain32._Memory.Lock(this.Handle);
            try
            {
                Marshal.StructureToPtr((object)value, ptr, true);
            }
            finally
            {
                Twain32._Memory.Unlock(this.Handle);
            }
        }
    }
}
