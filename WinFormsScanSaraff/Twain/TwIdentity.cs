using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{ProductName}, Version = {Version.Info}")]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TwIdentity
    {
        public uint Id;
        public TwVersion Version;
        public ushort ProtocolMajor;
        public ushort ProtocolMinor;
        [MarshalAs(UnmanagedType.U4)]
        public TwDG SupportedGroups;
        public TwStr32 Manufacturer;
        public TwStr32 ProductFamily;
        public TwStr32 ProductName;

        public override bool Equals(object obj)
        {
            return obj != null && obj is TwIdentity && (int)((TwIdentity)obj).Id == (int)this.Id;
        }

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
