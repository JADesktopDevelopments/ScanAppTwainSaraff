using System.Runtime.InteropServices;

namespace WinFormsScanSaraff.Twain
{
    [return: MarshalAs(UnmanagedType.U2)]
    internal delegate TwRC CallBackProc(
      TwIdentity srcId,
      TwIdentity appId,
      [MarshalAs(UnmanagedType.U4)] TwDG dg,
      [MarshalAs(UnmanagedType.U2)] TwDAT dat,
      [MarshalAs(UnmanagedType.U2)] TwMSG msg,
      IntPtr data);
}
