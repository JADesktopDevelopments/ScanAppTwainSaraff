using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("{Message}; ReturnCode = {ReturnCode}; ConditionCode = {ConditionCode}")]
    [Serializable]
    public sealed class TwainException : Exception
    {
        internal TwainException(TwCC cc, TwRC rc)
          : this(TwainException._CodeToMessage(cc))
        {
            this.ConditionCode = cc;
            this.ReturnCode = rc;
        }

        internal TwainException(string message)
          : base(message)
        {
        }

        internal TwainException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        internal TwainException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            this.ConditionCode = (TwCC)info.GetValue(nameof(ConditionCode), typeof(TwCC));
            this.ReturnCode = (TwRC)info.GetValue(nameof(ReturnCode), typeof(TwRC));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ConditionCode", (object)this.ConditionCode);
            info.AddValue("ReturnCode", (object)this.ReturnCode);
        }

        public TwCC ConditionCode { get; private set; }

        public TwRC ReturnCode { get; private set; }

        private static string _CodeToMessage(TwCC code)
        {
            switch (code)
            {
                case TwCC.Success:
                    return "It worked!";
                case TwCC.Bummer:
                    return "Failure due to unknown causes.";
                case TwCC.LowMemory:
                    return "Not enough memory to perform operation.";
                case TwCC.NoDS:
                    return "No Data Source.";
                case TwCC.MaxConnections:
                    return "DS is connected to max possible applications.";
                case TwCC.OperationError:
                    return "DS or DSM reported error, application shouldn't.";
                case TwCC.BadCap:
                    return "Unknown capability.";
                case TwCC.BadProtocol:
                    return "Unrecognized MSG DG DAT combination.";
                case TwCC.BadValue:
                    return "Data parameter out of range.";
                case TwCC.SeqError:
                    return "DG DAT MSG out of expected sequence.";
                case TwCC.BadDest:
                    return "Unknown destination Application/Source in DSM_Entry.";
                case TwCC.CapUnsupported:
                    return "Capability not supported by source.";
                case TwCC.CapBadOperation:
                    return "Operation not supported by capability.";
                case TwCC.CapSeqError:
                    return "Capability has dependancy on other capability.";
                case TwCC.Denied:
                    return "File System operation is denied (file is protected).";
                case TwCC.FileExists:
                    return "Operation failed because file already exists.";
                case TwCC.FileNotFound:
                    return "File not found.";
                case TwCC.NotEmpty:
                    return "Operation failed because directory is not empty.";
                case TwCC.PaperJam:
                    return "The feeder is jammed.";
                case TwCC.PaperDoubleFeed:
                    return "The feeder detected multiple pages.";
                case TwCC.FileWriteError:
                    return "Error writing the file (meant for things like disk full conditions).";
                case TwCC.CheckDeviceOnline:
                    return "The device went offline prior to or during this operation.";
                default:
                    return "Unknown error.";
            }
        }
    }
}
