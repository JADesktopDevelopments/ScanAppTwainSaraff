using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    internal sealed class TwTypeHelper
    {
        private static Dictionary<TwType, Type> _typeof = new Dictionary<TwType, Type>()
    {
      {
        TwType.Int8,
        typeof (sbyte)
      },
      {
        TwType.Int16,
        typeof (short)
      },
      {
        TwType.Int32,
        typeof (int)
      },
      {
        TwType.UInt8,
        typeof (byte)
      },
      {
        TwType.UInt16,
        typeof (ushort)
      },
      {
        TwType.UInt32,
        typeof (uint)
      },
      {
        TwType.Bool,
        typeof (TwBool)
      },
      {
        TwType.Fix32,
        typeof (TwFix32)
      },
      {
        TwType.Frame,
        typeof (TwFrame)
      },
      {
        TwType.Str32,
        typeof (TwStr32)
      },
      {
        TwType.Str64,
        typeof (TwStr64)
      },
      {
        TwType.Str128,
        typeof (TwStr128)
      },
      {
        TwType.Str255,
        typeof (TwStr255)
      },
      {
        TwType.Str1024,
        typeof (TwStr1024)
      },
      {
        TwType.Uni512,
        typeof (TwUni512)
      },
      {
        TwType.Handle,
        typeof (IntPtr)
      }
    };
        private static Dictionary<int, TwType> _typeofAux = new Dictionary<int, TwType>()
    {
      {
        32,
        TwType.Str32
      },
      {
        64,
        TwType.Str64
      },
      {
        128,
        TwType.Str128
      },
      {
        (int) byte.MaxValue,
        TwType.Str255
      },
      {
        1024,
        TwType.Str1024
      },
      {
        512,
        TwType.Uni512
      }
    };

        internal static Type TypeOf(TwType type) => TwTypeHelper._typeof[type];

        internal static TwType TypeOf(Type type)
        {
            Type type1 = type.IsEnum ? Enum.GetUnderlyingType(type) : type;
            foreach (KeyValuePair<TwType, Type> keyValuePair in TwTypeHelper._typeof)
            {
                if (keyValuePair.Value == type1)
                    return keyValuePair.Key;
            }
            if (type == typeof(bool))
                return TwType.Bool;
            if (type == typeof(float))
                return TwType.Fix32;
            if (type == typeof(RectangleF))
                return TwType.Frame;
            throw new KeyNotFoundException();
        }

        internal static TwType TypeOf(object obj)
        {
            return obj is string ? TwTypeHelper._typeofAux[((string)obj).Length] : TwTypeHelper.TypeOf(obj.GetType());
        }

        internal static int SizeOf(TwType type) => Marshal.SizeOf(TwTypeHelper._typeof[type]);

        internal static object CastToCommon(TwType type, object value)
        {
            switch (type)
            {
                case TwType.Bool:
                    return (object)(bool)(TwBool)value;
                case TwType.Fix32:
                    return (object)(float)(TwFix32)value;
                case TwType.Frame:
                    return (object)(RectangleF)(TwFrame)value;
                case TwType.Str32:
                case TwType.Str64:
                case TwType.Str128:
                case TwType.Str255:
                case TwType.Str1024:
                case TwType.Uni512:
                    return (object)value.ToString();
                default:
                    return value;
            }
        }

        internal static object CastToTw(TwType type, object value)
        {
            switch (type)
            {
                case TwType.Bool:
                    return (object)(TwBool)(bool)value;
                case TwType.Fix32:
                    return (object)(TwFix32)(float)value;
                case TwType.Frame:
                    return (object)(TwFrame)(RectangleF)value;
                case TwType.Str32:
                    return (object)(TwStr32)value.ToString();
                case TwType.Str64:
                    return (object)(TwStr64)value.ToString();
                case TwType.Str128:
                    return (object)(TwStr128)value.ToString();
                case TwType.Str255:
                    return (object)(TwStr255)value.ToString();
                case TwType.Str1024:
                    return (object)(TwStr1024)value.ToString();
                case TwType.Uni512:
                    return (object)(TwUni512)value.ToString();
                default:
                    Type type1 = value.GetType();
                    return type1.IsEnum && Enum.GetUnderlyingType(type1) == TwTypeHelper.TypeOf(type) ? Convert.ChangeType(value, Enum.GetUnderlyingType(type1)) : value;
            }
        }

        internal static object ValueToTw<T>(TwType type, T value)
        {
            int num1 = Marshal.SizeOf(typeof(T));
            IntPtr num2 = Marshal.AllocHGlobal(num1);
            Twain32._Memory.ZeroMemory(num2, (IntPtr)num1);
            try
            {
                Marshal.StructureToPtr((object)value, num2, true);
                return Marshal.PtrToStructure(num2, TwTypeHelper.TypeOf(type));
            }
            finally
            {
                Marshal.FreeHGlobal(num2);
            }
        }

        internal static T ValueFromTw<T>(object value)
        {
            int num1 = Math.Max(Marshal.SizeOf(typeof(T)), Marshal.SizeOf(value));
            IntPtr num2 = Marshal.AllocHGlobal(num1);
            Twain32._Memory.ZeroMemory(num2, (IntPtr)num1);
            try
            {
                Marshal.StructureToPtr(value, num2, true);
                return (T)Marshal.PtrToStructure(num2, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(num2);
            }
        }
    }
}
