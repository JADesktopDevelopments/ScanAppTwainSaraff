using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwImageMemXfer
    {
        [MarshalAs(UnmanagedType.U2)]
        public TwCompression Compression;
        public uint BytesPerRow;
        public uint Columns;
        public uint Rows;
        public uint XOffset;
        public uint YOffset;
        public uint BytesWritten;
        public TwMemory Memory;

        public TwImageMemXfer()
        {
            this.Compression = ~TwCompression.None;
            this.BytesPerRow = this.BytesWritten = this.Columns = this.Rows = this.XOffset = this.YOffset = uint.MaxValue;
        }
    }
}
