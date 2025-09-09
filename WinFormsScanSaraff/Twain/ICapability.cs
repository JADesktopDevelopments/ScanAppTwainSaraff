using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    public interface ICapability<T>
    {
        Twain32.Enumeration Get();

        T GetCurrent();

        T GetDefault();

        void Set(T value);

        void SetConstraint(T value);

        void SetConstraint(params T[] value);

        void SetConstraint(Twain32.Range value);

        void SetConstraint(Twain32.Enumeration value);

        void Reset();

        TwQC IsSupported();

        bool IsSupported(TwQC operation);
    }
}
