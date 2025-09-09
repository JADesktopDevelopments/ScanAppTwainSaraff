namespace WinFormsScanSaraff.Twain
{
    public enum TwRC : ushort
    {
        Success,
        Failure,
        CheckStatus,
        Cancel,
        DSEvent,
        NotDSEvent,
        XferDone,
        EndOfList,
        InfoNotSupported,
        DataNotAvailable,
        Busy,
        ScannerLocked,
    }
}
