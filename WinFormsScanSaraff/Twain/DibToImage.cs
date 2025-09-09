using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WinFormsScanSaraff.Twain
{
    internal sealed class DibToImage
    {
        private const int BufferSize = 262144;

        public static Stream WithStream(IntPtr dibPtr, IStreamProvider provider)
        {
            Stream output = provider != null ? provider.GetStream() : (Stream)new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(output);
            DibToImage.BITMAPINFOHEADER structure = (DibToImage.BITMAPINFOHEADER)Marshal.PtrToStructure(dibPtr, typeof(DibToImage.BITMAPINFOHEADER));
            int num1 = 0;
            if (structure.biCompression == 0)
            {
                int num2 = structure.biWidth * (int)structure.biBitCount >> 3;
                num1 = Math.Max(structure.biHeight * (num2 + ((num2 & 3) != 0 ? 4 - num2 & 3 : 0)) - structure.biSizeImage, 0);
            }
            int num3 = structure.biSize + structure.biSizeImage + num1 + (structure.ClrUsed << 2);
            binaryWriter.Write((ushort)19778);
            binaryWriter.Write(14 + num3);
            binaryWriter.Write(0);
            binaryWriter.Write(14 + structure.biSize + (structure.ClrUsed << 2));
            byte[] numArray = new byte[262144];
            int num4 = 0;
            int num5;
            for (; num4 < num3; num4 += num5)
            {
                num5 = Math.Min(262144, num3 - num4);
                Marshal.Copy((IntPtr)(dibPtr.ToInt64() + (long)num4), numArray, 0, num5);
                binaryWriter.Write(numArray, 0, num5);
            }
            return output;
        }

        public static Stream WithStream(IntPtr dibPtr)
        {
            return DibToImage.WithStream(dibPtr, (IStreamProvider)null);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private class BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;

            public int ClrUsed
            {
                get => !this.IsRequiredCreateColorTable ? this.biClrUsed : 1 << (int)this.biBitCount;
            }

            public bool IsRequiredCreateColorTable => this.biClrUsed == 0 && this.biBitCount <= (short)8;
        }
    }
}
